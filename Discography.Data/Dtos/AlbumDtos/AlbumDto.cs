using System;
using System.Collections.Generic;
using System.Text;

namespace Discography.Data.Dtos.AlbumDtos
{
    public class AlbumDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Label { get; set; }

        public DateTime DateReleased { get; set; }

        public Guid BandId { get; set; }
    }
}
