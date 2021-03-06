using System.Collections.Generic;

namespace Soli.Models
{
    public class TerritoryDetail : Territory
    {
        public string Color { get; set; }
        public IEnumerable<string> Zips { get; set; }
        public IEnumerable<string> Churches { get; set; }
    }
}
