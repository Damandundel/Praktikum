namespace Praktikum.Data.Models.Enums;

/// <summary>Вид запис в професионалния профил (CV).</summary>
public enum ProfileEntryType
{
    Education = 0,
    Internship = 1,
    Work = 2,
    Achievement = 3,
}

/// <summary>Състояние на връзка между двама потребители.</summary>
public enum ConnectionStatus
{
    Pending = 0,
    Accepted = 1,
}

/// <summary>Състояние на предадена работа по задание.</summary>
public enum SubmissionStatus
{
    Submitted = 0,
    Reviewed = 1,
    NeedsRevision = 2,
}

/// <summary>Решение на работодателя след приключил стаж.</summary>
public enum HiringDecision
{
    NotDecided = 0,
    Offered = 1,
    Declined = 2,
}

/// <summary>Тема на разговор в чата.</summary>
public enum ConversationTopic
{
    General = 0,
    PositionInquiry = 1,
    AssignmentComment = 2,
    InternshipFeedback = 3,
}
