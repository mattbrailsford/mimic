using System.Linq;
using Mimic.Extensions;
using Mimic.IO;
using Newtonsoft.Json.Linq;

namespace Mimic.Services
{
    public class MimicService
    {
        private IFileSystem _fileSystem;
        private ModelsService _modelsService;

        private string _sitemapPath;
        private string _viewModelsPath;

        public MimicService(IFileSystem fileSystem, string sitemapPath, string viewModelsPath)
        {
            _fileSystem = fileSystem;
            _modelsService = new ModelsService();

            _sitemapPath = sitemapPath;
            _viewModelsPath = viewModelsPath;
        }

        internal void LoadModelTemplates()
        {
            // Clear previous models
            _modelsService.ClearModelTemplates();

            // Register the sitemap first
            var sitemapName = _fileSystem.GetFileNameWithoutExtension(_sitemapPath).MakeAliasSafe();
            var sitemapJson = _fileSystem.GetFileContents(_sitemapPath);

            _modelsService.RegisterModelTemplate(sitemapName, sitemapJson);

            // Create the sitemap model
            var sitemap = _modelsService.GenerateModel(sitemapName);

            // Populate url properties
            PopulateSitemapUrls(sitemap);

            // Get list of view models templates
            var templates = _fileSystem.GetFiles(_viewModelsPath);

            // Load up the templates
            foreach (var template in templates)
            {
                var name = _fileSystem.GetFileNameWithoutExtension(template).MakeAliasSafe();
                var json = _fileSystem.GetFileContents(template);

                //TODO: Generate in member model classes

                _modelsService.RegisterModelTemplate(name, json, sitemap);
            }

            // Update the sitemap in the mimic context
            MimicContext.Current.Sitemap = sitemap;
        }

        public bool HasModelTemplate(string templateName)
        {
            return _modelsService.HasModelTemplate(templateName);
        }

        public JObject GenerateModel(string templateName, JObject data = null)
        {
            return _modelsService.GenerateModel(templateName, data);
        }

        public JObject GetCurrentNodeByUrl(string url, bool updateContext = false)
        {
            var curentNode = MimicContext.Current.Sitemap;

            if (!string.IsNullOrWhiteSpace(url))
            {
                var urlJsonPath = UrlToJsonPath(url);
                curentNode = (JObject)MimicContext.Current.Sitemap.SelectToken(urlJsonPath);
            }

            if (updateContext)
            {
                MimicContext.Current.CurrentPage = curentNode;
            }

            return curentNode;
        }

        protected void PopulateSitemapUrls(JContainer container, string path = "")
        {
            var jArray = container as JArray;
            if (jArray != null)
            {
                foreach (var item in jArray)
                {
                    var itemContainer = item as JContainer;
                    if (itemContainer != null)
                    {
                        PopulateSitemapUrls(itemContainer, path);
                    }
                }
            }

            var jObj = container as JObject;
            if (jObj != null)
            {
                var aliasProp = jObj["alias"].Value<string>();

                path = (path + "/" + aliasProp).TrimStart('/');

                jObj.Add("url", path);

                foreach (var prop in jObj.Properties())
                {
                    var propContainer = prop.Value as JContainer;
                    if (propContainer != null)
                    {
                        PopulateSitemapUrls(propContainer, path);
                    }
                }
            }
        }

        protected string UrlToJsonPath(string url)
        {
            return url.Split('/').Aggregate("$", (current, currentUrlPart) => current + (".pages[?(@.alias == '" + currentUrlPart + "')]"));
        }
    }
}
