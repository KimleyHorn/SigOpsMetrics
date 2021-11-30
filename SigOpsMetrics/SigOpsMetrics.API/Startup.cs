using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MySqlConnector;
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
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
            });
            services.AddCors(options =>
            {
                options.AddPolicy("AnyOrigin", builder => { builder.AllowAnyOrigin().AllowAnyMethod(); });
                options.AddPolicy(name: MyAllowSpecificOrigins,
                    builder =>
                    {
                        //Add future domains here
                        builder.WithOrigins("http://localhost:4200", "http://sigops-test.s3-website-us-east-1.amazonaws.com", "http://new.sigopsmetrics.com").AllowAnyMethod().AllowAnyHeader();
                    });
            });
            //services.AddResponseCaching();
            services.AddMvc(options =>
            {
                options.CacheProfiles.Add(CacheProfiles.Default, new CacheProfile()
                {
                    Duration = 43200 //12 hr
                });
            });
            services.AddControllers().AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );

            //Dependency injection
            services.Configure<AppConfig>(Configuration.GetSection("AppConfig"));
            services.AddTransient<MySqlConnection>(_ =>
                new MySqlConnection(Configuration["ConnectionStrings:Default"]));
            services.AddMemoryCache();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "SigOps Metrics API", Version = "v1", Description = ApiDescription.Description});

                var filePath = Path.Combine(System.AppContext.BaseDirectory, "SigOpsMetrics.API.xml");
                c.IncludeXmlComments(filePath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseResponseCompression();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(MyAllowSpecificOrigins);

            //app.UseResponseCaching();

            //app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            app.UseSwagger();

            
#if DEBUG

            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "SigOps Metrics API V1"); });
#else

            app.UseSwaggerUI(c => { c.SwaggerEndpoint(Environment.GetEnvironmentVariable("SwaggerPath"), "SigOps Metrics API V1"); });

#endif
        }
    }
}
