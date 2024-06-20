using System.ComponentModel.DataAnnotations;
using SpotMate.Domain.Enums;

namespace SpotMate.Application.DTOs.Requests;

public sealed class EditStatusDto
{
    public UserStatus? UserStatus { get; init; }
}