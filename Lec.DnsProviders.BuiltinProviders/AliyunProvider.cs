using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Lec.Acme.DnsProviders;
using Newtonsoft.Json;

namespace Lec.DnsProviders.BuiltinProviders
{
    /// <summary>
    /// The implemetation of the Aliyun DNS service
    /// </summary>
    /// <remarks>
    /// Aliyun documentation: https://help.aliyun.com/product/29697.html
    /// </remarks>
    [DnsProvider("Aliyun")]
    public class AliyunProvider : IDnsProvider
    {
        readonly HttpClient _httpClient = new HttpClient();
        
        const string AliyunDnsBaseUri = "https://alidns.aliyuncs.com/";
        const string AddDomainRecord = "AddDomainRecord";
        const string DeleteDomainRecord = "DeleteDomainRecord";
        const string HttpMethod = "POST";


        private string _accessKeyId;
        private string _accessKeySecret;
        private string _domainName;
        
        public void Initialize(string configuration)
        {
            var conf = KVConfigurationParser.Parse(configuration, new[]
            {
                "access_key_id", "access_key_secret", "domain"
            });

            _accessKeyId = conf["access_key_id"];
            _accessKeySecret = conf["access_key_secret"];
            _domainName = conf["domain"];
        }

        public string AddTxtRecord(string name, string value)
        {
            var recordName = name == _domainName ? "@" : name;
            recordName = name.Replace(_domainName, "").TrimEnd('.');
            
            var parameters = new Dictionary<string, string>()
            {
                {"Action", AddDomainRecord},
                {"DomainName", _domainName},
                {"RR", recordName},
                {"Type", "TXT"},
                {"Value", value}
            };

            var requestBody = SignRequestParameters(parameters);
            var resp = InvokeAliyunDnsAPI(AddDomainRecord, requestBody);
            if (string.IsNullOrEmpty(resp.Code))
            {
                return resp.RecordId;
            }
            
            throw new AliyunDnsException(AddDomainRecord + " (unexpected response)", null, null, resp);
        }

        public void RemoveTxtRecord(string recordRef)
        {
            var parameters = new Dictionary<string, string>()
            {
                {"Action", DeleteDomainRecord},
                {"RecordId", recordRef}
            };

            var requestBody = SignRequestParameters(parameters);
            var resp = InvokeAliyunDnsAPI(AddDomainRecord, requestBody);

            if (!string.IsNullOrEmpty(resp.Code))
            {
                throw new AliyunDnsException(AddDomainRecord + " (unexpected response)", null, null, resp);
            }
        }


        public void Dispose()
        {
            _httpClient.Dispose();
        }

        
        AliyunDnsResponse InvokeAliyunDnsAPI(string action, string signedParameters)
        {
            string responseContent = null;
            AliyunDnsResponse dnsResponse = null;
            try
            {
                if (_httpClient.DefaultRequestHeaders.UserAgent.Count < 1)
                {
                    _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.96 Safari/537.36");
                }

                var postContent = new StringContent(signedParameters, Encoding.ASCII, "application/x-www-form-urlencoded");
                var response = _httpClient.PostAsync(AliyunDnsBaseUri, postContent).Result;

                responseContent = response.Content.ReadAsStringAsync().Result;
                dnsResponse = JsonConvert.DeserializeObject<AliyunDnsResponse>(responseContent);
                return dnsResponse;
            }
            catch (HttpRequestException ex)
            {
                throw new AliyunDnsException(action, ex, responseContent, dnsResponse);
            }
            catch (WebException ex)
            {
                try
                {
                    string resp;
                    using (var sr = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        resp = sr.ReadToEnd();
                    }

                    throw new AliyunDnsException(action, ex, resp, null);
                }
                catch
                {
                    throw ex;
                }
                    
            }
            catch (JsonException jsonException)
            {
                throw new AliyunDnsException(action, jsonException, responseContent, null);
            }
        }
        
        
        
        
        
        string SignRequestParameters(Dictionary<string, string> requestParameters)
        {
            var nonce = Guid.NewGuid().ToString("N").Substring(4, 8);
            var parameters = new Dictionary<string, string>
            {
                {"Format", "JSON"},
                {"Version", "2015-01-09"},
                {"SignatureMethod", "HMAC-SHA1"},
                {"SignatureVersion", "1.0"},
                {"AccessKeyId", _accessKeyId},
                {"SignatureNonce", nonce},
                {"Timestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")}
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
            var stringToSign = $"{HttpMethod}&%2F&{Encode(canonicalizedQueryString)}";
            return Hmac(stringToSign, accessKeySecret);
        }


        class AliyunDnsResponse
        {
            public string RequestId { get; set; }
            public string RecordId { get; set; }
            
            public string HostId { get; set; }
            public string Code { get; set; }
            public string Message { get; set; }
        }

        class AliyunDnsException: Exception
        {
            public AliyunDnsResponse ResponseObject { get; set; }
            public string ResponseContent { get; set; }
            
            public AliyunDnsException(string apiAction, Exception exception, 
                string response, AliyunDnsResponse responseObject)
                : base($"Failed to request api {AliyunDnsBaseUri} with action {apiAction}", exception)
            {
                this.ResponseContent = response;
                this.ResponseObject = responseObject;
            }
        }
    }

}
