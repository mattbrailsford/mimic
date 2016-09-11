using System.Collections.Generic;

namespace Mimic.IO
{
    public interface IFileSystem
    {
        bool DirectoryExists(string path);

        IEnumerable<string> GetFiles(string pathPattern);

        IEnumerable<string> GetFiles(string path, string pattern); 

        bool FileExists(string path);

        string GetFileContents(string path);

        string GetFileNameWithoutExtension(string path);

        string MakeAbsolutePath(string relativePath);
    }
}
