using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using RazorEngine.Compilation;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using WebApiContrib.Formatting.Html;

namespace Mimic.Web.WebApi
{
    public class RazorViewParser : IViewParser
    {
        private readonly IRazorEngineService _razorEngineService;

        public RazorViewParser(IViewLocator viewLocator, string basePath)
        {
            _razorEngineService = RazorEngineService.Create(new TemplateServiceConfiguration
            {
                BaseTemplateType = typeof(RazorViewPage),
                TemplateManager = new ViewTemplateManager(viewLocator, basePath),
                ReferenceResolver = new MimicReferenceResolver(),
                DisableTempFileLocking = true,
                CachingProvider = new DefaultCachingProvider(t => { }), // Disable warnings
                CompilerServiceFactory = new MimicCompilerServiceFactory() // Override default temp folder
            });
        }

        public byte[] ParseView(IView view, string viewTemplate, Encoding encoding)
        {
            var parsedView = GetParsedView(view, viewTemplate);
            return encoding.GetBytes(parsedView);
        }

        protected string GetParsedView(IView view, string viewTemplate)
        {
            _razorEngineService.Compile(viewTemplate, view.ViewName);

            var dynamicModel = RazorDynamicObject.Create(view.Model);

            return _razorEngineService.Run(view.ViewName, null, dynamicModel);
        }
    }
}
