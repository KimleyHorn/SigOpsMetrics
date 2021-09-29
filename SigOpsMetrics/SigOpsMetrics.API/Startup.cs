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
        public const string ApiDescription = "<h2>API for downloading data from SigOpsMetrics.com</h2><br><p>This API allows for the download of the data on the SigOpsMetrics.com site to a comma-separated value (csv) file format, which can be opened in Excel. This page explains how to structure a query as a url. It also provides an interactive way to use the API to download data.</p><br><p><strong>Measure Definitions</strong> for use in query (see below):</p><br><ul><li><strong>aogd</strong> - Arrivals on Green(%)</li><li><strong>aogh</strong> - Arrivals on Green by hour(%)</li><li><strong>bi</strong> - Buffer Index</li><li><strong>bih</strong> - Buffer Index by hour</li><li><strong>cctv</strong> - CCTV Uptime (%)</li><li><strong>cu</strong> - Communications Uptime(%)</li><li><strong>du</strong> - Detector Uptime(%)</li><li><strong>mttr</strong> - Mean Time to Respond, TEAMS Tasks(days)</li><li><strong>outstanding</strong> - Tasks Outstanding, TEAMS</li><li><strong>over45</strong> - Tasks Outstanding Over 45 Days, TEAMS</li><li><strong>papd</strong> - Pedestrian Actuations Per Day</li><li><strong>pau</strong> - Pedestrian Actuation/Pushbutton Uptime (%)</li><li><strong>pd</strong> - Pedestrian Delay(minutes)</li><li><strong>prd</strong> - Progression Radio</li><li><strong>prh</strong> - Progression Ratio by hour</li><li><strong>pti</strong> - Planning Time Index</li><li><strong>ptih</strong> - Hourly Planning Time Index</li><li><strong>qsd</strong> - Queue Spillback Rate (%)</li><li><strong>qsh</strong> - Queue Spillback Rate by hour(%)</li><li><strong>reported</strong> - Tasks Reported, TEAMS</li><li><strong>resolved</strong> - Tasks Resolved, TEAMS</li><li><strong>sfd</strong> - Split Failure Rate(%)</li><li><strong>sfh</strong> - Split Failure Rate by Hour(%)</li><li><strong>sfo</strong> - Off-Peak Split Failure Rate by day(%)</li><li><strong>spd</strong> - Average Speed, HERE (mph)</li><li><strong>spdh</strong> - Average Speed by hour, HERE (mph)</li><li><strong>tp</strong> - Vehicular Throughput(veh/hr)</li><li><strong>tpri</strong> - Tasks by Priority, TEAMS</li><li><strong>tsou</strong> - Tasks by Source, TEAMS</li><li><strong>tsub</strong> - Tasks by Subcategory, TEAMS</li><li><strong>tti</strong> - Travel Time Index</li><li><strong>ttih</strong> - Hourly Travel Time Index</li><li><strong>ttyp</strong> - Tasks by Type, TEAMS</li><li><strong>vpd</strong> - Vehicular Traffic Volume (veh/day)</li><li><strong>vphpa</strong> - AM Peak Vehicular Traffic Volume(veh/hr)</li><li><strong>vphpp</strong> - PM Peak Vehicular Traffic Volume(veh/hr)</li></ul>";
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
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "SigOps Metrics API", Version = "v1", Description = ApiDescription});

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

            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/dev/swagger/v1/swagger.json", "SigOps Metrics API V1"); });

#endif
        }
    }
}
