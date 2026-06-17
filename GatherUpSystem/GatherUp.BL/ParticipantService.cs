using System;
using System.Collections.Generic;
using System.Linq;
using GatherUp.Core.DO;
using GatherUp.Core.Interfaces;

namespace GatherUp.BL
{
    public class ParticipantService
    {
        private readonly IRepository<Participant> _participantRepo;
        private readonly IRepository<Event> _eventRepo;
        private readonly IEmailService _emailService;
        private readonly IEventNotifier? _notifier;

        public event Action<int, int>? AttendanceConfirmed;
        public event Action<int, int>? InvitationSent;

        public ParticipantService(
            IRepository<Participant> participantRepo,
            IRepository<Event> eventRepo,
            IEmailService emailService,
            IEventNotifier? notifier = null)
        {
            _participantRepo = participantRepo;
            _eventRepo       = eventRepo;
            _emailService    = emailService;
            _notifier        = notifier;
        }

        public void AddParticipantToEvent(int eventId, Participant participant)
        {
            var ev = _eventRepo.GetById(eventId);
            _participantRepo.Add(participant);

            if (!ev.ParticipantIds.Contains(participant.Id))
                ev.ParticipantIds.Add(participant.Id);

            _eventRepo.Update(ev);
        }

        public void ConfirmAttendance(int participantId, int eventId, bool isAttending)
        {
            var participant = _participantRepo.GetById(participantId);
            participant.IsAttending = isAttending;

            if (isAttending && !participant.MailingPreferences.Contains(MailingPreference.EventChanges))
                participant.MailingPreferences.Add(MailingPreference.EventChanges);

            _participantRepo.Update(participant);

            AttendanceConfirmed?.Invoke(participantId, eventId);
            _notifier?.OnAttendanceConfirmed(participantId, eventId);
        }

        public void SendPendingInvitations(int eventId, string eventLink)
        {
            var ev = _eventRepo.GetById(eventId);

            var pending = _participantRepo.GetAll()
                .Where(p => ev.ParticipantIds.Contains(p.Id) && p.IsAttending == null);

            foreach (var p in pending)
            {
                _emailService.Send(p.Email, $"הזמנה לאירוע: {ev.Name}",
                    $"שלום {p.Name},\nאנא אשר/י הגעה דרך הלינק: {eventLink}");
                InvitationSent?.Invoke(p.Id, eventId);
            }
        }

        public IEnumerable<Participant> GetEventParticipants(int eventId)
        {
            var ev = _eventRepo.GetById(eventId);
            return _participantRepo.GetAll().Where(p => ev.ParticipantIds.Contains(p.Id));
        }

        public Participant? GetByEmail(string email) =>
            _participantRepo.GetAll().FirstOrDefault(p => p.Email == email);
    }
}
