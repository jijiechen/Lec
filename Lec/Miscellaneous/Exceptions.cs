using System;
using System.Collections.Generic;
using ACMESharp.Protocol.Resources;

namespace Lec
{
    class AuthorizationFailedException: Exception
    {
        public IList<Authorization> AuthorizationResults { get; }

        public AuthorizationFailedException(IList<Authorization> results)
            : base("One or more hostname is failed to be authorized")
        {
            AuthorizationResults = results;
        }
    }


    class CertificateApplicationException: Exception
    {
        public string ErrorReason { get; }

        public string CommonName { get; }

        public string[] AlternativeDnsNames { get; } = new string[0];

        public CertificateApplicationException(string reason, string commonName, string[] alternativeDnsNames = null)
            : base($"'{reason}' when trying to creating the certificate for {commonName}")
        {
            this.ErrorReason = reason;
            this.CommonName = commonName;

            if(alternativeDnsNames != null)
            {
                this.AlternativeDnsNames = alternativeDnsNames;
            }
        }
    }

}
