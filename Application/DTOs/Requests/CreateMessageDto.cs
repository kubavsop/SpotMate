using System.ComponentModel.DataAnnotations;

namespace SpotMate.Application.DTOs.Requests;

public class CreateMessageDto
{
    [Required]
    public required string Message { get; init; }
}