namespace SpotMate.Application.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException() {}
    
    public NotFoundException(string message) : base(message) {}
    
    public NotFoundException(string name, Guid key)
        : base($"{name} with id=({key}) was not found.") {}
}