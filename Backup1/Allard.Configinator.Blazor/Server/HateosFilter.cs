using System;
using Allard.Configinator.Blazor.Shared;
using Allard.Configinator.Blazor.Shared.ViewModels.Organization;
using Allard.Configinator.Core.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Allard.Configinator.Blazor.Server
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
            if (context.Result is not ObjectResult obj) return;

            switch (obj.Value)
            {
                case RealmViewModel realm:
                    AddToRealm(realm, true);
                    return;
                case OrganizationViewModel realms:
                {
                    realms
                        .Links = linkHelper
                        .CreateBuilder()
                        .AddRealms(true)
                        .Build();
                    foreach (var r in realms.Realms) AddToRealm(r, false);
                    break;
                }
            }
        }

        private void AddToRealm(RealmViewModel realm, bool self)
        {
            realm
                .Links = linkHelper
                .CreateBuilder()
                .AddRealm(realm.RealmId, self)
                .Build();
            foreach (var cs in realm.ConfigurationSections) AddToConfigurationSection(cs, false);
        }

        private void AddToConfigurationSection(ConfigurationSectionViewModel cs, bool self)
        {
            cs.Links = linkHelper
                .CreateBuilder()
                .AddConfigurationSection(new RealmId(cs.RealmId), new SectionId(cs.SectionId), self)
                .Build();
        }
    }
}