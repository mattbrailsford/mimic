using System;

namespace Mimic.Handlebars.Helpers
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HandlebarsHelperAttribute : Attribute
    {
        public string Name { get; set; }

        public HandlebarsHelperAttribute(string name)
        {
            Name = name;
        }
    }
}
