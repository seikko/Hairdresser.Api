namespace Hairdresser.Api.Services;

public interface IWhatsAppService
{
    Task<bool> SendTextMessageAsync(string to, string message);

    Task<bool> SendLocationMessageAsync(
        string to,
        double latitude,
        double longitude,
        string name,
        string address);
    Task<bool> SendInteractiveButtonsAsync(string to, string bodyText, List<(string id, string title)> buttons);
    Task<bool> SendInteractiveListAsync(string to, string bodyText, string buttonText, List<(string id, string title, string? description)> rows);
    Task MarkMessageAsReadAsync(string messageId);
}