using System;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Configuration;
using Allard.Configinator.Habitats;
using Allard.Configinator.Namespaces;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator
{
    public class Configinator
    {
        /// <summary>
        ///     Work with the storage of configuration values.
        /// </summary>
        private readonly IConfigStore configStore;

        /// <summary>
        ///     Habit configuration store.
        /// </summary>
        private readonly IHabitatService habitatService;
        
        public Configinator(
            IConfigStore configStore,
            IHabitatService habitatService,
            INamespaceService namespaceService)
        {
            this.configStore = configStore.EnsureValue(nameof(configStore));
            this.habitatService = habitatService.EnsureValue(nameof(habitatService));
            Habitats = new HabitatsAccessor(this.habitatService);
            Namespaces = new NamespaceAccessor(namespaceService.EnsureValue(nameof(namespaceService)));

            var setter = new Func<ConfigurationSectionValue, Task>(SetValueAsync);
            var getter = new Func<ConfigurationId, Task<ConfigurationSectionValue>>(GetValueAsync);
            Configuration = new ConfigurationAccessor(getter, setter);
        }

        public HabitatsAccessor Habitats { get; }
        public NamespaceAccessor Namespaces { get; }

        public ConfigurationAccessor Configuration { get; }

        private async Task<string> GetPathAsync(ConfigurationId id)
        {
            var ns = await Namespaces.ByName(id.Namespace).ConfigureAwait(false);
            var cs = ns.GetConfigurationSection(id.ConfigurationSection);
            var habitat = await habitatService.GetHabitatAsync(id.Habitat).ConfigureAwait(false);
            return cs.Path.Replace("{{habitat}}", habitat.Name);
        }

        private async Task SetValueAsync(ConfigurationSectionValue value)
        {
            value.EnsureValue(nameof(value));
            var path = await GetPathAsync(value.Id).ConfigureAwait(false);
            await configStore.SetValueAsync(new ConfigurationValue(path, value.Etag, value.Value))
                .ConfigureAwait(false);
        }

        private async Task<ConfigurationSectionValue> GetValueAsync(ConfigurationId id)
        {
            id.EnsureValue(nameof(id));
            var habitat = await habitatService.GetHabitatAsync(id.Habitat).ConfigureAwait(false);

            // --------------------------------------------------
            // get the values from the base paths.
            // --------------------------------------------------
            var baseValues = habitat
                .Bases
                .Select(async baseHabitat =>
                {
                    var baseId = new ConfigurationId(baseHabitat, id.Namespace, id.ConfigurationSection);
                    return await GetValueAsync(baseId).ConfigureAwait(false);
                })
                .ToList();

            // --------------------------------------------------
            // get the value from the requested path,
            // then wait for the base queries to finish.
            // --------------------------------------------------
            var value = await configStore.GetValueAsync(await GetPathAsync(id).ConfigureAwait(false));
            await Task.WhenAll(baseValues).ConfigureAwait(false);

            // --------------------------------------------------
            // put them all together, then merge.
            // --------------------------------------------------
            var all = baseValues
                .Where(b => b.Result?.Value != null)
                .Select(b => b.Result.Value)
                .ToList();
            if (value.Value != null) all.Add(value.Value);

            all.Reverse();

            var docs = all.Select(JToken.Parse).ToList();
            var final = new JsonMerger(docs).Merge()?.ToString();

            // TODO: etag only represents the bottom most layer - misleading. if top level changes,
            // then value is different, but etag is the same.
            // TODO: indicate that the value doesn't exist.
            return new ConfigurationSectionValue(id, value.ETag, final);
        }
    }
}