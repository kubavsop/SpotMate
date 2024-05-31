namespace SpotMate.Application.Exceptions;

public sealed class ForbiddenException : Exception
{
    public ForbiddenException() {}

    public ForbiddenException(string message) : base(message) {}

    public ForbiddenException(Guid userId) : base($"Access is forbidden for user with id=({userId})") {}
}