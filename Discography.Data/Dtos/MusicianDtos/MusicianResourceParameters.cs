using Discography.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discography.Data.Dtos.MusicianDtos
{
    public class MusicianResourceParameters: ResourceParameters
    {
        public string Instrument { get; set; }
    }
}
