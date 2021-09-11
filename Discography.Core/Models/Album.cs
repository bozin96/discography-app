using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Discography.Core.Models
{
    public class Album : BaseEntity
    {
        [Required]
        public string Title { get; set; }

        public string Label { get; set; }

        public DateTime  DateReleased { get; set; }

        public Guid BandId { get; set; }

        [ForeignKey("BandId")]
        public Band Band { get; set; }

        public List<Song> Songs { get; set; }

        public Album()
        {

        }

    }
}
