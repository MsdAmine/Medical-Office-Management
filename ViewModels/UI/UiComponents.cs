using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;

namespace MedicalOfficeManagement.ViewModels.UI
{
    public class PageHeaderModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? ContextTag { get; set; }
        public string? RoleBadge { get; set; }
        public string? RoleTone { get; set; }
        public IEnumerable<BreadcrumbItem> Breadcrumbs { get; set; } = Array.Empty<BreadcrumbItem>();
        public IEnumerable<ToolbarAction> Actions { get; set; } = Array.Empty<ToolbarAction>();
        public string? Meta { get; set; }
    }

    public class BreadcrumbItem
    {
        public string Label { get; set; } = string.Empty;
        public string? Url { get; set; }
        public bool Current { get; set; }
    }

    public class ToolbarAction
    {
        public string Label { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Href { get; set; }
        public string Style { get; set; } = "primary";
        public bool Disabled { get; set; }
        public string? Tooltip { get; set; }
        public string? DataTarget { get; set; }
        public bool IsButton { get; set; }
    }

    public class KpiCardModel
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Delta { get; set; }
        public string DeltaTone { get; set; } = "neutral";
        public string? Helper { get; set; }
        public string? Pill { get; set; }
        public string? Icon { get; set; }
    }

    public class KpiGridModel
    {
        public IEnumerable<KpiCardModel> Items { get; set; } = Array.Empty<KpiCardModel>();
        public int Columns { get; set; } = 4;
    }

    public class EmptyStateModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Body { get; set; }
        public string? Icon { get; set; }
        public ToolbarAction? PrimaryAction { get; set; }
        public ToolbarAction? SecondaryAction { get; set; }
        public string Tone { get; set; } = "info";
    }

    public class TableShellModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public IEnumerable<ToolbarAction> Actions { get; set; } = Array.Empty<ToolbarAction>();
        public Func<dynamic, IHtmlContent>? TableContent { get; set; }
    }

    public class ActionMenuModel
    {
        public string Label { get; set; } = "Actions";
        public IEnumerable<ActionMenuItem> Items { get; set; } = Array.Empty<ActionMenuItem>();
    }

    public class ActionMenuItem
    {
        public string Label { get; set; } = string.Empty;
        public string? Href { get; set; }
        public string Tone { get; set; } = "default";
        public bool Disabled { get; set; }
        public bool DividerBefore { get; set; }
    }

    public class FilterChipModel
    {
        public string Label { get; set; } = string.Empty;
        public string Tone { get; set; } = "muted";
    }

    public class FilterChipsModel
    {
        public string Title { get; set; } = "Active filters";
        public IEnumerable<FilterChipModel> Chips { get; set; } = Array.Empty<FilterChipModel>();
    }

    public class PresetOption
    {
        public string Label { get; set; } = string.Empty;
        public string? Url { get; set; }
        public bool Active { get; set; }
        public string? Description { get; set; }
    }

    public class PresetsBarModel
    {
        public string Title { get; set; } = "Presets";
        public IEnumerable<PresetOption> Options { get; set; } = Array.Empty<PresetOption>();
    }
}
