using System;
using System.Collections.Generic;
using System.Text;

namespace Discography.Data.Dtos.SongDtos
{
    public class SongDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Duration { get; set; } 

        public DateTime? DateRecorded { get; set; }

        public DateTime? DateReleased { get; set; }

        public string Lyrics { get; set; }

        public List<string> Genres { get; set; }

        public Guid BandId { get; set; }

        public Guid? LyricistId { get; set; }

        public Guid? ComposerId { get; set; }

        public Guid? AlbumId { get; set; }
    }
}
