using System.ComponentModel.DataAnnotations;

namespace mini_3_workspace_access_control.Service.CloudinaryService;

public class CloudinaryOptions
{
    [Required] public string CloudName { get; set; }
    [Required] public string ApiKey { get; set; }
    [Required] public string ApiSecret { get; set; }
}