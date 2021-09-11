using System;
using System.Collections.Generic;
using System.Text;

namespace Discography.Data.Dtos.MusicianDtos
{
    public class MusicianDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string AlsoKnownAs { get; set; }

        public string Biography { get; set; }

        public DateTime DateOfBirth { get; set; }

        public DateTime? DateOfDeath { get; set; }

        public List<string> Instruments { get; set; }

        public Guid BandId { get; set; }
    }
}
