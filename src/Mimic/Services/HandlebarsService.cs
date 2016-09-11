using System;
using System.Collections.Generic;
using Jint;
using Jint.Runtime.Interop;
using Mimic.Extensions;
using Mimic.Handlebars.Helpers;

namespace Mimic.Services
{
    internal class HandlebarsService
    {
        private Chevron.Handlebars _handlebars;

        public HandlebarsService()
            : this(null)
        { }

        public HandlebarsService(IEnumerable<IObjectConverter> converters)
        {
            var engine = new Engine(options =>
            {
                if (converters != null)
                {
                    foreach (var objectConverter in converters)
                    {
                        options.AddObjectConverter(objectConverter);
                    }
                }
            });

            _handlebars = new Chevron.Handlebars(engine);

            RegisterHelpers();

            AppDomain.CurrentDomain.ProcessExit += this.CurrentDomain_ProcessExit;
        }

        protected void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if (_handlebars != null)
            {
                _handlebars.Dispose();
                _handlebars = null;
            }
        }

        protected void RegisterHelpers()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    var attribs = type.GetCustomAttributes(typeof(HandlebarsHelperAttribute), false);
                    if (attribs.Length > 0)
                    {
                        var attr = attribs[0] as HandlebarsHelperAttribute;
                        var instance = Activator.CreateInstance(type) as HandlebarsHelper;
                        if (instance != null)
                        {
                            _handlebars.RegisterHelper(attr.Name, instance.GetJs());
                        }
                    }
                }
            }
        }

        public bool HasTemplate(string templateName)
        {
            return _handlebars.registeredTemplates.Contains(templateName.MakeAliasSafe());
        }

        public void ClearTemplates()
        {
            _handlebars.registeredTemplates.Clear();
        }

        public void RegisterJsonTemplate(string templateName, string template)
        {
            _handlebars.RegisterTemplate(templateName.MakeAliasSafe(), template);
        }

        public string Transform(string templateName, object model = null)
        {
            return _handlebars.Transform(templateName.MakeAliasSafe(), model);
        }
    }
}
