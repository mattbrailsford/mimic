using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Mimic.Services
{
    internal class TypeBuilderService
    {
        private string _baseAssemblyName = "Mimic.ViewModels";
        private int _versionCount;
        private ConcurrentBag<string> _typeNameCache;
        private string _tempDirPath;

        public TypeBuilderService()
        {
            _versionCount = 0;
            _typeNameCache = new ConcurrentBag<string>();
            _tempDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");

            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
        }

        private void CurrentDomainOnProcessExit(object sender, EventArgs eventArgs)
        {
            CleanupTempFolder();
        }

        internal void CleanupTempFolder()
        {
            if (Directory.Exists(_tempDirPath))
            {
                foreach (var file in Directory.GetFiles(_tempDirPath, "*.dll"))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception)
                    {
                        // Likely a lock on the file so leave it for now and we'll try again next time
                    }
                }
            }
        }

        public void DeclareTypes(IEnumerable<string> typeNames)
        {
            var newTypeNames = typeNames.Where(x => _typeNameCache.All(y => !string.Equals(y, x, StringComparison.InvariantCultureIgnoreCase))).ToArray();
            if (newTypeNames.Any())
            {
                var assemblyName = new AssemblyName
                {
                    Name = _baseAssemblyName,
                    Version = new Version(0, 0, 0, _versionCount++)
                };

                var assemblyFileName = _baseAssemblyName + ".delta" + _versionCount + ".dll";
                
                if (!Directory.Exists(_tempDirPath)) Directory.CreateDirectory(_tempDirPath);

                var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save, _tempDirPath);

                var moduleBuilder = assemblyBuilder.DefineDynamicModule(_baseAssemblyName, assemblyFileName);

                foreach (var typeName in newTypeNames)
                {
                    var tb = moduleBuilder.DefineType(typeName, TypeAttributes.Class | TypeAttributes.BeforeFieldInit | TypeAttributes.Public);

                    var ctr = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, null);
                    var il = ctr.GetILGenerator();
                    il.Emit(OpCodes.Ret);
                    tb.CreateType();

                    // Add the type name to cache
                    _typeNameCache.Add(typeName);
                }

                assemblyBuilder.Save(assemblyFileName);
            }
        }
    }
}
