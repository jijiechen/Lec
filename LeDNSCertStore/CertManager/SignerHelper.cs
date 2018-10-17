using System;
using System.IO;
using ACMESharp.Crypto.JOSE;
using ACMESharp.Crypto.JOSE.Impl;

namespace LeDNSCertStore.CertManager
{
    class SignerHelper
    {
        public static IJwsTool LoadFromFile(string signerPath)
        {
            var xml = File.ReadAllText(signerPath);
            return GenerateTool("RS256", xml);
        }


        public static void SaveToFile(RSJwsTool signer, string signerPath)
        {
            var xml = signer.Export();
            File.WriteAllText(signerPath, xml);
        }
        
        private static IJwsTool GenerateTool(string keyType, string path)
        {
            if (keyType.StartsWith("ES"))
            {
                var tool = new ACMESharp.Crypto.JOSE.Impl.ESJwsTool();
                tool.HashSize = int.Parse(keyType.Substring(2));
                tool.Init();
                tool.Import(path);
                return tool;
            }

            if (keyType.StartsWith("RS"))
            {
                var tool = new ACMESharp.Crypto.JOSE.Impl.RSJwsTool();
                tool.KeySize = int.Parse(keyType.Substring(2));
                tool.Init();
                tool.Import(path);
                return tool;
            }

            throw new Exception($"Unknown or unsupported KeyType [{keyType}]");
        }
    }
}
