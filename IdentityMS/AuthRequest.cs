namespace IdentityMS
{
    public class AuthRequest
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Audience { get; set; }
        public string[] Scopes { get; set; }
    }
}
