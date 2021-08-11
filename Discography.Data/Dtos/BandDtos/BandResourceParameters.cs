using Discography.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discography.Data.Dtos
{
    public class BandResourceParameters : PaginationRequest
    {
        public int YearOfFormation { get; set; } = 0;

        public string Genre { get; set; }
    }
}
