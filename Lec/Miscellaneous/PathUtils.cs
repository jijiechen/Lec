using System.IO;

namespace Lec
{
    static class PathUtils
    {
        public static string PrepareOutputFilePath(string outputFilePath, out string directoryPath)
        {
            directoryPath = Path.GetDirectoryName(NormalizedPath(outputFilePath));
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return Path.Combine(directoryPath, Path.GetFileName(outputFilePath));
        }

        public static string NormalizedPath(string filePath)
        {
            var normalizedPath = Path.GetFullPath(Path.IsPathRooted(filePath)
                ? filePath
                : Path.Combine(Directory.GetCurrentDirectory(), filePath));
            return normalizedPath;
        }

        public static string AppliationPath => Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
    }
}
