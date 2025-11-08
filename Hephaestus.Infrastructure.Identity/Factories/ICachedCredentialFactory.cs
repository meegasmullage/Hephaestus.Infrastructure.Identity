namespace Hephaestus.Infrastructure.Identity.Factories
{
    public interface ICachedCredentialFactory
    {
        CachedCredential GetCredential(string credentialKey);

        CachedCredential GetCredential();
    }
}
