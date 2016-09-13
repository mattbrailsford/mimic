using System;
using System.Collections.Concurrent;
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
    public class ViewTemplateManager : ITemplateManager
    {
        private readonly ConcurrentDictionary<ITemplateKey, ITemplateSource> _dynamicTemplates = new ConcurrentDictionary<ITemplateKey, ITemplateSource>();

        private IViewLocator _viewLocator;
        private string _siteRootPath;

        public ViewTemplateManager(IViewLocator viewLocator, string siteRootPath)
        {
            _viewLocator = viewLocator;
            _siteRootPath = siteRootPath;
        }

        protected string Resolve(string name)
        {
            // Remove the file extension if there is one
            if (name.Contains("."))
            {
                name = name.Substring(0, name.LastIndexOf(".", StringComparison.InvariantCulture));
            }

            return _viewLocator.GetView(_siteRootPath, new TempView { ViewName = name });
        }

        public ITemplateSource Resolve(ITemplateKey key)
        {
            ITemplateSource templateSource;
            if (_dynamicTemplates.TryGetValue(key, out templateSource))
                return templateSource;

            return new LoadedTemplateSource(Resolve(key.Name));
        }

        public ITemplateKey GetKey(string name, ResolveType resolveType, ITemplateKey context)
        {
            return new NameOnlyTemplateKey(name, resolveType, context);
        }

        public void AddDynamic(ITemplateKey key, ITemplateSource source)
        {
            _dynamicTemplates.AddOrUpdate(key, source, (k, oldSource) =>
            {
                //if (oldSource.Template != source.Template)
                //    throw new InvalidOperationException("The same key was used for another template!");

                return source;
            });
        }

        internal class TempView : IView
        {
            public object Model { get; internal set; }

            public Type ModelType { get; internal set; }

            public string ViewName { get; internal set; }
        }
    }
}
