using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Lec.Tests
{
    public class GenerateSignatureTests
    {
        private string _accessKeyId = "testid";
        private string _accessKeySecret = "testsecret";

        [Fact]
        public void should_sign()
        {
            var parameters = new Dictionary<string, string>()
            {
                {"Action", "DescribeDomainRecords"},
                {"DomainName", "example.com"},    
            };
            
            var signedParameters = BuildRequestParameters(parameters)
                .Split("&")
                .Select(frag => frag.Split("="))
                .ToDictionary(arr => arr[0], arr => arr[1]);
            
            Assert.Equal("XML", signedParameters["Format"]);
            Assert.Equal("DescribeDomainRecords", signedParameters["Action"]);
            Assert.Equal("testid", signedParameters["AccessKeyId"]);
            Assert.Equal("HMAC-SHA1", signedParameters["SignatureMethod"]);
            Assert.Equal("f59ed6a9-83fc-473b-9cc6-99c95df3856e", signedParameters["SignatureNonce"]);
            Assert.Equal("example.com", signedParameters["DomainName"]);
            Assert.Equal("2015-01-09", signedParameters["Version"]);
            Assert.Equal("1.0", signedParameters["SignatureVersion"]);
            Assert.Equal("uRpHwaSEt3J%2B6KQD%2F%2FsvCh%2Fx%2BpI%3D", signedParameters["Signature"]);
            Assert.Equal("2016-03-24T16%3A41%3A54Z", signedParameters["Timestamp"]);
        }
        
        
        string BuildRequestParameters(Dictionary<string, string> requestParameters)
        {
            var nonce = "f59ed6a9-83fc-473b-9cc6-99c95df3856e";//Guid.NewGuid().ToString("N").Substring(4, 8);
            var parameters = new Dictionary<string, string>
            {
                {"Format", "XML"},
                {"Version", "2015-01-09"},
                {"SignatureMethod", "HMAC-SHA1"},
                {"SignatureVersion", "1.0"},
                {"AccessKeyId", _accessKeyId},
                {"SignatureNonce", nonce},
                {"Timestamp", "2016-03-24T16:41:54Z"}
            };

            foreach (var key in requestParameters.Keys)
            {
                parameters.Add(key, requestParameters[key]);
            }

            var signature = GenerateSignature(_accessKeySecret, parameters);
            parameters.Add("Signature", signature);

            return string.Join("&", parameters.Select(kv => $"{kv.Key}={WebUtility.UrlEncode(kv.Value)}"));
        }
    
        static string GenerateSignature(string accessKeySecret, Dictionary<string, string> parametersBeforeSign)
        {
            string Encode(string p)
            {
                return WebUtility.UrlEncode(p)
                                .Replace("+", "20%")
                                .Replace("*", "%2A")
                                .Replace("%7E", "~");
            }
            
            string Hmac(string val, string keySecret)
            {
                var bytes = Encoding.ASCII.GetBytes(val);
                var key = Encoding.ASCII.GetBytes(keySecret + "&");

                using (var hmacsha1 = new HMACSHA1(key))
                using (var stream = new MemoryStream(bytes))
                {
                    return Convert.ToBase64String(hmacsha1.ComputeHash(stream));
                }    
            }
                
            var encodedParameters = parametersBeforeSign
                .OrderBy(kv => kv.Key, StringComparer.Ordinal)
                .Select(kv => $"{Encode(kv.Key)}={Encode(kv.Value)}");
            
            var canonicalizedQueryString = string.Join("&", encodedParameters);
            var stringToSign = $"GET&%2F&{Encode(canonicalizedQueryString)}";
            return Hmac(stringToSign, accessKeySecret);
        }
    }
}