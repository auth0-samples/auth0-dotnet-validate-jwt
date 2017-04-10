using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            const string auth0Domain = "https://jerrie.auth0.com/"; // Your Auth0 domain
            const string auth0Audience = "https://rs256.test.api"; // Your API Identifier
            const string testToken = ""; // Obtain a JWT to validate and put it in here

            try
            {
                // Download the OIDC configuration which contains the JWKS
                IConfigurationManager<OpenIdConnectConfiguration> configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>($"{auth0Domain}.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
                OpenIdConnectConfiguration openIdConfig = AsyncHelper.RunSync(async () => await configurationManager.GetConfigurationAsync(CancellationToken.None));

                // Configure the TokenValidationParameters. Assign the SigningKeys which were downloaded from Auth0. 
                // Also set the Issuer and Audience(s) to validate
                TokenValidationParameters validationParameters =
                    new TokenValidationParameters
                    {
                        ValidIssuer = auth0Domain,
                        ValidAudiences = new[] { auth0Audience },
                        IssuerSigningKeys = openIdConfig.SigningKeys
                    };

                // Now validate the token. If the token is not valid for any reason, an exception will be thrown by the method
                SecurityToken validatedToken;
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                var user = handler.ValidateToken(testToken, validationParameters, out validatedToken);

                // The ValidateToken method above will return a ClaimsPrincipal. Get the user ID from the NameIdentifier claim
                // (The sub claim from the JWT will be translated to the NameIdentifier claim)
                Console.WriteLine($"Token is validated. User Id {user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error occurred while validating token: {e.Message}");
                throw;
            }

            Console.WriteLine();
            Console.WriteLine("Press ENTER to continue...");
            Console.ReadLine();
        }
    }
}