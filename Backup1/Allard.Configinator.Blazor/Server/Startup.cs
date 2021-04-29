using Allard.Configinator.Blazor.Shared;
using Allard.Configinator.Core;
using Allard.Configinator.Core.Infrastructure;
using Allard.Configinator.Infrastructure.MongoDb;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Allard.Configinator.Blazor.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var repo = new OrganizationRepositoryMongo();
            services
                .AddSingleton<IConfigStore, MemoryConfigStore>()
                .AddSingleton<IConfiginatorService, ConfiginatorService>()
                .AddSingleton<IOrganizationRepository>(repo)
                .AddSingleton<IOrganizationQueries>(repo)
                .AddTransient<IActionFilter, HateosFilter>();

            // MediatR
            services.AddMediatR(typeof(Startup).Assembly);

            // Link builder - used to construct links in the responses
            services.AddSingleton<LinkHelper>();

            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}