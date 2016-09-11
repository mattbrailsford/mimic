//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text.RegularExpressions;
//using Jint;
//using JsonHandlebars.Extensions;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

//namespace JsonHandlebars
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            // Declare variables
//            var currentUrl = "blog";

//            // Load JSON
//            var sitemapJson = File.ReadAllText("sitemap.json");
//            var modelJson = File.ReadAllText("mymodel.json");

//            // Transform JSON
//            var engine = new Engine(options =>
//            {
//                options.AddObjectConverter(new JValueObjectConverter());
//            });

//            using (var hb = new Chevron.Handlebars(engine))
//            {
//                // Register heleprs
//                hb.RegisterHelper("range", @"function(from, to, options){ 
//                    var data = options.data ? Handlebars.createFrame(options.data) : {};
//                    var out = """";
//                    for (var i = from; i <= to; i++) {
//                        data.first = i == from;
//                        data.last = i == to;
//                        data.index = i - from;
//                        out += options.fn(i, { data : data });
//                    }
//                    return out;
//                }");

//                // Parse model JsonPaths
//                var jsonPathMatches = Regex.Matches(modelJson, "\\$\\.([\\.a-zA-Z0-9]+(\\[.*\\])?)+", RegexOptions.Multiline);
//                var jsonPaths = new Dictionary<string, string>();
//                for (var i = 0; i < jsonPathMatches.Count; i++)
//                {
//                    var jsonPathMatch = jsonPathMatches[i];
//                    if (jsonPaths.Values.All(x => x != jsonPathMatch.Value))
//                    {
//                        var key = "jsonpath" + jsonPaths.Count;
//                        jsonPaths.Add(key, jsonPathMatch.Value);
//                        modelJson = modelJson.Replace(jsonPathMatch.Value, key);
//                    }
//                }

//                // Register templates
//                hb.RegisterJsonTemplate("sitemap", sitemapJson);
//                hb.RegisterJsonTemplate("mymodel", modelJson);

//                // Perform the sitemap transformation
//                sitemapJson = hb.Transform("sitemap", new { });

//                // Parse the sitemap
//                var sitemap = JsonConvert.DeserializeObject<JObject>(sitemapJson);

//                GenerateUrls(sitemap);

//                // Convert current URL into JsonPath statement to find current node
//                var currentNode = sitemap;
//                if (!string.IsNullOrWhiteSpace(currentUrl))
//                {
//                    currentNode = (JObject)sitemap.SelectToken(UrlToJsonPath(currentUrl));
//                }

//                // Append parsed JsonPath queries to properties collection
//                foreach (var kvp in jsonPaths)
//                {
//                    var nodes = sitemap.SelectTokens(kvp.Value);
//                    currentNode.Add(kvp.Key, new JArray(nodes));
//                }

//                var model = hb.Transform("mymodel", currentNode);
//                var modelObj = JsonConvert.DeserializeObject<JObject>(model);

//                var t = 1;
//            }
//        }

//        private static string UrlToJsonPath(string url)
//        {
//            return url.Split('/').Aggregate("$", (current, currentUrlPart) => current + (".pages[?(@.alias == '" + currentUrlPart + "')]"));
//        }

//        private static void GenerateUrls(JContainer container, string path = "")
//        {
//            var jArray = container as JArray;
//            if (jArray != null)
//            {
//                foreach (var item in jArray)
//                {
//                    var itemContainer = item as JContainer;
//                    if (itemContainer != null)
//                    {
//                        GenerateUrls(itemContainer, path);
//                    }
//                }
//            }

//            var jObj = container as JObject;
//            if (jObj != null)
//            {
//                var aliasProp = jObj["alias"].Value<string>();

//                path = (path + "/" + aliasProp).TrimStart('/');

//                jObj.Add("url", path);

//                foreach (var prop in jObj.Properties())
//                {
//                    var propContainer = prop.Value as JContainer;
//                    if (propContainer != null)
//                    {
//                        GenerateUrls(propContainer, path);
//                    }
//                }
//            }
//        }
//    }
//}
