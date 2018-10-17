using System.IO;
using Newtonsoft.Json;

namespace Lec.Miscellaneous
{
    internal class FileUtils
    {
        internal static bool SaveStateFrom<T>(T value, string fullPath)
        {
            var fullDir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(fullDir))
                Directory.CreateDirectory(fullDir);

            var ser = JsonConvert.SerializeObject(value, Formatting.Indented);
            File.WriteAllText(fullPath, ser);
            return true;
        }
        
        
        internal static bool LoadStateInto<T>(ref T value, string fullPath, bool throwOnException)
        {
            if (!File.Exists(fullPath))
                if (throwOnException)
                    throw new FileNotFoundException($"Failed to read object from non-existent path [{fullPath}]", fullPath);
                else
                    return false;
            
            var ser = File.ReadAllText(fullPath);
            value = JsonConvert.DeserializeObject<T>(ser);
            return true;
        }
    }
}