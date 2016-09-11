using System.Web.Http;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Mimic.Web.Razor;
using Owin;
using WebApiContrib.Formatting.Razor;

namespace Mimic.Web
{
    public class WebBootstrap
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            var config = new HttpConfiguration();

            // Ignore any routes containing a file extension
            config.Routes.IgnoreRoute(
                "Assets",
                "{*asset}",
                new { asset = @".*(\.[0-9a-zA-Z]+)+($|\/|\?|\#)" });

            // Route all other (extensionless) urls to the mimic controller
            config.Routes.MapHttpRoute(
                "DefaultApi",
                "{*uri}",
                new { controller = "Mimic", action = "Get", uri = RouteParameter.Optional }
            );

            // Register the razor formatter
            var viewLocator = new RazorViewLocator();
            var viewParser = new RazorViewParser(new ViewTemplateResolver(viewLocator, MimicContext.Current.BasePath));
            config.Formatters.Add(new RazorViewFormatter(MimicContext.Current.BasePath, viewLocator, viewParser));

            // Configure the web api
            appBuilder.UseWebApi(config);

            // Setup static file routing
            var fileSystem = new PhysicalFileSystem(MimicContext.Current.BasePath);
            var staticOpts = new StaticFileOptions
            {
                FileSystem = fileSystem
            };

            appBuilder.UseStaticFiles(staticOpts);
        }
    }
}
