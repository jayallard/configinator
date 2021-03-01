using System.IO;
using Allard.Configinator.Configuration;
using Allard.Configinator.Habitats;
using Allard.Configinator.Realms;
using Allard.Configinator.Schema;
using Allard.Configinator.Schema.Validator;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Allard.Configinator.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // const string DataFolder =
            //     @"/home/jaya/personal/projects/configinator/Allard.Configinator.Tests/TestFiles/FullSetup";
            const string DataFolder =
                @"/Users/jallard/personal/ConfigurationManagement/Allard.Configinator.Tests/TestFiles/FullSetup";

            services.AddControllers();

            // configinator!
            services
                .AddSingleton<Configinator>()
                .AddSingleton<IConfigStore, MemoryConfigStore>()
                .AddSingleton<IHabitatService, HabitatService>()
                .AddSingleton<IHabitatRepository>(
                    new HabitatsRepositoryYamlFile(Path.Combine(DataFolder, "habitats.yml")))
                .AddSingleton<IRealmService, RealmService>()
                .AddSingleton<IRealmRepository>(new RealmRepositoryYamlFiles(DataFolder))
                .AddSingleton<ISchemaValidator, SchemaValidator>()
                .AddSingleton<ISchemaService, SchemaService>()
                .AddSingleton<ITypeValidatorFactory, ValidatorFactoryServices>()
                .AddSingleton<ISchemaRepository>(new SchemaRepositoryYamlFiles(DataFolder));

            // MediatR
            services.AddMediatR(typeof(Startup).Assembly);

            // Link builder - used to construct links in the responses
            services.AddSingleton<LinkHelper>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Allard.Configinator.Api", Version = "v1"});
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Allard.Configinator.Api v1"));
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}