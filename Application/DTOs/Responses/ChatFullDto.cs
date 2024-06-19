using System.ComponentModel.DataAnnotations;

namespace SpotMate.Application.DTOs.Responses;

public sealed class ChatFullDto
{
    [Required]
    public required ChatShortDto Chat { get; init; }
    [Required]
    public required int UnreadMessagesCount { get; init; }
    
    [Required]
    public required bool IsBlocked { get; init; }
    public MessageDto? LastMessage { get; init; }
}