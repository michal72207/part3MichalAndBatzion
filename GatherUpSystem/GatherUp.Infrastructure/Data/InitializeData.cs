using System;
using System.Collections.Generic;
using GatherUp.Core.DO;
using GatherUp.Core.Interfaces;

namespace GatherUp.Infrastructure.Data
{
    public static class InitializeData
    {
        public static void Initialize(
            IRepository<Event> eventRepo,
            IRepository<Participant> participantRepo,
            IRepository<VendorAllocation> vendorRepo,
            IRepository<Poll> pollRepo)
        {
            var p1 = new Participant { Id = 101, Name = "אביטל ישראלי", Email = "avital@gmail.com",  IsAttending = true, HasPaid = true,  AmountContributed = 150 };
            var p2 = new Participant { Id = 102, Name = "אביגיל לוי",   Email = "avigail@gmail.com", IsAttending = null, HasPaid = false, AmountContributed = 0 };

            participantRepo.Add(p1);
            participantRepo.Add(p2);

            var vendor = new VendorAllocation { Id = 1, Name = "קייטרינג אסאדו", AmountOwed = 4500 };
            vendorRepo.Add(vendor);

            var poll1 = new Poll
            {
                Id = 1,
                Name = "שאלון מיקום ותאריך התחלתי",
                Questions = new List<PollQuestion>
                {
                    new PollQuestion { Id = 1, QuestionText = "באיזה תאריך עדיף לקיים את המפגש?", Options = new List<string> { "01/07", "05/07", "10/07" } }
                }
            };

            var poll2 = new Poll
            {
                Id = 2,
                Name = "סקר המשך - כיבוד",
                Questions = new List<PollQuestion>
                {
                    new PollQuestion { Id = 2, QuestionText = "איזה סגנון כיבוד להזמין?", Options = new List<string> { "בשרי", "חלבי", "טבעוני" } }
                }
            };

            pollRepo.Add(poll1);
            pollRepo.Add(poll2);

            eventRepo.Add(new Event
            {
                Id = 500,
                Name = "שבת גיבוש משפחתית 2026",
                Date = DateTime.Now.AddDays(30),
                Location = "ירושלים",
                PricePerParticipant = 150,
                EventManagerId = 1,
                EventHostId    = 2,
                ParticipantIds = new List<int> { p1.Id, p2.Id },
                VendorIds      = new List<int> { vendor.Id },
                PollIds        = new List<int> { poll1.Id, poll2.Id }
            });
        }
    }
}
