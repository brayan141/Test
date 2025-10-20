using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityMS.Services
{
    public class JwtService
    {
        private readonly string _secretKey = "G8r#qBzL5uM2!eWp9@YtNvZcF3$hRxKd";
        private readonly string _issuer = "IdentityMS";
        private readonly string[] _audiences = new[] { "UserMS", "LocationMS", "InventoryMS" };

        public string GenerateToken(string clientId, string[] scopes, string audience, int expireMinutes = 60)
        {
            // Validar que la audiencia esté en la lista permitida
            if (!_audiences.Contains(audience))
                throw new ArgumentException("Audience no válida", nameof(audience));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, clientId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var scope in scopes)
            {
                claims.Add(new Claim("scope", scope));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
