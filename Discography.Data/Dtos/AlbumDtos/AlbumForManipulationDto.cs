using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Discography.Data.Dtos.AlbumDtos
{
    public abstract class AlbumForManipulationDto
    {
        [Required(ErrorMessage = "You should fill out a title.")]
        public string Title { get; set; }

        public string Label { get; set; }

        public DateTime DateReleased { get; set; }
    }
}
