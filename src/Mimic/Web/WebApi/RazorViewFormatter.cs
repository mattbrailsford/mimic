using RazorEngine.Templating;
using WebApiContrib.Formatting.Html;
using WebApiContrib.Formatting.Html.Formatting;

namespace Mimic.Web.WebApi
{
    public sealed class RazorViewFormatter : HtmlMediaTypeViewFormatter
    {
        public RazorViewFormatter(string siteRootPath = null, IViewLocator viewLocator = null, IViewParser viewParser = null)
          : base(
                siteRootPath, 
                viewLocator ?? new RazorViewLocator(), 
                viewParser ?? new RazorViewParser(viewLocator ?? new RazorViewLocator(), siteRootPath)
            )
        { }
    }
}
