using System;
using System.Collections.Generic;
using System.IO;
using ACMESharp.Crypto.JOSE;
using ACMESharp.Crypto.JOSE.Impl;
using Newtonsoft.Json;

namespace Lec.Persistence
{
    class SignerHelper
    {
        public static IJwsTool LoadFromFile(string signerPath)
        {
            signerPath = PathUtils.NormalizedPath(signerPath);
            var json = File.ReadAllText(signerPath);
            var signerObj = JsonConvert.DeserializeObject<ExportedSigner>(json);
            return GenerateTool(signerObj.JwsAlg, json);
        }


        public static void SaveToFile(IJwsTool signer, string signerPath)
        {
            var json = signer.Export();
            
            var dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            dic.Add("JwsAlg", signer.JwsAlg);
            json = JsonConvert.SerializeObject(dic);
            
            File.WriteAllText(signerPath, json);
        }
        
        private static IJwsTool GenerateTool(string keyType, string json)
        {
            if (keyType.StartsWith("ES"))
            {
                var tool = new ACMESharp.Crypto.JOSE.Impl.ESJwsTool();
                tool.HashSize = int.Parse(keyType.Substring(2));
                tool.Init();
                tool.Import(json);
                return tool;
            }

            if (keyType.StartsWith("RS"))
            {
                var tool = new ACMESharp.Crypto.JOSE.Impl.RSJwsTool();
                tool.KeySize = int.Parse(keyType.Substring(2));
                tool.Init();
                tool.Import(json);
                return tool;
            }

            throw new Exception($"Unknown or unsupported KeyType [{keyType}]");
        }


        class ExportedSigner
        {
            public string JwsAlg { get; set; }
        }
    }
}
