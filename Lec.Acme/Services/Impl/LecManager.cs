using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACMESharp.Protocol;
using Lec.Acme.DnsProviders;
using Lec.Acme.Models;
using Microsoft.Extensions.Options;

namespace Lec.Acme.Services.Impl
{
    class LecManager: ILecManager
    {
        private readonly IAcmeClientFactory _clientFactory;
        private readonly IDnsAuthorizer _dnsAuthorizer;
        private readonly ICertificateCollector _certificateCollector;
        private readonly ICsrGenerator _csrGenerator;

        private bool _hasInited;
        private LecAcmeConfiguration _config;
        private AcmeProtocolClient _acmeClient;

        public LecManager(IAcmeClientFactory clientFactory, 
            IDnsAuthorizer dnsAuthorizer, 
            ICertificateCollector certificateCollector, 
            ICsrGenerator csrGenerator,
            IOptions<LecAcmeConfiguration> configration)
        {
            _clientFactory = clientFactory;
            _dnsAuthorizer = dnsAuthorizer;
            _certificateCollector = certificateCollector;
            _csrGenerator = csrGenerator;
            _config = configration.Value;
        }
        
        
        public async Task InitializeAsync(AcmeAccount account)
        {
            _acmeClient = await _clientFactory.CreateAcmeClientAsync(_config, account?.Account, account?.Signer);
            _hasInited = true;
        }

        public async Task<AcmeAccount> CreateAccountAsync(IEnumerable<string> contacts, bool acceptTos)
        {
            if (!_hasInited)
            {
                throw new InvalidOperationException("Should initialize this AcmeManager instance before using it.");
            }
            
            var account = await _acmeClient.CreateAccountAsync(contacts, acceptTos);
            var signer = _acmeClient.Signer;
            
            return new AcmeAccount
            {
                Account = account,
                Signer = signer
            };
        }

        public async Task<IssuedCertificate> RequestCertificateAsync(IDnsProvider dnsProvider, string hostname, IEnumerable<string> alternativeHostnames)
        {
            if (!_hasInited)
            {
                throw new InvalidOperationException("Should initialize this AcmeManager instance before using it.");
            }

            
            var alternatives = alternativeHostnames.ToArray();
            var order = await _acmeClient.CreateOrderAsync(new[] {hostname}.Concat( alternatives ));
            await _dnsAuthorizer.AuthorizeAsync(_acmeClient, order, dnsProvider);

            var csr = await _csrGenerator.GenerateCsrAsync(_config.PrivateKeyBitLength, hostname, alternatives);
            return await _certificateCollector.CollectCertificateAsync(_acmeClient, order, csr);
        }
    }
}