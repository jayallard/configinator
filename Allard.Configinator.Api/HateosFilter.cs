using System;
using Allard.Configinator.Api.Commands.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Allard.Configinator.Api
{
    public class HateosFilter : IActionFilter
    {
        private readonly LinkHelper linkHelper;

        public HateosFilter(LinkHelper linkHelper)
        {
            this.linkHelper = linkHelper;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            Console.WriteLine();
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is not ObjectResult obj)
            {
                return;
            }

            if (obj.Value is RealmViewModel realm)
            {
                AddToRealm(realm, true);
                return;
            }

            if (obj.Value is RealmsViewModel realms)
            {
                realms
                    .Links = linkHelper
                    .CreateBuilder()
                    .AddRealms(true)
                    .Build();
                foreach (var r in realms.Realms)
                {
                    AddToRealm(r, false);
                }
                return;
            }
        }

        private void AddToRealm(RealmViewModel realm, bool self)
        {
            realm
                .Links = linkHelper
                .CreateBuilder()
                .AddRealm(realm.RealmName, self)
                .Build();
            foreach (var cs in realm.ConfigurationSections)
            {
                AddToConfigurationSection(cs, false);
            }
        }

        private void AddToConfigurationSection(ConfigurationSectionViewModel cs, bool self)
        {
            cs.Links = linkHelper
                .CreateBuilder()
                .AddConfigurationSection(cs.RealmId.Name, cs.ConfigurationSectionId.Name, self)
                .Build();
        }
    }
}