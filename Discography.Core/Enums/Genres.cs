using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Discography.Core.Enums
{
    [Flags]
    public enum Genres
    {
        [Display(Name = "Empty")]
        Empty = 0,
        [Display(Name = "Blues")]
        Blues = 1,
        [Display(Name = "Rock")]
        Rock = 2,
        [Display(Name = "Jazz")]
        Jazz = 4,
        [Display(Name = "Funk")]
        Funk = 8,
        [Display(Name = "Folk")]
        Folk = 16,
        [Display(Name = "Pop")]
        Pop = 32,
        [Display(Name = "Classical")]
        Classical = 64,
        [Display(Name = "Hip Hop")]
        HipHop = 128
    }
}
