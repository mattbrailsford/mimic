using System;
using System.IO;
using RazorEngine;
using RazorEngine.Compilation;
using RazorEngine.Compilation.CSharp;

namespace Mimic.Web.WebApi
{
    public class MimicCompilerService : CSharpDirectCompilerService
    {
        protected override string GetTemporaryDirectory()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp", "RazorEngine_" + Path.GetRandomFileName());

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }
    }

    public class MimicCompilerServiceFactory : ICompilerServiceFactory
    {
        public ICompilerService CreateCompilerService(Language language)
        {
            return new MimicCompilerService();
        }
    }
}
