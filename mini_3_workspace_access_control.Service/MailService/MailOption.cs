using System.ComponentModel.DataAnnotations;

namespace mini_3_workspace_access_control.Service.MailService;

public class MailOptions
{
    [Required]public string Mail { get; set; } = null!;
    [Required]public string DisplayName { get; set; } = null!;
    [Required]public string Password { get; set; } = null!;
    [Required]public string Host { get; set; } = null!;
    [Required]public int Port { get; set; }
}
