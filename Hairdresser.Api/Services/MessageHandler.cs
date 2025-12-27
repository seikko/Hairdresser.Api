using System.Globalization;
using Hairdresser.Api.Models;

namespace Hairdresser.Api.Services
{
    public class MessageHandler : IMessageHandler
    {
        private readonly IWhatsAppService _whatsAppService;
        private readonly IBookingService _bookingService;
        private readonly IConversationService _conversationService;
        private readonly ILogger<MessageHandler> _logger;

        public MessageHandler(
            IWhatsAppService whatsAppService,
            IBookingService bookingService,
            IConversationService conversationService,
            ILogger<MessageHandler> logger)
        {
            _whatsAppService = whatsAppService;
            _bookingService = bookingService;
            _conversationService = conversationService;
            _logger = logger;
        }

        public async Task HandleIncomingMessageAsync(string from, string messageText, string? senderName)
        {
            _logger.LogInformation("Processing message from {From}: {Message}", from, messageText);

            var user = await _bookingService.GetOrCreateUserAsync(from, senderName);

            var state = await _conversationService.GetStateAsync(from);

            if (messageText.Trim().ToLower().StartsWith("/randevu") || messageText.Trim().ToLower() == "randevu")
            {
                await StartBookingFlowAsync(from);
                return;
            }

            if (messageText.Trim().ToLower().StartsWith("/iptal"))
            {
                await StartCancellationFlowAsync(from, user.Id);
                return;
            }

            if (messageText.Trim().ToLower() == "/yardim" || messageText.Trim().ToLower() == "yardım")
            {
                await SendHelpMessageAsync(from);
                return;
            }

            if (state != null)
            {
                await ProcessConversationStepAsync(from, messageText, state, user.Id);
            }
            else
            {
                await SendWelcomeMessageAsync(from);
            }
        }

        public async Task HandleInteractiveReplyAsync(string from, string replyId, string replyTitle)
        {
            _logger.LogInformation("Processing interactive reply from {From}: {ReplyId}", from, replyId);

            var user = await _bookingService.GetOrCreateUserAsync(from, null);
            var state = await _conversationService.GetStateAsync(from);

            if (state == null)
            {
                await SendWelcomeMessageAsync(from);
                return;
            }

            if (replyId.StartsWith("worker_"))
            {
                await HandleWorkerSelectionAsync(from, replyId, state);
            }
            else if (replyId.StartsWith("date_"))
            {
                await HandleDateSelectionAsync(from, replyId, state, user.Id);
            }
            else if (replyId.StartsWith("time_"))
            {
                await HandleTimeSelectionAsync(from, replyId, state, user.Id);
            }
            else if (replyId.StartsWith("cancel_"))
            {
                await HandleAppointmentCancellationAsync(from, replyId, user.Id);
            }
            else if (replyId == "confirm_yes")
            {
                await ConfirmAppointmentAsync(from, state, user.Id);
            }
            else if (replyId == "confirm_no")
            {
                await _conversationService.ClearStateAsync(from);
                await _whatsAppService.SendTextMessageAsync(from, "Randevu oluşturma iptal edildi. Yeni randevu için /randevu yazabilirsiniz.");
            }
        }

        private async Task SendWelcomeMessageAsync(string from)
        {
            var message = @"👋 Hoş geldiniz! Kuaför randevu sistemine hoş geldiniz.

📅 *Randevu almak için:* /randevu
❌ *Randevuyu iptal etmek için:* /iptal
❓ *Yardım için:* /yardim";

            await _whatsAppService.SendTextMessageAsync(from, message);
        }

        private async Task SendHelpMessageAsync(string from)
        {
            var message = @"ℹ️ *Yardım Menüsü*

*Kullanılabilir Komutlar:*
📅 `/randevu` - Yeni randevu oluştur
❌ `/iptal` - Mevcut randevuyu iptal et
❓ `/yardim` - Bu yardım mesajını göster

*Nasıl Çalışır:*
1. `/randevu` yazın
2. Çalışan seçin
3. Tarih seçin
4. Müsait saatleri görün
5. Saat seçin
6. Randevunuzu onaylayın

Herhangi bir sorunuz varsa bizimle iletişime geçebilirsiniz!";

            await _whatsAppService.SendTextMessageAsync(from, message);
        }

