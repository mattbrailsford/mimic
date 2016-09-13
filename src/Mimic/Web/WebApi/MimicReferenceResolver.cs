using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RazorEngine.Compilation;
using RazorEngine.Compilation.ReferenceResolver;

namespace Mimic.Web.WebApi
{
    public class MimicReferenceResolver : IReferenceResolver
    {
        public string FindLoaded(IEnumerable<string> refs, string find)
        {
            return refs.FirstOrDefault(r => r.EndsWith(System.IO.Path.DirectorySeparatorChar + find));
        }
        public IEnumerable<CompilerReference> GetReferences(TypeContext context, IEnumerable<CompilerReference> includeAssemblies = null)
        {
            IEnumerable<string> loadedAssemblies = new UseCurrentAssembliesReferenceResolver()
               .GetReferences(context, includeAssemblies)
               .Select(r => r.GetFile())
               .ToArray();

            foreach (var loadedAssembly in loadedAssemblies)
            {
                yield return CompilerReference.From(Assembly.LoadFrom(loadedAssembly));
            }

            // Loop the temp folder
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
            foreach (var file in Directory.GetFiles(path, "*.dll"))
            {
                yield return CompilerReference.From(Assembly.LoadFrom(file));
            }
        }
    }
}
