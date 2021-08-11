using Discography.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Discography.Core.Models
{
    public class Musician : BaseEntity
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string MiddleName { get; set; }

        public string AlsoKnownAs { get; set; }

        public string Biography { get; set; }

        public DateTime DateOfBirth { get; set; }

        public DateTime? DateOfDeath { get; set; }

        public Instruments Instruments { get; set; }

        public Guid BandId { get; set; }

        [ForeignKey("BandId")]
        public Band Band { get; set; }

        public Musician()
        {

        }
    }
}
