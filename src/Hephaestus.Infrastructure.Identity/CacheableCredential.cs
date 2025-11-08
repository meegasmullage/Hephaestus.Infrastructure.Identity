using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;

namespace Hephaestus.Infrastructure.Identity
{
    public sealed class CacheableCredential : TokenCredential
    {
        private readonly string _credentialKey;
        private readonly TokenCredential _credential;
        private readonly CredentialSnapshot _snapshot;
        private ICredentialCache _credentialCache;

        public CacheableCredential(string credentialKey, TokenCredential credential)
        {
            var credentialType = credential.GetType();

            if (credentialType == typeof(CachedCredential)
                || credentialType == typeof(CacheableCredential))
            {
                throw new ArgumentException($"Type '{credentialType.FullName}' is not supported", nameof(credential));
            }

            _credentialKey = credentialKey;
            _credential = credential;

            _snapshot = new CredentialSnapshot
            {
                CredentialKey = credentialKey,
                Credential = credential
            };
        }

        public CacheableCredential(TokenCredential credential)
            : this(Constants.CredentialKey.DefaultCredentialKey, credential)
        {
        }

        internal CredentialSnapshot CurrentSnapshot => _snapshot;

        internal ICredentialCache CredentialCache
        {
            get => _credentialCache;
            set => _credentialCache = value;
        }

        private AccessToken GetTokenCore(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var accessToken = _credentialCache != null ?
                _credentialCache.GetToken(_credentialKey, requestContext, cancellationToken) : _credential.GetToken(requestContext, cancellationToken);

            _snapshot.RequestContext = requestContext;
            _snapshot.AccessToken = accessToken;

            return accessToken;
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return GetTokenCore(requestContext, cancellationToken);
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var accessToken = GetTokenCore(requestContext, cancellationToken);

            return new ValueTask<AccessToken>(accessToken);
        }
    }
}
