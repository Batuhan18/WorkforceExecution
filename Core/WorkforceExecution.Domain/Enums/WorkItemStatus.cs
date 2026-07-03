namespace WorkforceExecution.Domain.Enums;

public enum WorkItemStatus
{
    Planned = 0,            // Teknik Ofis olusturdu, HoM'a atandi
    InProgress = 1,         // HoM crew olusturdu, is yurutuluyor
    PendingHomApproval = 2, // Gun sonu fact girildi, HoM onayina dustu
    PendingSiteChief = 3,   // HoM onayladi, Site Chief bekliyor
    PendingPm = 4,          // Site Chief onayladi, PM bekliyor
    Approved = 5,           // PM onayladi -> Daily Report'a girer
    Rejected = 6            // Reddedildi (comment ile InProgress'e geri doner)
}
    