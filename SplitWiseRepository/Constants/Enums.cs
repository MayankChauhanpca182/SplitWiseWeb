namespace SplitWiseRepository.Constants;

public static class Enums
{
}

public enum FeriendRequestStatus
{
    Requested = 0,
    Accepted = 1,
    Rejected = 2
}

public enum SplitType
{
    Equally = 0,
    Unequally = 1,
    ByShare = 2,
    ByPercentage = 3
}

public enum ActivityType
{
    GroupCreated = 0,
    GroupUpdated = 1,
    GroupDeleted = 2,
    MemberAdded = 3,
    MemberRemoved = 4,
    GroupExpenseAdded = 5,
    GroupExpenseUpdated = 6,
    GroupPaymenent = 7,
    NonGroupPaymenent = 8,
    LeaveGroup = 9,
    ExpenseAdded = 10,
    ExpenseUpdated = 11
}
