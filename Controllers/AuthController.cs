using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Models;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.Commons;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;
        private string _issuer;
        private string _secret;

        public AuthController(ILogger<AuthController> logger, IConfiguration config)
        {
            _config = config;
            _logger = logger;
        }

        private async Task<string> GenerateJwtToken(string username)
        {
            await GetVaultSecret();
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, username)
    };
            var token = new JwtSecurityToken(
                _issuer,
                "http://localhost",
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            if (login.Username == "haavy_user" && login.Password == "aaakodeord")
            {
                var token = GenerateJwtToken(login.Username);
                return Ok(new { token });
            }
            return Unauthorized();
        }
        private async Task GetVaultSecret()
        {
            var EndPoint = "https://localhost:8201/";
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback =
            (message, cert, chain, sslPolicyErrors) => { return true; };

            // Initialize one of the several auth methods.
            IAuthMethodInfo authMethod =
            new TokenAuthMethodInfo("00000000-0000-0000-0000-000000000000");
            // Initialize settings. You can also set proxies, custom delegates etc. here.
            var vaultClientSettings = new VaultClientSettings(EndPoint, authMethod)
            {
                Namespace = "",
                MyHttpClientProviderFunc = handler
                => new HttpClient(httpClientHandler)
                {
                    BaseAddress = new Uri(EndPoint)
                }
            };
            IVaultClient vaultClient = new VaultClient(vaultClientSettings);

            // Use client to read a key-value secret.
            Secret<SecretData> kv2Secret = await vaultClient.V1.Secrets.KeyValue.V2
            .ReadSecretAsync(path: "hemmeligheder", mountPoint: "secret");
             _issuer = kv2Secret.Data.Data["Issuer"].ToString()!;

            // Use client to read a key-value secret.
            _secret = kv2Secret.Data.Data["Secret"].ToString()!;
            _logger.LogInformation("issue: {0}", _issuer);
            _logger.LogInformation("secret: {0}", _secret);
        }

    }

}
