using System.ComponentModel.DataAnnotations;

namespace mini_3_workspace_access_control.Service.MailService;

public class MailOptions
{
    [Required]public string Mail { get; set; }
    [Required]public string DisplayName { get; set; }
    [Required]public string Password { get; set; }
    [Required]public string Host { get; set; }
    [Required]public int Port { get; set; }
}