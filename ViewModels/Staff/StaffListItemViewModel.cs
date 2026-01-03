namespace MedicalOfficeManagement.ViewModels.Staff;

public class StaffListItemViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleLabel { get; set; } = string.Empty;
    public string? Detail { get; set; }
}
