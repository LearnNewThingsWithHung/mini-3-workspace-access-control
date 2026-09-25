namespace mini_3_workspace_access_control.Service.PersonAccess;

public interface IService
{
    Task<Response.AccessPersonResponse> GetMeAsync(
        Guid personId,
        CancellationToken ct = default);
}
