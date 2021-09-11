using Discography.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discography.Data.Dtos.SongDtos
{
    public class SongResourceParameters : ResourceParameters
    {
        public string Genre { get; set; }

        public DateTime? FromDateReleased { get; set; } = null;

        public DateTime? ToDateReleased { get; set; } = null;
    }
}
