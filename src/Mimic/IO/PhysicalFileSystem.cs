using System;
using System.Collections.Generic;
using System.IO;

namespace Mimic.IO
{
    public class PhysicalFileSystem : IFileSystem
    {
        private string _basePath;
        private FileSystemWatcher _fileSystemWatcher;

        public PhysicalFileSystem(string basePath)
        {
            _basePath = basePath;
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(MakeAbsolutePath(path));
        }

        public IEnumerable<string> GetFiles(string pathPattern)
        {
            var dirPath = pathPattern.Substring(0, pathPattern.LastIndexOf('/') + 1);
            var pattern = pathPattern.Substring(pathPattern.LastIndexOf('/') + 1);

            return GetFiles(dirPath, pattern);
        }

        public IEnumerable<string> GetFiles(string path, string pattern)
        {
            return Directory.GetFiles(MakeAbsolutePath(path), pattern);
        }

        public bool FileExists(string path)
        {
            return File.Exists(MakeAbsolutePath(path));
        }

        public string GetFileContents(string path)
        {
            return File.ReadAllText(MakeAbsolutePath(path));
        }

        public string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        public string MakeAbsolutePath(string relativePath)
        {
            return relativePath.Replace("~", _basePath.TrimEnd('/'));
        }

        public void Watch(string path, string filter, Action callback)
        {
            _fileSystemWatcher = new FileSystemWatcher(path, filter)
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };
            _fileSystemWatcher.Created += (sender, args) => callback();
            _fileSystemWatcher.Changed += (sender, args) => callback();
            _fileSystemWatcher.Renamed += (sender, args) => callback();
            _fileSystemWatcher.Deleted += (sender, args) => callback();
            _fileSystemWatcher.Error += (sender, args) => Watch(path, filter, callback);
        }
    }
}
