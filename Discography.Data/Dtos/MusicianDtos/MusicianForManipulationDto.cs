using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Discography.Data.Dtos.MusicianDtos
{
    public abstract class MusicianForManipulationDto
    {
        [Required(ErrorMessage = "You should fill out a first name.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "You should fill out a last name.")]
        public string LastName { get; set; }

        public string MiddleName { get; set; }

        public string AlsoKnownAs { get; set; }

        public string Biography { get; set; }

        [Required(ErrorMessage = "You should fill out a date of birth.")]
        public DateTime DateOfBirth { get; set; }

        public DateTime? DateOfDeath { get; set; }

        public List<string> Instruments { get; set; }
    }
}
