using Discography.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discography.Data.Dtos.BandDtos
{
    public class BandResourceParameters : ResourceParameters
    {
        public int YearOfFormation { get; set; } = 0;

        public string Genre { get; set; }
    }
}
