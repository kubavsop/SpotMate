using System.ComponentModel.DataAnnotations;

namespace SpotMate.Application.DTOs.Responses;

public sealed class ChatDto
{
    [Required]
    public required Guid Id { get; init; }
    [Required]
    public required int UnreadMessagesCount { get; init; }
    [Required]
    public required MessageDto LastMessage { get; init; }
}