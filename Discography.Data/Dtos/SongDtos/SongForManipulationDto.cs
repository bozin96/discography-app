using Discography.Data.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Discography.Data.Dtos.SongDtos
{
    //[SongDateRecorderMustBeLessThanDateReleased(
    //  ErrorMessage = "The recording date must be before the release date.")]
    public abstract class SongForManipulationDto
    {
        [Required(ErrorMessage = "You should fill out a title.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "You should fill out a duration.")]
        public string Duration { get; set; }

        public DateTime? DateRecorded { get; set; }

        public DateTime? DateReleased { get; set; }

        public string Lyrics { get; set; }

        public List<string> Genres { get; set; }

        public Guid? LyricistId { get; set; } = null;

        public Guid? ComposerId { get; set; } = null;

        public Guid? BandId { get; set; } = null;
    }
}
