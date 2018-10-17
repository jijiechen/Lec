
using System;

namespace LeDNSCertStore.DnsProviders
{
    public interface IDnsProvider: IDisposable
    {
        void Initialize(string configuration);
        string AddTxtRecord(string name, string value);
        void RemoveTxtRecord(string recordRef);
    }

    [AttributeUsage( AttributeTargets.Class, Inherited = false, AllowMultiple = false )]
    public class DnsProviderAttribute : Attribute
    {
        public DnsProviderAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get;}
    }
}
