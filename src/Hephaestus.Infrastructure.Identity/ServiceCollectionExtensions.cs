using Hephaestus.Infrastructure.Identity.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Hephaestus.Infrastructure.Identity
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentity(this IServiceCollection services)
        {
            services.TryAddSingleton<IValidateOptions<CredentialCacheOptions>, CredentialCacheValidateOptions>();

            services.TryAddSingleton<ICredentialCache, CredentialCache>();
            services.TryAddSingleton<ICachedCredentialFactory, CachedCredentialFactory>();

            return services;
        }
    }
}
