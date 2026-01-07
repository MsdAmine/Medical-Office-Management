namespace MedicalOfficeManagement.Models;

public enum InvoiceStatus
{
    Draft = 0,
    Pending = 1,
    Paid = 2,
    Overdue = 3,
    Cancelled = 4
}

public enum AppointmentStatus
{
    Scheduled = 0,
    Completed = 1,
    Cancelled = 2,
    PendingApproval = 3,
    NoShow = 4
}

public enum LabResultStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}

public enum LabResultPriority
{
    Routine = 0,
    Urgent = 1,
    Stat = 2
}

public enum ReportStatus
{
    Draft = 0,
    Generated = 1,
    Archived = 2
}

public enum InventoryStatus
{
    InStock = 0,
    LowStock = 1,
    OutOfStock = 2,
    Discontinued = 3
}
