using System;
using Mimic.Web;
using Microsoft.Owin.Hosting;
using Mimic.IO;
using Mimic.Services;

namespace Mimic
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseUrl = "http://localhost:9000/";
            var basePath = "C:\\Users\\Matt\\Work\\Sandbox\\Mimic";

            // Setup initial context
            MimicContext.Current = new MimicContext
            {
                BaseUrl = baseUrl,
                BasePath = basePath,

                Services = new MimicServicesContext
                {
                    MimicService = new MimicService(new PhysicalFileSystem(basePath),
                        "~/sitemap.json",
                        "~/ViewModels/*.json")
                }
            };

            MimicContext.Current.Services.MimicService.LoadModelTemplates();

            // Setup file watchers on sitemap / models / views?

            // Start OWIN host 
            using (WebApp.Start<WebBootstrap>(url: baseUrl))
            {
                Console.WriteLine("Web API Self hosted on " + baseUrl + " Hit ENTER to exit...");
                Console.ReadLine();
            }
        }
    }
}
