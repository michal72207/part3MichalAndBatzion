using System.Collections.Generic;
using System.Xml.Serialization;

namespace GatherUp.Core.DO
{
    public class PollQuestion
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        public string QuestionText { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
        public List<PollResponse> ParticipantAnswers { get; set; } = new();
    }
}
