using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;

namespace OpenTelemetry.FrontMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                //https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/examples/AspNetCore/Program.cs
                //.ConfigureLogging((context, builder) =>
                //{
                //    builder.ClearProviders();
                //    builder.AddConsole();
                //    var useLogging = context.Configuration.GetValue<bool>("UseLogging");
                //    if (useLogging)
                //    {
                //        builder.AddOpenTelemetry(options =>
                //        {
                //            options.IncludeScopes = true;
                //            options.ParseStateValues = true;
                //            options.IncludeFormattedMessage = true;
                //            options.AddConsoleExporter();
                //        });
                //    }
                //})
                ;
    }
}
