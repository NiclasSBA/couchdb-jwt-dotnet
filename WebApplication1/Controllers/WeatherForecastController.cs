using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IConfiguration _configuration;
        public WeatherForecastController( IHttpContextAccessor accessor, IConfiguration configuration)
        {
            var host = accessor.HttpContext.Request.Host.Value;
            _configuration = configuration;
          
        }

        [HttpGet]
        public JwtResponse Get()
        {

    //this one is fine
    //var t = "MIICXAIBAAKBgQCJHYcuRHdJ5S8GYcz/IgF5hJY+vvchVaeKyD+GGSoiD58pRJ3kx5b7YbbP/EyzwhUxWncvbsiWZFdqca/DHsFKxNnRgvidoyq2dgA+erP91aFHpccH2ykNxC6LTzMXX4XWp5mXKm6XfMkBFzsVC4/a7A6UHnsL7MU2b4lec+WkSwIDAQABAoGAMV1zLOIzfGRKAOc3MefhVgm5Og/w04yODHY6AKKQu8CaEfaFTjfZkNnGQq1YRCOtE565aFdfWl335vfVSs+I0UTKYtUdU0DkeZ93nB+eaUIQ/7UC99UlcdSrlRfXGwBxdcwM+Ek93VeITWERydh+xyXN3VxzaYtApA1fB/YGnzkCQQDcb8X6xFigw0qSpHXX+qpBQcTSbIQ5u47vK0VRIkDI3vN3tGBeIpU78kK3E+cHG8e74Nobn1/nJ09TKS4jzoudAkEAnzyDQPD269xzwTvICVMDvezEN4sy3+XJdJjtOdtL4RZKRqOUiaJmKhR1QUtlrG71LqudUiwZ7DnzloKyvLYvBwJAXCyw0GcB2FdQ+3ihfipmvtrNfl+5+poe7otddMup41S24bsfAL3dQS/QDdXYqPRI1Jr1GM/PvkyFsvRpQre/UQJBAIavIDVlmvSUWjQu5Fs+pAOYp75zNmy6Z1L/pmcxXVTdDaYB5jkj61XcR/EaXL0kfK0k6sP+GU79FVNQ6O1FCzECQAD8SLqaJOGbiPYrf+gRx337xQTatlaaXIaRrtNKj4E3/WtyKQQXEILCMDS1Xa88sK12nQgu6DVxOtIs+7cEvds=";
             var readFile = System.IO.File.ReadAllText("./testkeypair.pem");
            var public_key_from_File = System.IO.File.ReadAllText("./testkeypairpublic.pem");
            var pub_key = public_key_from_File.Replace("\r", "");

            using RSA rsa = RSA.Create();
            rsa.ImportFromPem(readFile);

            var now = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();
            var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
            {
                CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
            };
            var testingClaims =  "admin" ;
            var authClaims = new List<Claim>()
            {
                new Claim("sub", "hardcoded username"),
                new Claim("alg", "RS256"),
                //INFO _couchdb.roles is not optional, if not present, an empty array is there... check /session endpoint
                //the empty claim for couchdb.roles is added to format the claim as a list, otherwise we get a badarg error when requesting resources from couch
                new Claim("_couchdb.roles","theadminrole"),
                new Claim("_couchdb.roles",""),
                //new Claim("iss", _configuration["JWT:ValidIssuer"]),
                //new Claim("iss", _configuration["JWT:ValidIssuer"]),

                //INFO _admin is like the superuser/god claim and gives access the entire node, not just a single database
                //new Claim("_couchdb.roles","_admin"),
            
            };

            //Theres an issue with iss claim on couch 3.x
            //https://github.com/apache/couchdb/issues/3232
            //Bypassing issue by removing it for now
            var token = new JwtSecurityToken(
                   //issuer: _configuration["JWT:ValidIssuer"],
                   audience: _configuration["JWT:ValidAudience"],
                   claims: authClaims,
                   
                   signingCredentials: signingCredentials,
                   notBefore: now.AddMinutes(-5),
                   expires: now.AddMinutes(30)
                   );

            var issuedToken = new JwtSecurityTokenHandler().WriteToken(token);



            var validatersa = RSA.Create();
            validatersa.ImportFromPem(pub_key);
            var tert = validatersa.ExportRSAPublicKey();
           var tert2 = System.Text.Encoding.BigEndianUnicode.GetString(tert);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                //ValidateAudience = true,
                //ValidateLifetime = true,
                //ValidateIssuerSigningKey = true,
                //ValidIssuer = _configuration["JWT:ValidIssuer"],
                ValidAudience = _configuration["JWT:ValidAudience"],
                IssuerSigningKey = new RsaSecurityKey(validatersa),
                CryptoProviderFactory = new CryptoProviderFactory()
                {
                    CacheSignatureProviders = false
                }
            };

            try
            {
                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(issuedToken, validationParameters, out var validatedSecurityToken);
            }
            catch(Exception ex)
            {
                throw;
            }

            return new JwtResponse
            {
                Token = issuedToken,
                ExpiresAt = unixTimeSeconds,
            };
        }

      

        private static string ConvertPKCS1Pem(string pemContents)
        {
            var rgx = new Regex("/\r?\n|\r/g");
            return rgx.Replace(pemContents, "")
                     .Replace("-----BEGIN RSA PRIVATE KEY-----", "")
                     .Replace("-----END RSA PRIVATE KEY-----", "");
            //return pemContents
            //    .TrimStart("-----BEGIN RSA PRIVATE KEY-----".ToCharArray())
            //    .TrimEnd("-----END RSA PRIVATE KEY-----".ToCharArray())
            //    .Replace(" ", "")
            //    .Replace("\n", "")
            //    .Replace("\r", "")
            //    .Replace("\r\n", "");
        }


    }
   
    public static class TypeConverterExtension
    {
        public static byte[] ToByteArray(this string value) =>
               Convert.FromBase64String(value);
    }
}
