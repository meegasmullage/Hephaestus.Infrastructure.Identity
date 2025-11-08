using System;
using Azure.Core;

namespace Hephaestus.Infrastructure.Identity.Extensions
{
    public static class TokenRequestContextExtensions
    {
        public static TokenRequestContext SetParentRequestId(this TokenRequestContext requestContext)
        {
            if (!string.IsNullOrEmpty(requestContext.ParentRequestId))
            {
                return requestContext;
            }

            return new TokenRequestContext(requestContext.Scopes,
                $"Infrastructure.Identity:{Guid.NewGuid():N}",
                requestContext.Claims,
                requestContext.TenantId,
                requestContext.IsCaeEnabled);
        }
    }
}
