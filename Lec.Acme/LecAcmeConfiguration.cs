namespace Lec.Acme
{
    public class LecAcmeConfiguration
    {
#if DEBUG
        public string AcmeServerBaseUri { get; set; } = "https://acme-staging-v02.api.letsencrypt.org/";

#else
        public string AcmeServerBaseUri { get; set; } = "https://acme-v02.api.letsencrypt.org/";
#endif
        
        
        public short PrivateKeyBitLength { get; set; } = 4096;

        public string ProxyUri { get; set; }
        public string ProxyUserName { get; set; }
        public string ProxyPassword { get; set; }

    }
}