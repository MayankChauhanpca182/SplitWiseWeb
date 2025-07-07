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
    MemberAdded = 2,
    MemberRemoved = 3,
    GroupExpenseAdded = 4,
    GroupExpenseUpdated = 5,
    Paid = 6
}
