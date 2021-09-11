using Discography.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Discography.Data.Dtos.BandDtos
{
    public abstract class BandForManipulationDto
    {
        [Required(ErrorMessage = "You should fill out a name.")]
        public string Name { get; set; }

        public int YearOfFormation { get; set; }

        public string AlsoKnownAs { get; set; }

        public string Description { get; set; }

        public List<ActivePeriod> ActivePeriods { get; set; }

        public List<string> Genres { get; set; }
    }
}
