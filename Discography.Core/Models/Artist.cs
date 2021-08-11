using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Discography.Core.Models
{
    public abstract class Artist : BaseEntity
    {
        public string AlsoKnownAs { get; set; }

        public string BiographyDescription { get; set; }
    }
}
