using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Discography.Core.Enums
{
    public enum Instruments
    {
        [Display(Name = "Empty")]
        Empty = 0,
        [Display(Name = "Guitar")]
        Guitar = 1,
        [Display(Name = "Drums")]
        Drums = 2,
        [Display(Name = "Bass")]
        Bass = 4,
        [Display(Name = "Keyboard")]
        Keyboard = 8,
        [Display(Name = "Harmonica")]
        Harmonica = 16,
        [Display(Name = "Saxophone")]
        Saxophone = 32,
        [Display(Name = "Trumpet")]
        Trumpet = 64,
        [Display(Name = "Organ")]
        Organ = 128
    }
}
