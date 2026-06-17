using System.Collections.Generic;
using System.Xml.Serialization;
using GatherUp.Core.Interfaces;

namespace GatherUp.Core.DO
{
    public class VendorAllocation : IEntity
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        public string Name      { get; set; } = string.Empty;
        public decimal AmountOwed { get; set; }
        public List<int> ReceiptIds { get; set; } = new();
    }
}
