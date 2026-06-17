using System.Linq;
using GatherUp.Core.DO;
using GatherUp.Core.Interfaces;

namespace GatherUp.BL
{
    public class EventNotifierService : IEventNotifier
    {
        private readonly IRepository<Participant> _participantRepo;
        private readonly IRepository<Event>       _eventRepo;
        private readonly IEmailService            _emailService;

        public EventNotifierService(
            IRepository<Participant> participantRepo,
            IRepository<Event> eventRepo,
            IEmailService emailService)
        {
            _participantRepo = participantRepo;
            _eventRepo       = eventRepo;
            _emailService    = emailService;
        }

        public void OnAttendanceConfirmed(int participantId, int eventId)
        {
            var ev      = _eventRepo.GetById(eventId);
            var manager = _participantRepo.GetAll().FirstOrDefault(p => p.Id == ev.EventManagerId);
            if (manager?.MailingPreferences.Contains(MailingPreference.AttendanceConfirmed) == true)
            {
                var who = _participantRepo.GetAll().FirstOrDefault(p => p.Id == participantId);
                _emailService.Send(manager.Email, "אישור הגעה התקבל",
                    $"המשתתף {who?.Name ?? participantId.ToString()} אישר הגעה לאירוע {ev.Name}");
            }
        }

        public void OnPaymentReceived(int participantId, int eventId)
        {
            var ev      = _eventRepo.GetById(eventId);
            var manager = _participantRepo.GetAll().FirstOrDefault(p => p.Id == ev.EventManagerId);
            if (manager?.MailingPreferences.Contains(MailingPreference.PaymentReceived) == true)
            {
                var who = _participantRepo.GetAll().FirstOrDefault(p => p.Id == participantId);
                _emailService.Send(manager.Email, "תשלום התקבל",
                    $"המשתתף {who?.Name ?? participantId.ToString()} שילם עבור אירוע {ev.Name}");
            }
        }

        public void OnPollAnswered(int participantId, int pollId)
        {
            var ev = _eventRepo.GetAll().FirstOrDefault(e => e.PollIds.Contains(pollId));
            if (ev == null) return;

            var manager = _participantRepo.GetAll().FirstOrDefault(p => p.Id == ev.EventManagerId);
            if (manager?.MailingPreferences.Contains(MailingPreference.PollAnswered) == true)
            {
                var who = _participantRepo.GetAll().FirstOrDefault(p => p.Id == participantId);
                _emailService.Send(manager.Email, "תשובה חדשה לסקר",
                    $"המשתתף {who?.Name ?? participantId.ToString()} ענה על סקר {pollId} באירוע {ev.Name}");
            }
        }

        public void OnPollCreated(int pollId, int eventId)
        {
            var ev = _eventRepo.GetById(eventId);
            _participantRepo.GetAll()
                .Where(p => ev.ParticipantIds.Contains(p.Id)
                    && p.MailingPreferences.Contains(MailingPreference.PollCreated))
                .ToList()
                .ForEach(p => _emailService.Send(p.Email, "סקר חדש נוצר",
                    $"שלום {p.Name}, נוצר סקר חדש באירוע {ev.Name}."));
        }

        public void OnEventDetailsChanged(int eventId)
        {
            var ev = _eventRepo.GetById(eventId);
            _participantRepo.GetAll()
                .Where(p => ev.ParticipantIds.Contains(p.Id)
                    && p.MailingPreferences.Contains(MailingPreference.EventChanges))
                .ToList()
                .ForEach(p => _emailService.Send(p.Email, "פרטי האירוע עודכנו",
                    $"שלום {p.Name}, חל עדכון בפרטי האירוע {ev.Name}."));
        }
    }
}
