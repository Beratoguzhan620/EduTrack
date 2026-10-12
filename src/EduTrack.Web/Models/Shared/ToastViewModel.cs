namespace EduTrack.Web.Models.Shared;

public enum ToastType
{
    Success,
    Error,
}

/// <summary>Short notification rendered by <c>Partials/_Toast</c> and shown in the bottom-right stack.</summary>
public sealed record ToastViewModel(string Message, ToastType Type = ToastType.Success);
