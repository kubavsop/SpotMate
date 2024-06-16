using System.ComponentModel.DataAnnotations;
using SpotMate.Application.Mapping;
using SpotMate.Domain.Entities;

namespace SpotMate.Application.DTOs.Responses;

public sealed class StepDto
{
    [Required]
    [Range(0, int.MaxValue)]
    public int StepCount { get; set; }
    [Required]
    public Guid UserId { get; set; }
}