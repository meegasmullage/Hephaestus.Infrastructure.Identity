using Azure.Core;

namespace Hephaestus.Infrastructure.Identity
{
    internal class CredentialSnapshot
    {
        public string CredentialKey { get; set; }

        public TokenCredential Credential { get; set; }

        public TokenRequestContext RequestContext { get; set; }

        public AccessToken AccessToken { get; set; }
    }
}
