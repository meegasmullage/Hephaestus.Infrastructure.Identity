using System.Collections.Generic;
using System.Threading;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hephaestus.Infrastructure.Identity.Sandbox
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var cacheableCredential = new CacheableCredential(Constants.CredentialKey.Credential1, new DefaultAzureCredential());

            var accessToken = cacheableCredential.GetToken(new TokenRequestContext([
                "https://vault.azure.net/.default"
            ]), CancellationToken.None);

            //
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    //Configure CredentialCacheOptions is optional, unless you want to enable cacheing for Credentials that were created manually
                    services.Configure<CredentialCacheOptions>(options =>
                    {
                        options.TokenCredentials = new Dictionary<string, TokenCredential> {
                            {
                                Constants.CredentialKey.Credential2, new DefaultAzureCredential()
                            }
                        };

                        options.CacheableCredentials = [
                            cacheableCredential
                        ];
                    });

                    services.AddIdentity();

                    services.AddHostedService<ExampleService>();
                });

            builder
                .UseConsoleLifetime()
                .Build()
                .Run();
        }
    }
}
