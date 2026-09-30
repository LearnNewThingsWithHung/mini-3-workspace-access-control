namespace mini_3_workspace_access_control.Service.Exceptions;

public sealed class BadRequestException : AppException
{
    public BadRequestException(string messageCode, string detail)
        : base("Bad Request", 400, messageCode, detail)
    {
    }
}

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string messageCode, string detail)
        : base("Forbidden", 403, messageCode, detail)
    {
    }
}

public sealed class NotFoundException : AppException
{
    public NotFoundException(string messageCode, string detail)
        : base("Not Found", 404, messageCode, detail)
    {
    }
}

public sealed class ConflictException : AppException
{
    public ConflictException(string messageCode, string detail)
        : base("Conflict", 409, messageCode, detail)
    {
    }
}

public sealed class GoneException : AppException
{
    public GoneException(string messageCode, string detail)
        : base("Gone", 410, messageCode, detail)
    {
    }
}