        private async Task StartBookingFlowAsync(string from)
        {
            var workers = await _bookingService.GetActiveWorkersAsync();

            if (workers.Count == 0)
            {
                await _whatsAppService.SendTextMessageAsync(from, "❌ Şu anda müsait çalışan bulunmamaktadır. Lütfen daha sonra tekrar deneyin.");
                return;
            }

            var workerList = workers.Select(w => (
                $"worker_{w.Id}",
                w.Name,
                w.Specialty ?? "Kuaför"
            )).ToList();

            var state = new ConversationState
            {
                PhoneNumber = from,
                CurrentStep = ConversationStep.AwaitingWorker
            };

            await _conversationService.UpdateStateAsync(state);

            await _whatsAppService.SendInteractiveListAsync(
                from,
                "💇 Lütfen randevu almak istediğiniz çalışanı seçin:",
                "Çalışan Seç",
                workerList!
            );
        }

        private async Task HandleWorkerSelectionAsync(string from, string replyId, ConversationState state)
        {
            var workerIdString = replyId.Replace("worker_", "");
            if (!int.TryParse(workerIdString, out var workerId))
            {
                await _whatsAppService.SendTextMessageAsync(from, "❌ Geçersiz seçim. Lütfen tekrar deneyin.");
                return;
            }

            var worker = await _bookingService.GetWorkerByIdAsync(workerId);
            if (worker == null)
            {
                await _whatsAppService.SendTextMessageAsync(from, "❌ Çalışan bulunamadı. Lütfen tekrar deneyin.");
                return;
            }

            state.SelectedWorkerId = workerId;
            state.SelectedWorkerName = worker.Name;
            state.CurrentStep = ConversationStep.AwaitingDate;
            await _conversationService.UpdateStateAsync(state);

            var availableDates = new List<(string id, string title, string? description)>();
            var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3));

            for (int i = 0; i < 7; i++)
            {
                var date = today.AddDays(i);
                var dayName = date.ToString("dddd", new CultureInfo("tr-TR"));
                var formattedDate = date.ToString("dd MMMM yyyy", new CultureInfo("tr-TR"));

                availableDates.Add((
                    $"date_{date:yyyy-MM-dd}",
                    $"{dayName}",
                    formattedDate
                ));
            }

