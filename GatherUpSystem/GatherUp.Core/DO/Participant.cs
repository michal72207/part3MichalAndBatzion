using System.Collections.Generic;

namespace GatherUp.Core.DO
{
    public class Participant : Person
    {
        public bool? IsAttending { get; set; }
        public bool HasPaid { get; set; }
        public decimal AmountContributed { get; set; }
        public List<MailingPreference> MailingPreferences { get; set; } = new();
    }
}
