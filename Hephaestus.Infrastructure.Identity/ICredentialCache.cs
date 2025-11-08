using System.Threading;
using System.Threading.Tasks;
using Azure.Core;

namespace Hephaestus.Infrastructure.Identity
{
    public interface ICredentialCache
    {
        AccessToken GetToken(string credentialKey, TokenRequestContext requestContext, CancellationToken cancellationToken = default);

        ValueTask<AccessToken> GetTokenAsync(string credentialKey, TokenRequestContext requestContext, CancellationToken cancellationToken = default);

        AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken = default);

        ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken = default);
    }
}
