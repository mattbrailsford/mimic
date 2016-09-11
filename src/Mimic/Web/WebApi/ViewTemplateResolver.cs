using System;
using RazorEngine.Templating;
using WebApiContrib.Formatting.Html;

namespace Mimic.Web.WebApi
{
    /// <summary>
    /// The standard TemplateResovler doesn't look for view in the views folder
    /// so we create a custom template resovler that delgates the lookup to the
    /// view locator which does know how to search the view folder.
    /// This is probably still not quite right though as it should probably be
    /// location aware based on it's parent view, but this is the best we can
    /// achive without tweaking the RazorEngine code.
    /// </summary>
    public class ViewTemplateResolver : ITemplateResolver
    {
        private IViewLocator _viewLocator;
        private string _siteRootPath;

        public ViewTemplateResolver(IViewLocator viewLocator, string siteRootPath)
        {
            this._viewLocator = viewLocator;
            this._siteRootPath = siteRootPath;
        }

        public string Resolve(string name)
        {
            return this._viewLocator.GetView(this._siteRootPath, new TempView { ViewName = name.Substring(0, name.LastIndexOf(".")) });
        }

        internal class TempView: IView
        {
            public object Model { get; internal set; }

            public Type ModelType { get; internal set; }

            public string ViewName { get; internal set; }
        }
    }
}
