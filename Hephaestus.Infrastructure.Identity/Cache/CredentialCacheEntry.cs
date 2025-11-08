using System.Collections.Generic;
using Azure.Core;

namespace Hephaestus.Infrastructure.Identity.Cache
{
    internal class CredentialCacheEntry
    {
        public object LockObject { get; private set; }

        public TokenCredential Credential { get; private set; }

        public IDictionary<string, AccessTokenCacheEntry> AccessTokenCache { get; private set; }

        public CredentialCacheEntry(TokenCredential credential)
        {
            Credential = credential;

            LockObject = new object();
            AccessTokenCache = new Dictionary<string, AccessTokenCacheEntry>();
        }
    }
}
