using Azure.Core;

namespace Hephaestus.Infrastructure.Identity.Cache
{
    internal class AccessTokenCacheEntry
    {
        public TokenRequestContext RequestContext { get; private set; }

        public AccessToken AccessToken { get; set; }

        public AccessTokenCacheEntry(TokenRequestContext requestContext)
        {
            RequestContext = requestContext;
        }
    }
}
