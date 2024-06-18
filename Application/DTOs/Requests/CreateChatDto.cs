using System.ComponentModel.DataAnnotations;

namespace SpotMate.Application.DTOs.Requests;

public class CreateChatDto
{
    [Required]
    public required Guid UserId { get; init; }
}