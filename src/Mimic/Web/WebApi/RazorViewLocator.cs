using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using WebApiContrib.Formatting.Html;

namespace Mimic.Web.WebApi
{
    public class RazorViewLocator : IViewLocator
    {
        protected readonly string[] _viewLocationFormats = {
            "~\\Views\\{0}.cshtml",
            "~\\Views\\{0}.vbhtml",
            "~\\Views\\Partials\\{0}.cshtml",
            "~\\Views\\Partials\\{0}.vbhtml",
            "~\\Views\\Shared\\{0}.cshtml",
            "~\\Views\\Shared\\{0}.vbhtml",
            "~\\Views\\Shared\\Partials\\{0}.cshtml",
            "~\\Views\\Shared\\Partials]]{0}.vbhtml"
        };

        public RazorViewLocator()
        { }

        public RazorViewLocator(IEnumerable<string> additionalViewLocations)
        {
            _viewLocationFormats = _viewLocationFormats.Union(additionalViewLocations).ToArray();
        }

        public string GetView(string siteRootPath, IView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var physicalSiteRootPath = GetPhysicalSiteRootPath(siteRootPath);
            foreach (var str in _viewLocationFormats)
            {
                var path = string.Format(str.Replace("~", GetPhysicalSiteRootPath(siteRootPath)), (object)view.ViewName);

                if (File.Exists(path))
                    return File.ReadAllText(path);
            }

            throw new FileNotFoundException(string.Format("Can't find a view with the name '{0}.cshtml' or '{0}.vbhtml in the '\\Views' folder under  path '{1}'", (object)view.ViewName, (object)physicalSiteRootPath));
        }

        internal string GetPhysicalSiteRootPath(string siteRootPath)
        {
            if (string.IsNullOrWhiteSpace(siteRootPath))
            {
                var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
                if (directoryName != null)
                {
                    return directoryName.Replace("file:\\", string.Empty)
                        .Replace("\\bin", string.Empty)
                        .Replace("\\Debug", string.Empty)
                        .Replace("\\Release", string.Empty);
                }
            }

            return siteRootPath;
        }
    }
}
