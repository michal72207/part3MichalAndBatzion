using System;
using System.Collections.Generic;
using System.Linq;
using GatherUp.Core.DO;
using GatherUp.Core.Interfaces;

namespace GatherUp.BL
{
    public record PollResults(Poll Poll, IEnumerable<QuestionResult> QuestionResults);

    public record QuestionResult(
        PollQuestion Question,
        IEnumerable<(string Option, int Count, double Percentage)> OptionStats
    );

    public class PollService
    {
        private readonly IRepository<Poll>        _pollRepo;
        private readonly IRepository<Event>       _eventRepo;
        private readonly IRepository<Participant> _participantRepo;
        private readonly IEmailService            _emailService;
        private readonly IEventNotifier?          _notifier;

        public event Action<int, int>? PollCreated;
        public event Action<int, int>? PollAnswered;

        public PollService(
            IRepository<Poll> pollRepo,
            IRepository<Event> eventRepo,
            IRepository<Participant> participantRepo,
            IEmailService emailService,
            IEventNotifier? notifier = null)
        {
            _pollRepo        = pollRepo;
            _eventRepo       = eventRepo;
            _participantRepo = participantRepo;
            _emailService    = emailService;
            _notifier        = notifier;
        }

        public Poll CreatePoll(int eventId, string name, List<(string QuestionText, List<string> Options)> questions)
        {
            var ev = _eventRepo.GetById(eventId);

            int newId = _pollRepo.GetAll().Any() ? _pollRepo.GetAll().Max(p => p.Id) + 1 : 1;

            var poll = new Poll
            {
                Id = newId,
                Name = name,
                Questions = questions.Select((q, i) => new PollQuestion
                {
                    Id = i + 1,
                    QuestionText = q.QuestionText,
                    Options = q.Options
                }).ToList()
            };

            _pollRepo.Add(poll);

            if (!ev.PollIds.Contains(poll.Id))
                ev.PollIds.Add(poll.Id);
            _eventRepo.Update(ev);

            PollCreated?.Invoke(poll.Id, eventId);
            _notifier?.OnPollCreated(poll.Id, eventId);
            NotifyPollCreated(ev, poll.Name);

            return poll;
        }

        public void SubmitVote(int pollId, int questionId, int participantId, string answer)
        {
            var poll     = _pollRepo.GetById(pollId);
            var question = poll.Questions.FirstOrDefault(q => q.Id == questionId)
                ?? throw new ArgumentException($"שאלה {questionId} לא נמצאה");

            // מחיקת הצבעה ישנה אם קיימת
            var existing = question.ParticipantAnswers.FirstOrDefault(r => r.ParticipantId == participantId);
            if (existing != null)
                question.ParticipantAnswers.Remove(existing);

            question.ParticipantAnswers.Add(new PollResponse { ParticipantId = participantId, Answer = answer });
            _pollRepo.Update(poll);

            PollAnswered?.Invoke(participantId, pollId);
            _notifier?.OnPollAnswered(participantId, pollId);
        }

        public PollResults GetPollResults(int pollId)
        {
            var poll = _pollRepo.GetById(pollId);

            var results = poll.Questions.Select(q =>
            {
                int total = q.ParticipantAnswers.Count;
                var stats = q.Options.Select(opt =>
                {
                    int count    = q.ParticipantAnswers.Count(r => r.Answer == opt);
                    double pct   = total > 0 ? Math.Round((double)count / total * 100, 1) : 0;
                    return (opt, count, pct);
                });
                return new QuestionResult(q, stats);
            });

            return new PollResults(poll, results);
        }

        public bool IsPollOpen(int pollId) => _pollRepo.GetAll().Any(p => p.Id == pollId);

        private void NotifyPollCreated(Event ev, string pollName)
        {
            _participantRepo.GetAll()
                .Where(p => ev.ParticipantIds.Contains(p.Id)
                    && p.MailingPreferences.Contains(MailingPreference.PollCreated))
                .ToList()
                .ForEach(p => _emailService.Send(p.Email,
                    $"סקר חדש: {pollName}",
                    $"שלום {p.Name}, נוצר סקר חדש. אנא מלא/י אותו."));
        }
    }
}
