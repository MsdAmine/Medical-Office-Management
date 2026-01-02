using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.ViewModels
{
    public class SettingToggleViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Enabled { get; set; }
    }

    public class SettingsIndexViewModel
    {
        public IEnumerable<SettingToggleViewModel> NotificationSettings { get; set; } = Array.Empty<SettingToggleViewModel>();
        public IEnumerable<SettingToggleViewModel> SecuritySettings { get; set; } = Array.Empty<SettingToggleViewModel>();
        public IEnumerable<SettingToggleViewModel> PreferenceSettings { get; set; } = Array.Empty<SettingToggleViewModel>();
    }
}
