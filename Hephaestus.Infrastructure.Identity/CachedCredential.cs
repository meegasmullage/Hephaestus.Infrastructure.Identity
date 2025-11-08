using Azure.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Hephaestus.Infrastructure.Identity
{
    public sealed class CachedCredential : TokenCredential
    {
        private readonly ICredentialCache _credentialCache;
        private readonly string _credentialKey;

        internal CachedCredential(ICredentialCache credentialCache, string credentialKey)
        {
            _credentialCache = credentialCache;
            _credentialKey = credentialKey;
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return _credentialCache.GetToken(_credentialKey, requestContext, cancellationToken);
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return _credentialCache.GetTokenAsync(_credentialKey, requestContext, cancellationToken);
        }
    }
}
