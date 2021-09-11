using Discography.Core.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discography.Core.Validation
{
    public class SongValidator : AbstractValidator<Song>
    {
        public SongValidator()
        {
            RuleFor(x => x.Id).NotNull();
            RuleFor(x => x.DateRecorded).LessThan(x => x.DateReleased);
        }
    }
}
