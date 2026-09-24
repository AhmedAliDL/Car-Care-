namespace App.Application.Common.Exceptions;

public abstract class AppException : Exception
{
    public int StatusCode { get; }
    public string Title { get; }

    protected AppException(int statusCode, string title, string message) : base(message)
    {
        StatusCode = statusCode;
        Title = title;
    }
}

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message) : base(404, "Not Found", message)
    {
    }
}

public sealed class ConflictException : AppException
{
    public ConflictException(string title, string message) : base(409, title, message)
    {
    }
}

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(403, "Forbidden", message)
    {
    }
}

public sealed class BadRequestException : AppException
{
    public BadRequestException(string title, string message) : base(400, title, message)
    {
    }
}

public sealed class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message) : base(401, "Unauthorized", message)
    {
    }
}
