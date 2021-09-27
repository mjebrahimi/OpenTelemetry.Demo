using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenTelemetry.FrontMVC.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OpenTelemetry.FrontMVC.Controllers
{
    public class HomeController : Controller
    {
        private static readonly ActivitySource activitySource = new("OpenTelemetry.FrontMVC");
        private static readonly HttpClient httpClient = new();
        private readonly AppDbConext dbConext;

        public HomeController(AppDbConext dbConext)
        {
            this.dbConext = dbConext;
        }

        public async Task<IActionResult> Index()
        {
            //DoSomeWork();

            using (var activity = activitySource.StartActivity("GET:" + "https://localhost:44357/WeatherForecast", ActivityKind.Client))
            {
                activity?.AddEvent(new ActivityEvent("GetFromJsonAsync:Started"));

                await httpClient.GetFromJsonAsync<List<WeatherForecastDto>>("https://localhost:44357/WeatherForecast");

                activity?.AddEvent(new ActivityEvent("GetFromJsonAsync:Ended"));
            }

            await dbConext.Users.ToListAsync();

            DoSomeWork();

            return View();
        }

        public IActionResult Error()
        {
            throw new Exception("An error occurred");
        }

        private void DoSomeWork()
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;

            using (var activity = activitySource.StartActivity("DoWork1"))
            {
                DoWork1();
            }
        }

        private void DoWork1()
        {
            var x1 = Activity.Current?.TraceId;
            var x2 = Activity.Current?.SpanId;
            var x3 = Activity.Current?.Id; //TraceId+SpanId
            var x4 = Activity.Current?.ParentSpanId;
            var x5 = Activity.Current?.ParentId; //Parent TraceId+SpanId

            if (Activity.Current?.IsAllDataRequested is true)
            {
                Activity.Current?.AddEvent(new("An event occurred"));
                Activity.Current?.SetTag("TagKey", "TagValue");
            }

            //Activity.Current?.SetStatus(Status.Error.WithDescription("An error occurred"));
            Activity.Current?.SetStatus(ActivityStatusCode.Error, "An error occurred");

            Thread.Sleep(100);

            DoWork2();
        }

        private void DoWork2()
        {
            //activitySource.StartActivity($"{HttpContext.Request.Method}:{HttpContext.Request.GetDisplayUrl()}", ActivityKind.Server);
            using (var activity = activitySource.StartActivity("DoWork2"))
            {
                if (activity?.IsAllDataRequested is true)
                {
                    activity?.AddEvent(new("An event occurred"));
                    activity?.SetTag("TagKey", "TagValue");
                }
                Thread.Sleep(100);
            }
        }
    }
}
