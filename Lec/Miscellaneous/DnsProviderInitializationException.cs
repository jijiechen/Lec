using System;

namespace Lec
{
    public class DnsProviderInitializationException: Exception
    {
        public DnsProviderInitializationException(string message) : base(message) { }
    }

    public class DnsProviderMissingConfigurationException : DnsProviderInitializationException
    {
        public DnsProviderMissingConfigurationException(string confKey)  
            : base($"The required configuration {confKey} is missing.")
        {

        }
    }
}
