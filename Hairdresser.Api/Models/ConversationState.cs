namespace Hairdresser.Api.Models
{
    /// <summary>
    /// Represents the conversation state for a user
    /// This is stored in-memory (could be Redis in production)
    /// </summary>
    public class ConversationState
    {
        public string PhoneNumber { get; set; } = null!;
        public ConversationStep CurrentStep { get; set; } = ConversationStep.Initial;
        public int? SelectedWorkerId { get; set; }
        public string? SelectedWorkerName { get; set; }
        public DateOnly? SelectedDate { get; set; }
        public TimeOnly? SelectedTime { get; set; }
        public string? ServiceType { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public enum ConversationStep
    {
        Initial,
        AwaitingWorker,
        AwaitingDate,
        AwaitingTime,
        AwaitingServiceType,
        ConfirmingAppointment,
        CancellingAppointment
    }
}

