// File: ViewModels/Dashboard/TaskViewModel.cs
namespace MedicalOfficeManagement.ViewModels.Dashboard
{
    public class TaskViewModel
    {
        public string Category { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Url { get; set; } = string.Empty;
        public string ColorClass { get; set; } = string.Empty;
    }
}
