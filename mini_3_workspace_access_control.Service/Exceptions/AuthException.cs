namespace mini_3_workspace_access_control.Service.Exceptions;

public class MissingAccessTokenException : AppException
{
    public MissingAccessTokenException()
        : base("Unauthorized", 401, "MISSING_ACCESS_TOKEN", 
            "Access token is missing.") { }
}

public class ExpiredAccessTokenException : AppException
{
    public ExpiredAccessTokenException()
        : base("Unauthorized", 401, "EXPIRED_ACCESS_TOKEN", 
            "Access token has expired.") { }
}

public class ExpiredRefreshTokenException : AppException
{
    public ExpiredRefreshTokenException()
        : base("Unauthorized",401,  "EXPIRED_REFRESH_TOKEN", 
            "Refresh token has expired. Please login again.") { }
}

public class DemoPersonUnauthorizedException : AppException
{
    public DemoPersonUnauthorizedException()
        : base(
            "Unauthorized",
            401,
            "DEMO_PERSON_REQUIRED",
            "The demo person id is missing, invalid, inactive, or does not identify a person.")
    {
    }
}
