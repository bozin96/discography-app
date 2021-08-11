using System;
using System.ComponentModel.DataAnnotations;

namespace Discography.Core.Models
{
    public abstract class BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime? CreationDate { get; }
    }
}