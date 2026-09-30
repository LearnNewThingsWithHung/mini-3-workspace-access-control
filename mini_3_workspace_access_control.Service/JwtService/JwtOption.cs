using System.ComponentModel.DataAnnotations;

namespace mini_3_workspace_access_control.Service.JwtService;

public class JwtOption
{
    [Required]public string Issuer { get; set; } = null!;
    [Required]public string Audience { get; set; } = null!;
    [Required]public string AccessTokenKey { get; set; } = null!;
    [Required]public int AccessTokenExpireMin { get; set; }
    [Required]public string RefreshTokenKey { get; set; } = null!;
    [Required]public int RefreshTokenExpireMin { get; set; }
}
