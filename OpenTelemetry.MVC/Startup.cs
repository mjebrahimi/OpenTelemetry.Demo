using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Exporter.Prometheus;
using OpenTelemetry.FrontMVC.Models;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Trace;

namespace OpenTelemetry.FrontMVC
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
            services.AddDbContext<AppDbConext>(opt =>
            {
                opt
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
                    .UseSqlServer("Data Source=.;Initial Catalog=TestDb;Integrated Security=true");
            });

            services.AddControllersWithViews();

            services.AddOpenTelemetryTracing(builder =>
            {
                //Configuration can place outside of Configure but within this web can use ServiceProvider
                builder.Configure((sp, builder) =>
                {
                    var isDevelopment = sp.GetRequiredService<IWebHostEnvironment>().IsDevelopment();
                    var ingorePaths = new[] { "/lib", "/js", "/css", "/favicon.ico", "/serviceworker" };

                    builder
                        .AddSource("OpenTelemetry.FrontMVC")

                        .AddAspNetCoreInstrumentation(p => p.Filter = http => ingorePaths.Any(path => http.Request.Path.StartsWithSegments(path)) is false)
                        .AddHttpClientInstrumentation()
                        //.AddSqlClientInstrumentation(/*p => p.SetDbStatementForText = true*/)
                        .AddEntityFrameworkCoreInstrumentation(/*p => p.SetDbStatementForText = true*/)
                        //.AddRedisInstrumentation(connectionMultiplexer)
                        .SetErrorStatusOnException()

                        .SetSampler(new TraceIdRatioBasedSampler(isDevelopment ? 1.0 : 0.5))
                        //https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/examples/Console/TestMetrics.cs
                        //https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/examples/Console/TestPrometheusExporter.cs
                        //.AddPrometheusExporter(opt => opt.Url = $"http://localhost:{port}/metrics/")
                        //https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/examples/Console/TestJaegerExporter.cs
                        //https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/examples/Console/TestZipkinExporter.cs
                        .AddConsoleExporter()
                        .AddJaegerExporter() //Default: "AgentHost": "localhost", "AgentPort": 6831
                        .AddZipkinExporter(); //Default: "http://localhost:9411/api/v2/spans"

                    //https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/examples/Console/TestZPagesExporter.cs
                    //var zpagesOptions = new ZPagesExporterOptions() { Url = "http://localhost:7284/rpcz/", RetentionTime = 3600000 };
                    //var zpagesExporter = new ZPagesExporter(zpagesOptions);
                    //var httpServer = new ZPagesExporterStatsHttpServer(zpagesExporter);

                    //// Start the server
                    //httpServer.Start();
                    //builder.AddZPagesExporter(o =>
                    // {
                    //     o.Url = zpagesOptions.Url;
                    //     o.RetentionTime = zpagesOptions.RetentionTime;
                    // })
                });

                // For options which can be bound from IConfiguration.
                //services.Configure<ZipkinExporterOptions>(Configuration.GetSection("Zipkin"));
                //services.Configure<JaegerExporterOptions>(Configuration.GetSection("Jaeger"));
                //services.Configure<AspNetCoreInstrumentationOptions>(Configuration.GetSection("AspNetCoreInstrumentation"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            #region Seed Database
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbConext>();

                if (dbContext.Users.Any() is false)
                {
                    for (int i = 1; i <= 10; i++)
                    {
                        dbContext.Users.Add(new() { Name = $"User{i}" });
                    }
                    dbContext.SaveChanges();
                }
            }
            #endregion

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
