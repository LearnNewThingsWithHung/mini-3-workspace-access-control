namespace mini_3_workspace_access_control.Service.Exceptions;

public class ServerException : AppException
{
    public ServerException(string detail)
        : base(title: "Internal Server Error", statusCode: 500, messageCode: "INTERNAL_SERVER_ERROR", detail: detail) { }
}