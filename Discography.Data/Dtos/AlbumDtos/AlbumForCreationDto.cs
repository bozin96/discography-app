using Discography.Data.Dtos.SongDtos;
using Discography.Data.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discography.Data.Dtos.AlbumDtos
{
    public class AlbumForCreationDto : AlbumForManipulationDto
    {
        public ICollection<SongForCreationDto> Songs { get; set; } = new List<SongForCreationDto>();
    }
}
