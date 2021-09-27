using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Trace;

namespace OpenTelemetry.BackAPI
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
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "OpenTelemetry.BackAPI", Version = "v1" });
            });

            services.AddOpenTelemetryTracing(builder =>
            {
                builder.Configure((sp, builder) =>
                {
                    var isDevelopment = sp.GetRequiredService<IWebHostEnvironment>().IsDevelopment();
                    var ingorePaths = new[] { "/swagger", "/favicon.ico", "/serviceworker" };

                    builder
                        //.AddSource("OpenTelemetry.FrontMVC")
                        //.AddSource(DbLoggerCategory.Name) //"Microsoft.EntityFrameworkCore"

                        .AddAspNetCoreInstrumentation(p => p.Filter = http => ingorePaths.Any(path => http.Request.Path.StartsWithSegments(path)) is false)
                        .AddHttpClientInstrumentation()
                        //.AddSqlClientInstrumentation(/*p => p.SetDbStatementForText = true*/)
                        .AddEntityFrameworkCoreInstrumentation(/*p => p.SetDbStatementForText = true*/)
                        .SetErrorStatusOnException()

                        .SetSampler(new TraceIdRatioBasedSampler(isDevelopment ? 1.0 : 0.5))
                        .AddConsoleExporter()
                        .AddJaegerExporter()
                        .AddZipkinExporter();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenTelemetry.BackAPI v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
