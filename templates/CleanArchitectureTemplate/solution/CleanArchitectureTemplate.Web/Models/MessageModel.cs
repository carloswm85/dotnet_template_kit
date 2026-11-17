using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace CleanArchitectureTemplate.Web.Models;

public enum MessageType
{
    Success,
    Error,
    Warning,
    Info,
    Primary,
    Secondary,
}

/// <summary>
/// A dismissible alert rendered by <c>Views/Shared/_Message.cshtml</c>.
/// </summary>
public sealed class MessageModel
{
    public string? Title { get; init; }
    public string Content { get; init; } = string.Empty;
    public MessageType Type { get; init; } = MessageType.Info;

    public bool HasMessage => !string.IsNullOrWhiteSpace(Content);

    public string ToBootstrapClass() =>
        Type switch
        {
            MessageType.Success => "alert-success",
            MessageType.Error => "alert-danger",
            MessageType.Warning => "alert-warning",
            MessageType.Info => "alert-info",
            MessageType.Primary => "alert-primary",
            _ => "alert-secondary",
        };

    public string ToIconClass() =>
        Type switch
        {
            MessageType.Success => "bi-check-circle-fill",
            MessageType.Error => "bi-x-octagon-fill",
            MessageType.Warning => "bi-exclamation-triangle-fill",
            MessageType.Primary => "bi-bell-fill",
            _ => "bi-info-circle-fill",
        };
}

/// <summary>
/// Flash messages that survive a redirect (Post/Redirect/Get). The layout
/// renders the pending message once.
/// </summary>
/// <example>
/// TempData.SetMessage(new MessageModel { Content = "Saved.", Type = MessageType.Success });
/// return RedirectToAction(nameof(Index));
/// </example>
public static class TempDataMessageExtensions
{
    private const string Key = "FlashMessage";

    public static void SetMessage(this ITempDataDictionary tempData, MessageModel message) =>
        tempData[Key] = JsonSerializer.Serialize(message);

    public static MessageModel? GetMessage(this ITempDataDictionary tempData) =>
        tempData[Key] is string json ? JsonSerializer.Deserialize<MessageModel>(json) : null;
}