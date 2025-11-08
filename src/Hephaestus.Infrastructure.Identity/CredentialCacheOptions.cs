using System;
using System.Collections.Generic;
using Azure.Core;

namespace Hephaestus.Infrastructure.Identity
{
    public class CredentialCacheOptions
    {
        public TimeSpan RenewalTrigger { get; set; }

        public IDictionary<string, TokenCredential> TokenCredentials { get; set; }

        public CacheableCredential[] CacheableCredentials { get; set; }

        public CredentialCacheOptions()
        {
            RenewalTrigger = new TimeSpan(0, 30, 1);
            TokenCredentials = new Dictionary<string, TokenCredential>();
            CacheableCredentials = [];
        }
    }
}
