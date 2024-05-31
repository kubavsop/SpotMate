using Microsoft.AspNetCore.Identity;

namespace SpotMate.Application.Exceptions;

public sealed class IdentityException: Exception
{
    public List<IdentityError>? Errors { get; set; }
    
    public IdentityException(List<IdentityError> errors)
    {
        Errors = errors;
    }
}