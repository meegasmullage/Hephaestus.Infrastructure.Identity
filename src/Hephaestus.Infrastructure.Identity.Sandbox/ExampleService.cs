using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Azure.Core;
using Azure.Storage.Blobs;
using Hephaestus.Infrastructure.Identity.Factories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hephaestus.Infrastructure.Identity.Sandbox
{
    public class ExampleService : IHostedService
    {
        private readonly ILogger<ExampleService> _logger;
        private readonly ICredentialCache _credentialCache;
        private readonly ICachedCredentialFactory _cachedCredentialFactory;
        private readonly System.Timers.Timer _timer;

        public ExampleService(ILogger<ExampleService> logger, ICredentialCache credentialCache, ICachedCredentialFactory cachedCredentialFactory)
        {
            _logger = logger;
            _credentialCache = credentialCache;
            _cachedCredentialFactory = cachedCredentialFactory;

            _timer = new System.Timers.Timer
            {
                Interval = 1000 * 5,
                Enabled = true
            };

            _timer.Elapsed += TimerElapsed;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _timer.Enabled = false;

                Task.Run(async () =>
                {
                    //
                    var accessToken = _credentialCache.GetToken(new TokenRequestContext([
                        "https://vault.azure.net/.default"
                    ]));

                    accessToken = _credentialCache.GetToken(Constants.CredentialKey.Credential1, new TokenRequestContext([
                        "https://vault.azure.net/.default"
                    ]));

                    accessToken = await _credentialCache.GetTokenAsync(Constants.CredentialKey.Credential2, new TokenRequestContext([
                        "https://vault.azure.net/.default"
                    ]));

                    //
                    var cachedCredential = _cachedCredentialFactory.GetCredential();

                    var blobServiceClient = new BlobServiceClient(new Uri("https://x8rndstorage.blob.core.windows.net/"), cachedCredential);
                    var containerItems = blobServiceClient.GetBlobContainersAsync();

                    await foreach (var containerItem in containerItems)
                    {
                        Console.WriteLine(containerItem.Name);
                    }

                    //
                    cachedCredential = _cachedCredentialFactory.GetCredential(Constants.CredentialKey.Credential1);

                    accessToken = cachedCredential.GetToken(new TokenRequestContext([
                        "https://vault.azure.net/.default"
                     ]), CancellationToken.None);

                    cachedCredential = _cachedCredentialFactory.GetCredential(Constants.CredentialKey.Credential2);

                    accessToken = cachedCredential.GetToken(new TokenRequestContext([
                        "https://vault.azure.net/.default"
                     ]), CancellationToken.None);

                    accessToken = await cachedCredential.GetTokenAsync(new TokenRequestContext([
                        "https://vault.azure.net/.default"
                     ]), CancellationToken.None);

                    //
                    accessToken = await _credentialCache.GetTokenAsync(Constants.CredentialKey.Credential2, new TokenRequestContext([
                        "https://database.windows.net/.default"
                    ]));

                    accessToken = await _credentialCache.GetTokenAsync(new TokenRequestContext([
                        "https://database.windows.net/.default"
                    ]));
                }).GetAwaiter().GetResult();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while running the service");
            }
            finally
            {
                _timer.Enabled = true;
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _timer.Enabled = true;

            await Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Enabled = false;

            return Task.CompletedTask;
        }
    }
}
