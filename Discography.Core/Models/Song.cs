using Discography.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Discography.Core.Models
{
    public class Song : BaseEntity
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public int DurationInSeconds { get; set; } // koverzija u string minuti:sekunde

        public DateTime? DateRecorded { get; set; }

        public DateTime? DateReleased { get; set; }

        public string Lyrics { get; set; }
        public Genres Genres { get; set; }

        public Guid BandId { get; set; }

        [ForeignKey("BandId")]
        public Band Band { get; set; }

        public Guid? LyricistId { get; set; }

        [ForeignKey("LyricistId")]
        public Musician Lyricist  { get; set; }

        public Guid? ComposerId { get; set; }

        [ForeignKey("ComposerId")]
        public Musician Composer { get; set; }

        public Guid? AlbumId { get; set; }

        [ForeignKey("AlbumId")]
        public Album Album { get; set; }

        public Song()
        {

        }
    }
}
