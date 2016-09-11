using System;
using System.Text;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using WebApiContrib.Formatting.Html;

namespace Mimic.Web.WebApi
{
    public class RazorViewParser : IViewParser
    {
        private readonly ITemplateService _templateService;

        public RazorViewParser(ITemplateService templateService)
        {
            if (templateService == null)
                throw new ArgumentNullException(nameof(templateService));

            _templateService = templateService;
        }

        public RazorViewParser(ITemplateServiceConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            _templateService = new TemplateService(config);
        }

        public RazorViewParser(ITemplateResolver resolver, Type baseTemplateType)
          : this(new TemplateServiceConfiguration {
              BaseTemplateType = baseTemplateType,
              Resolver = resolver 
          })
        { }

        public byte[] ParseView(IView view, string viewTemplate, Encoding encoding)
        {
            var parsedView = this.GetParsedView(view, viewTemplate);
            return encoding.GetBytes(parsedView);
        }

        protected string GetParsedView(IView view, string viewTemplate)
        {
            _templateService.Compile(viewTemplate, view.ModelType, view.ViewName);
            return _templateService.Run(view.ViewName, view.Model, null);
        }
    }
}
