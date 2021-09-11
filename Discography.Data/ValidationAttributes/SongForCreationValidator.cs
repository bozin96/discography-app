using Discography.Data.Dtos.SongDtos;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Discography.Data.ValidationAttributes
{
    public class SongForCreationValidator : AbstractValidator<SongForCreationDto>
    {
        public SongForCreationValidator()
        {
            RuleFor(x => x.DateRecorded).LessThan(x => x.DateReleased);
        }
    }
}
