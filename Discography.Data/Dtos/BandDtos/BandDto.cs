using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discography.Data.Models
{
    public class BandDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public int YearOfFormation { get; set; }

        public string AlsoKnownAs { get; set; }

        public string Description { get; set; }

        public List<string> ActivePeriods { get; set; }

        public List<string> Genres { get; set; }
    }
}
