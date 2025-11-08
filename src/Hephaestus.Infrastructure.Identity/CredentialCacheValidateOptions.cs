using System.Linq;
using Microsoft.Extensions.Options;

namespace Hephaestus.Infrastructure.Identity
{
    public class CredentialCacheValidateOptions : IValidateOptions<CredentialCacheOptions>
    {
        public ValidateOptionsResult Validate(string name, CredentialCacheOptions options)
        {
            var cachedCredentialType = typeof(CachedCredential);
            var cacheableCredentialType = typeof(CacheableCredential);

            var credential = options.TokenCredentials.Values.FirstOrDefault(r =>
            {
                var credentialType = r.GetType();

                return cachedCredentialType.IsAssignableFrom(credentialType)
                || cacheableCredentialType.IsAssignableFrom(credentialType);
            });

            return credential == null ?
                ValidateOptionsResult.Success :
                ValidateOptionsResult.Fail($"Type '{credential.GetType()}' is not supported");

        }
    }
}
