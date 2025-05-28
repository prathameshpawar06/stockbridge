using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace stockbridge_api.Helper
{
    public class TokenValidationService
    {
        private readonly IConfiguration _configuration;

        public TokenValidationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<GenericResponse<ValidatedUser>> ValidateTokenAsync(string token)
        {
            var instance = _configuration["AzureAd:Instance"];
            var tenantId = _configuration["AzureAd:TenantId"];
            var audience = _configuration["AzureAd:ClientId"];

            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{instance}{tenantId}/v2.0/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever());

            OpenIdConnectConfiguration config = await configurationManager.GetConfigurationAsync(CancellationToken.None);
            var signingKeys = config.SigningKeys;

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,
                ValidateIssuer = true,
                ValidIssuers = new List<string>
                {
                    $"{instance}{tenantId}/v2.0", // For V2.0 tokens
                    $"https://sts.windows.net/{tenantId}/" // For V1.0 tokens
                },
                ValidateAudience = true,
                ValidAudiences = new List<string>
                {
                    $"api://{audience}", // For audience as a URI
                    audience // For audience as a plain ID
                },
                ValidateLifetime = true,
            };

            IdentityModelEventSource.ShowPII = true;

            SecurityToken validatedToken;
            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                var firstName = principal.FindFirst(ClaimTypes.GivenName)?.Value;
                var role = principal.FindFirst(ClaimTypes.Role)?.Value;

                var validatedUser = new ValidatedUser
                {
                    Email = email,
                    FirstName = firstName,
                    Role = role
                };

                return new GenericResponse<ValidatedUser>(true, "Token is valid", validatedUser);
            }
            catch (SecurityTokenException ex)
            {
                return new GenericResponse<ValidatedUser>(false, $"Token validation failed: {ex.Message}", null);
            }
        }
    }
    public class ValidatedUser
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string Role { get; set; }
    }
}
