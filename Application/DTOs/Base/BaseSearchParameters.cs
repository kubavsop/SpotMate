using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SpotMate.Application.DTOs.Base;

public abstract class BaseSearchParameters
{
    [DefaultValue(DefaultLimit)]
    [Range(1, int.MaxValue)]
    public int Limit { get; set; } = DefaultLimit;
    
    [DefaultValue(DefaultOffset)]
    [Range(0, int.MaxValue)]
    public int Offset { get; set; } = DefaultOffset;
    
    
    private const int DefaultLimit = 10;
    private const int DefaultOffset = 0;
}