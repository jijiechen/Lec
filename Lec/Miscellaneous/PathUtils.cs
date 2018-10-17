using System.IO;

namespace Lec
{
    static class PathUtils
    {
        public static string PrepareOutputFilePath(string outputFilePath, out string directoryPath)
        {
            directoryPath = Path.GetDirectoryName(outputFilePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return Path.Combine(directoryPath, Path.GetFileName(outputFilePath));
        }

        public static string AppliationPath => Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
    }
}