            await _whatsAppService.SendInteractiveListAsync(
                from,
                $"✅ Çalışan: *{worker.Name}*\n\n📅 Lütfen randevu için bir tarih seçin:",
                "Tarih Seç",
                availableDates
            );
        }

        private async Task HandleDateSelectionAsync(string from, string replyId, ConversationState state, int userId)
        {
            var dateString = replyId.Replace("date_", "");
            if (!DateOnly.TryParse(dateString, out var selectedDate))
            {
                await _whatsAppService.SendTextMessageAsync(from, "❌ Geçersiz tarih. Lütfen tekrar deneyin.");
                return;
            }

            if (!state.SelectedWorkerId.HasValue)
            {
                await _whatsAppService.SendTextMessageAsync(from, "❌ Lütfen önce bir çalışan seçin. /randevu");
                await _conversationService.ClearStateAsync(from);
                return;
            }

            state.SelectedDate = selectedDate;
            state.CurrentStep = ConversationStep.AwaitingTime;
            await _conversationService.UpdateStateAsync(state);

            var availableSlots = await _bookingService.GetAvailableTimeSlotsForWorkerAsync(state.SelectedWorkerId.Value, selectedDate);

            if (availableSlots.Count == 0)
            {
                await _whatsAppService.SendTextMessageAsync(from, $"❌ {state.SelectedWorkerName} için bu tarihte müsait saat yok. Lütfen başka bir tarih seçin. /randevu");
                await _conversationService.ClearStateAsync(from);
                return;
            }

            var timeButtons = availableSlots.Take(10).Select(time => (
                $"time_{time:HH:mm}",
                time.ToString("HH:mm"),
                (string?)null
            )).ToList();

            var formattedDate = selectedDate.ToString("dd MMMM yyyy", new CultureInfo("tr-TR"));
            await _whatsAppService.SendInteractiveListAsync(
                from,
                $"✅ Çalışan: *{state.SelectedWorkerName}*\n📅 Tarih: *{formattedDate}*\n\n🕐 Lütfen bir saat seçin:",
                "Saat Seç",
                timeButtons
            );
        }

        private async Task HandleTimeSelectionAsync(string from, string replyId, ConversationState state, int userId)
        {
            var timeString = replyId.Replace("time_", "");
            if (!TimeOnly.TryParse(timeString, out var selectedTime))
            {
                await _whatsAppService.SendTextMessageAsync(from, "❌ Geçersiz saat. Lütfen tekrar deneyin.");
                return;
            }

            state.SelectedTime = selectedTime;
            state.CurrentStep = ConversationStep.ConfirmingAppointment;
            await _conversationService.UpdateStateAsync(state);

            var formattedDate = state.SelectedDate!.Value.ToString("dd MMMM yyyy", new CultureInfo("tr-TR"));
            var formattedTime = selectedTime.ToString("HH:mm");

            await _whatsAppService.SendInteractiveButtonsAsync(
                from,
                $"✅ *Randevu Onayı*\n\n💇 Çalışan: *{state.SelectedWorkerName}*\n📅 Tarih: *{formattedDate}*\n🕐 Saat: *{formattedTime}*\n\nRandevunuzu onaylıyor musunuz?",
                new List<(string id, string title)>
                {
                    ("confirm_yes", "✅ Evet, Onayla"),
                    ("confirm_no", "❌ Hayır, İptal")
                }
            );
        }

        private async Task ConfirmAppointmentAsync(string from, ConversationState state, int userId)
        {
            if (!state.SelectedDate.HasValue || !state.SelectedTime.HasValue || !state.SelectedWorkerId.HasValue)
            {
                await _whatsAppService.SendTextMessageAsync(from, "❌ Bir hata oluştu. Lütfen tekrar deneyin. /randevu");
                await _conversationService.ClearStateAsync(from);
                return;
            }

            var appointment = await _bookingService.CreateAppointmentAsync(
                userId,
                state.SelectedWorkerId.Value,
                state.SelectedDate.Value,
                state.SelectedTime.Value,
                state.ServiceType
            );

            if (appointment == null)
            {
                await _whatsAppService.SendTextMessageAsync(from, "❌ Bu saat artık müsait değil. Lütfen başka bir saat seçin. /randevu");
                await _conversationService.ClearStateAsync(from);
                return;
            }

            var formattedDate = state.SelectedDate.Value.ToString("dd MMMM yyyy", new CultureInfo("tr-TR"));
            var formattedTime = state.SelectedTime.Value.ToString("HH:mm");

            var confirmationMessage = $@"✅ *Randevunuz Oluşturuldu!*

💇 Çalışan: *{state.SelectedWorkerName}*
📅 Tarih: *{formattedDate}*
🕐 Saat: *{formattedTime}*
📝 Randevu No: *{appointment.Id}*

Randevunuzu iptal etmek için: /iptal

Görüşmek üzere! 👋";

            await _whatsAppService.SendTextMessageAsync(from, confirmationMessage);
            await _conversationService.ClearStateAsync(from);
        }

        private async Task StartCancellationFlowAsync(string from, int userId)
        {
            var appointments = await _bookingService.GetUserAppointmentsAsync(userId);

            if (appointments.Count == 0)
            {
                await _whatsAppService.SendTextMessageAsync(from, "❌ Aktif randevunuz bulunmamaktadır.");
                return;
            }

            var appointmentList = appointments.Select(a => (
                $"cancel_{a.Id}",
                $"{a.AppointmentDate:dd/MM/yyyy} {a.AppointmentTime:HH:mm}",
                (string?)$"{a.Worker?.Name ?? "Kuaför"} - No: {a.Id}"
            )).ToList();

            await _whatsAppService.SendInteractiveListAsync(
                from,
                "❌ İptal etmek istediğiniz randevuyu seçin:",
                "Randevu Seç",
                appointmentList
            );
        }

        private async Task HandleAppointmentCancellationAsync(string from, string replyId, int userId)
        {
            var appointmentIdString = replyId.Replace("cancel_", "");
            if (!int.TryParse(appointmentIdString, out var appointmentId))
            {
                await _whatsAppService.SendTextMessageAsync(from, "❌ Geçersiz randevu. Lütfen tekrar deneyin.");
                return;
            }

            var success = await _bookingService.CancelAppointmentAsync(userId, appointmentId);

            if (success)
            {
                await _whatsAppService.SendTextMessageAsync(from, $"✅ Randevunuz (No: {appointmentId}) başarıyla iptal edildi.");
            }
            else
            {
                await _whatsAppService.SendTextMessageAsync(from, "❌ Randevu iptal edilemedi. Lütfen daha sonra tekrar deneyin.");
            }
        }

        private async Task ProcessConversationStepAsync(string from, string messageText, ConversationState state, int userId)
        {
            await _whatsAppService.SendTextMessageAsync(from, "Lütfen yukarıdaki seçeneklerden birini seçin veya /randevu yazarak yeni bir randevu oluşturun.");
        }
    }
}

