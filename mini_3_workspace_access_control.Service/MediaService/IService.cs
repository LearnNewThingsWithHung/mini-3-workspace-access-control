using Microsoft.AspNetCore.Http;

namespace mini_3_workspace_access_control.Service.MediaService;

public interface IService
{
    public Task<string> UploadImageAsync(IFormFile file);
}