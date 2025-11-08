namespace Hephaestus.Infrastructure.Identity.Factories
{
    public class CachedCredentialFactory : ICachedCredentialFactory
    {
        private readonly ICredentialCache _credentialCache;

        public CachedCredentialFactory(ICredentialCache credentialCache)
        {
            _credentialCache = credentialCache;
        }

        public CachedCredential GetCredential(string credentialKey)
        {
            return new CachedCredential(_credentialCache, credentialKey);
        }

        public CachedCredential GetCredential()
        {
            return GetCredential(Constants.CredentialKey.DefaultCredentialKey);
        }
    }
}
