using System.Collections.Concurrent;
using Hairdresser.Api.Models;

namespace Hairdresser.Api.Services;

/// <summary>
/// In-memory conversation state management
/// For production, consider using Redis or a distributed cache
/// </summary>
public class ConversationService(ILogger<ConversationService> logger) : IConversationService
{
    private readonly ConcurrentDictionary<string, ConversationState> _states = new();

    public Task<ConversationState?> GetStateAsync(string phoneNumber)
    {
        _states.TryGetValue(phoneNumber, out var state);
        return Task.FromResult(state);
    }

    public Task UpdateStateAsync(ConversationState state)
    {
        state.LastUpdated = DateTime.UtcNow;
        _states[state.PhoneNumber] = state;
        logger.LogInformation("Updated conversation state for {PhoneNumber} to step {Step}", state.PhoneNumber, state.CurrentStep);
        return Task.CompletedTask;
    }

    public Task ClearStateAsync(string phoneNumber)
    {
        _states.TryRemove(phoneNumber, out _);
        logger.LogInformation("Cleared conversation state for {PhoneNumber}", phoneNumber);
        return Task.CompletedTask;
    }
}