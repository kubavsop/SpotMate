using System.Reflection;
using AutoMapper;

namespace SpotMate.Application.Mapping;

public sealed class ConventionalMappingProfile: Profile
{
    private static readonly Type MapFromType = typeof(IMapFrom<>);
    
    public ConventionalMappingProfile()
    {
        var types = Assembly
            .GetExecutingAssembly()
            .GetExportedTypes()
            .Where(t => !t.IsAbstract &&
                        t.GetInterfaces().Any(ImplementsMapFromInterface));
        
        foreach (var type in types)
        {
            var instance = Activator.CreateInstance(type);
            var methodInfo = type.GetInterfaces().FirstOrDefault(ImplementsMapFromInterface)
                ?.GetMethod(nameof(IMapFrom<object>.Mapping));
            methodInfo?.Invoke(instance, new object[] { this });
        }
    }
    
    private static bool ImplementsMapFromInterface(Type t)
    {
        return t.IsGenericType && t.GetGenericTypeDefinition() == MapFromType;
    }
}