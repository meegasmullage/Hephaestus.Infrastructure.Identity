using System;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Hephaestus.Infrastructure.Identity.Cache;
using Hephaestus.Infrastructure.Identity.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hephaestus.Infrastructure.Identity
{
    public class CredentialCache : ICredentialCache
    {
        private readonly ILogger<CredentialCache> _logger;
        private readonly CredentialCacheOptions _options;
        private readonly Dictionary<string, CredentialCacheEntry> _credentialCache;
        private readonly System.Timers.Timer _timer;
        private bool _isRunning;

        public CredentialCache(ILogger<CredentialCache> logger, IOptions<CredentialCacheOptions> options)
        {
            _logger = logger;
            _options = options.Value;

            _credentialCache = _options.TokenCredentials.ToDictionary(r => r.Key, r => new CredentialCacheEntry(r.Value));

            if (_options.CacheableCredentials.Length > 0)
            {
                var requestContextDefaultValue = default(TokenRequestContext);
                var accessTokenDefaultValue = default(AccessToken);

                foreach (var cacheableCredential in _options.CacheableCredentials)
                {
                    var snapshot = cacheableCredential.CurrentSnapshot;

                    var credentialCacheEntry = new CredentialCacheEntry(snapshot.Credential);

                    if (!requestContextDefaultValue.Equals(snapshot.RequestContext)
                        && !accessTokenDefaultValue.Equals(snapshot.AccessToken))
                    {
                        var accessTokenCache = credentialCacheEntry.AccessTokenCache;

                        var accessTokenCacheKey = GetAccessTokenCacheKey(snapshot.RequestContext);

                        accessTokenCache.Add(accessTokenCacheKey, new AccessTokenCacheEntry(snapshot.RequestContext)
                        {
                            AccessToken = snapshot.AccessToken
                        });
                    }

                    _credentialCache.Add(snapshot.CredentialKey, credentialCacheEntry);

                    cacheableCredential.CredentialCache = this;
                }
            }

            if (!_credentialCache.TryGetValue(Constants.CredentialKey.DefaultCredentialKey, out _))
            {
                _credentialCache.Add(Constants.CredentialKey.DefaultCredentialKey, new CredentialCacheEntry(new DefaultAzureCredential()));
            }

            _timer = new System.Timers.Timer
            {
                AutoReset = false,
                Enabled = true,
                Interval = 1000 * 60,
            };

            _timer.Elapsed += AccessTokenRenewalHandler;
        }

        private void AccessTokenRenewalHandler(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (_isRunning)
                {
                    return;
                }

                _isRunning = true;
                _timer.Enabled = false;

                foreach (var (key, credentialCacheEntry) in _credentialCache)
                {
                    foreach (var (_, accessTokenCacheEntry) in credentialCacheEntry.AccessTokenCache)
                    {
                        var expiresOn1 = accessTokenCacheEntry.AccessToken.ExpiresOn.ToUniversalTime();
                        var expiresOn2 = DateTimeOffset.UtcNow.Add(_options.RenewalTrigger);

                        if (expiresOn1 > expiresOn2)
                        {
                            continue;
                        }

                        try
                        {
                            lock (credentialCacheEntry.LockObject)
                            {
                                accessTokenCacheEntry.AccessToken = credentialCacheEntry.Credential
                                    .GetToken(accessTokenCacheEntry.RequestContext.SetParentRequestId(), default);

                                _logger.LogInformation("Access token renewed. [CredentialKey={credentialKey}] [TenantId={tenantId}] [Scopes={scopes}]",
                                    key, accessTokenCacheEntry.RequestContext.TenantId, string.Join(",", accessTokenCacheEntry.RequestContext.Scopes));
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "An error occurred while renewing access tokens. [CredentialKey={credentialKey}]", key);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while renewing access tokens");
            }
            finally
            {
                _isRunning = false;
                _timer.Enabled = true;
            }
        }

        private static string GetAccessTokenCacheKey(TokenRequestContext requestContext)
        {
            var stringBuilder = new StringBuilder(4096);

            stringBuilder
                .AppendJoin(string.Empty, requestContext.Scopes)
                .Append(requestContext.Claims)
                .Append(requestContext.TenantId)
                .Append(requestContext.IsCaeEnabled);

            var requestContextHash = XxHash64.Hash(Encoding.UTF8.GetBytes(stringBuilder.ToString()));

            return stringBuilder
                .Clear()
                .AppendJoin(string.Empty, requestContextHash.Select(r => r.ToString("x")))
                .ToString();
        }

        public AccessToken GetToken(string credentialKey, TokenRequestContext requestContext, CancellationToken cancellationToken = default)
        {
            if (!_credentialCache.TryGetValue(credentialKey, out var credentialCacheEntry))
            {
                throw new KeyNotFoundException($"Credential key ${credentialKey} not found");
            }

            var accessTokenCache = credentialCacheEntry.AccessTokenCache;
            var accessTokenCacheKey = GetAccessTokenCacheKey(requestContext);

            if (!accessTokenCache.TryGetValue(accessTokenCacheKey, out var accessTokenCacheEntry))
            {
                lock (credentialCacheEntry.LockObject)
                {
                    if (!accessTokenCache.TryGetValue(accessTokenCacheKey, out accessTokenCacheEntry))
                    {
                        var credential = credentialCacheEntry.Credential;

                        accessTokenCacheEntry = new AccessTokenCacheEntry(requestContext)
                        {
                            AccessToken = credential.GetToken(requestContext.SetParentRequestId(), cancellationToken)
                        };

                        accessTokenCache[accessTokenCacheKey] = accessTokenCacheEntry;
                    }
                }
            }

            return accessTokenCacheEntry.AccessToken;
        }

        public ValueTask<AccessToken> GetTokenAsync(string credentialKey, TokenRequestContext requestContext, CancellationToken cancellationToken = default)
        {
            var accessToken = GetToken(credentialKey, requestContext, cancellationToken);

            return new ValueTask<AccessToken>(accessToken);
        }

        public AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken = default)
        {
            return GetToken(Constants.CredentialKey.DefaultCredentialKey, requestContext, cancellationToken);
        }

        public ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken = default)
        {
            return GetTokenAsync(Constants.CredentialKey.DefaultCredentialKey, requestContext, cancellationToken);
        }
    }
}
