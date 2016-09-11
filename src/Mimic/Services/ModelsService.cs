using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Mimic.Extensions;
using Mimic.Handlebars.ObjectConverters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mimic.Services
{
    internal class ModelsService
    {
        private HandlebarsService _handlebars;

        private IDictionary<string, string> _jsonPaths;
        private IDictionary<string, JArray> _jsonPathsResults;
        private IDictionary<string, IEnumerable<string>> _modelPlaceholders;

        public ModelsService()
        {
            _handlebars = new HandlebarsService(new [] { new JValueObjectConverter() });
            _jsonPaths = new ConcurrentDictionary<string, string>();
            _jsonPathsResults = new ConcurrentDictionary<string, JArray>();
            _modelPlaceholders = new ConcurrentDictionary<string, IEnumerable<string>>();
        }

        public void ClearModelTemplates()
        {
            _jsonPaths.Clear();
            _jsonPathsResults.Clear();
            _modelPlaceholders.Clear();
            _handlebars.ClearTemplates();
        }

        public bool HasModelTemplate(string name)
        {
            return _handlebars.HasTemplate(name);
        }

        public void RegisterModelTemplate(string name, string json, JObject jsonPathSrc = null)
        {
            var safeName = name.MakeAliasSafe();

            // Extract json path statements
            if (jsonPathSrc != null)
            {
                IList<string> placeholders;
                json = ProcessJsonPaths(json, jsonPathSrc, out placeholders);

                // Record which statements are associated with this template
                if (_modelPlaceholders.ContainsKey(safeName))
                {
                    _modelPlaceholders[safeName] = placeholders;
                }
                else
                {
                    _modelPlaceholders.Add(safeName, placeholders);
                }
            }

            _handlebars.RegisterJsonTemplate(name, ProcessJsonTemplate(json));
        }

        public JObject GenerateModel(string templateName, JObject data = null)
        {
            var safeTemplateName = templateName.MakeAliasSafe();

            // Inject jsonPath data into model
            var modelClone = (JObject)data?.DeepClone(); // We deep clone as we are appending properties which we don't want to persist
            if (modelClone != null && _modelPlaceholders.ContainsKey(safeTemplateName))
            {
                foreach (var placeholder in _modelPlaceholders[safeTemplateName])
                {
                    if (_jsonPathsResults.ContainsKey(placeholder))
                    {
                        modelClone.Add(placeholder, _jsonPathsResults[placeholder]);
                    }
                }
            }

            // Transform the data against the stored template
            var json = _handlebars.Transform(templateName, modelClone);

            // Convert to JSON model object
            return JsonConvert.DeserializeObject<JObject>(json);
        }

        protected string ProcessJsonPaths(string json, JObject jsonPathSrc, out IList<string> placeholders)
        {
            placeholders = new List<string>();

            var jsonPathMatches = Regex.Matches(json, "\\$\\.([\\.a-zA-Z0-9]+(\\[.*\\])?)+", RegexOptions.Multiline);
            for (var i = 0; i < jsonPathMatches.Count; i++)
            {
                var jsonPathMatch = jsonPathMatches[i];

                if (!_jsonPaths.ContainsKey(jsonPathMatch.Value))
                {
                    var placeholder = "jsonpath" + _jsonPaths.Count;
                    _jsonPaths.Add(jsonPathMatch.Value, placeholder);
                    _jsonPathsResults.Add(placeholder, new JArray(jsonPathSrc.SelectTokens(jsonPathMatch.Value)));
                }

                placeholders.Add(_jsonPaths[jsonPathMatch.Value]);

                json = json.Replace(jsonPathMatch.Value, _jsonPaths[jsonPathMatch.Value]);
            }

            return json;
        }

        protected string ProcessJsonTemplate(string json)
        {
            // Strip whitespace from JSON
            json = Regex.Replace(json, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");

            // Replace any handlebars templates not in quotes so that it doesn't break the JSON parser
            json = Regex.Replace(json, "([^\"\\s]\\s*)({{[^}]*}})(\\s*[^\"\\s])", "$1\"%$2%\"$3");

            // Parse the JSON for processing
            var jObj = JsonConvert.DeserializeObject<JObject>(json);

            // Convert JSON into a regular handlebars template
            var rawTemplate = new StringBuilder();
            JsonToHandlebarsTemplateRescursive(jObj, rawTemplate);
            json = rawTemplate.ToString();

            // Replace any previously replaced non quoted handlebar templates with their original value
            // Because these are not strings, we'll force the value lowercase to make it JSON safe too
            json = Regex.Replace(json, "\"%({{[^}]*}})%\"", " $1 ");

            return json;
        }

        protected void JsonToHandlebarsTemplateRescursive(JContainer container, StringBuilder builder)
        {
            // If container is array, just append to the builder
            var jArray = container as JArray;
            if (jArray != null)
            {
                builder.Append("[");

                for (var i = 0; i < jArray.Count; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(",");
                    }

                    if (jArray[i] is JContainer)
                    {
                        JsonToHandlebarsTemplateRescursive((JContainer)jArray[i], builder);
                    }
                    else
                    {
                        builder.Append(jArray[i].ToString(Formatting.None));
                    }
                }

                builder.Append("]");
            }

            // If container is an object, check to see if it's a helper and process accordingly
            var jObj = container as JObject;
            if (jObj != null)
            {
                var props = jObj.Properties().ToList();
                if (props.Count > 0)
                {
                    // Check to see if node is actually a handlebars helper
                    var firstProp = props[0];
                    if (firstProp.Name.StartsWith("#"))
                    {
                        // Handlerbars helper, so covert into handlebars template format
                        foreach (var prop in props)
                        {
                            // Construct the helper opening tag
                            builder.Append("{{" + prop.Name + "}}");

                            // Assume that all looping helpers support the @index variable
                            // and use this to dertimne if we should add a comma between objects
                            builder.Append("{{#if @index}},{{/if}}");

                            // Append the helper template (recurse if an object, incase we contain nested templates)
                            if (prop.Value is JContainer)
                            {
                                JsonToHandlebarsTemplateRescursive((JContainer)prop.Value, builder);
                            }
                            else
                            {
                                builder.Append(prop.Value.ToString(Formatting.None));
                            }
                        }

                        // Construct the helper closing tag
                        builder.Append("{{/" + firstProp.Name.Substring(1, firstProp.Name.IndexOf(" ", StringComparison.InvariantCulture) - 1) + "}}");
                    }
                    else
                    {
                        // Not a handlerbars block, so just process properties normally
                        builder.Append("{");

                        for (var i = 0; i < props.Count; i++)
                        {
                            if (i > 0)
                            {
                                builder.Append(",");
                            }

                            builder.AppendFormat("\"{0}\":", props[i].Name);
                            if (props[i].Value is JContainer)
                            {
                                JsonToHandlebarsTemplateRescursive((JContainer)props[i].Value, builder);
                            }
                            else
                            {
                                builder.Append(props[i].Value.ToString(Formatting.None));
                            }
                        }

                        builder.Append("}");
                    }
                }
            }
        }
    }
}
