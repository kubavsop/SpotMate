namespace SpotMate.Application.Exceptions;

public sealed class BadRequestException : Exception
{
    public BadRequestException() {}
    
    public BadRequestException(string message) : base(message) {}
}