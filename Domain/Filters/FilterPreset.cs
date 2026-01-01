// File: Domain/Filters/FilterPreset.cs
namespace MedicalOfficeManagement.Domain.Filters;

public class FilterPreset
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TargetPage { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
