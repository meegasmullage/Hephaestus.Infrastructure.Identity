# Hephaestus.Infrastructure.Identity
A simple, lightweight .NET library for the automatic caching and renewal of Azure access tokens (TokenCredential objects)

## Why Use This Library?

- Performance: Caching tokens reduces the latency and overhead associated with constantly fetching new tokens from Azure AD.

- Reliability: Automatic renewal ensures that access tokens are valid before being used, minimizing authorization failures due to expired credentials.

- Simplicity: Wraps existing TokenCredential implementations (like DefaultAzureCredential) to transparently manage the token lifecycle.

- Lightweight: The library is designed to be a simple wrapper with no heavy external dependencies, making integration quick and seamless.