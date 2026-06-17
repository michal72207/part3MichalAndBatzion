using System.Xml.Serialization;
using GatherUp.Core.Interfaces;

namespace GatherUp.Core.DO
{
    public abstract class Person : IEntity
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        public string Name  { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
