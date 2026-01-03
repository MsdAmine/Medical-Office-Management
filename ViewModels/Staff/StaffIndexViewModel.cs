using System.Collections.Generic;
using System.Linq;

namespace MedicalOfficeManagement.ViewModels.Staff;

public class StaffIndexViewModel
{
    public StaffCreateViewModel StaffForm { get; set; } = new();
    public IEnumerable<StaffListItemViewModel> Medecins { get; set; } = Enumerable.Empty<StaffListItemViewModel>();
    public IEnumerable<StaffListItemViewModel> Secretaires { get; set; } = Enumerable.Empty<StaffListItemViewModel>();
    public string? StatusMessage { get; set; }
}
