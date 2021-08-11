using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Discography.Core.Models
{
    public class ActivePeriod
    {
        public int StartYear { get; set; }


        public int? EndYear { get; set; }

        public ActivePeriod()
        {

        }

        public ActivePeriod(int start)
        {
            StartYear = start;
        }

        public ActivePeriod(int start, int end)
        {
            if (start > end)
                throw new ArgumentException();

            StartYear = start;
            EndYear = end;
        }
    }
}
