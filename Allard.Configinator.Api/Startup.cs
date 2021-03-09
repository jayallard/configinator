using Allard.Configinator.Core;
using Allard.Configinator.Core.Infrastructure;
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
                .AddSingleton<IConfigStore, MemoryConfigStore>()
                .AddSingleton<IConfiginatorService, ConfiginatorService>();

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