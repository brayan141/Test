using IdentityMS.Services;
using IdentityMS;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwtService;

    public AuthController(JwtService jwtService)
    {
        _jwtService = jwtService;
    }

    [HttpPost("token")]
    public IActionResult GetToken([FromBody] AuthRequest request)
    {
        // Validación simulada
        if (request.ClientSecret != "Z7k!qP9x@R3t#sD8wL4v%N2yB6j*X0h")
            return Unauthorized("Invalid client secret.");

        try
        {
            var token = _jwtService.GenerateToken(
                clientId: request.ClientId,
                scopes: request.Scopes,
                audience: request.Audience
            );

            return Ok(new { token });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}