using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SigOpsMetrics.API.Classes;

namespace SigOpsMetrics.API
{
    public static class CacheProfiles
    {
        public const string Default = "Default";
    }

    public class Startup
    {
        private const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                    builder =>
                    {
                        //Add future domains here
                        builder.WithOrigins("http://localhost:4200");
                    });
            });
            services.AddResponseCaching();
            services.AddMvc(options =>
            {
                options.CacheProfiles.Add(CacheProfiles.Default, new CacheProfile()
                {
                    Duration = 43200 //12 hr
                });
            });
            services.AddControllers();

            //Dependency injection
            services.Configure<AppConfig>(Configuration.GetSection("AppConfig"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(MyAllowSpecificOrigins);

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
