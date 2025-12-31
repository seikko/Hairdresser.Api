using Hairdresser.Api.Models;

namespace Hairdresser.Api.Services;

public interface IConversationService
{
    Task<ConversationState?> GetStateAsync(string phoneNumber);
    Task UpdateStateAsync(ConversationState state);
    Task ClearStateAsync(string phoneNumber);
}