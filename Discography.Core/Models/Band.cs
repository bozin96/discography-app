using Discography.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Discography.Core.Models
{
    public class Band : BaseEntity
    {
        [Required]
        public string Name { get; set; }

        public int YearOfFormation { get; set; }

        public string AlsoKnownAs { get; set; }

        public string Description { get; set; }

        public List<ActivePeriod> ActivePeriods { get; set; } = new List<ActivePeriod>(); // Dodaj validaciju da samo jedan moze da bude stillActive i to poslednji koji ima samo jednu godinu

        public Genres Genres { get; set; }

        public Band()
        {

        }

    }
}
