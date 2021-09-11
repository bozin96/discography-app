using Discography.Data.Dtos.SongDtos;
using Discography.Data.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;

namespace Discography.Data.ValidationAttributes
{
    public class SongDateRecorderMustBeLessThanDateReleasedAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value,
            ValidationContext validationContext)
        {
            //if (!validationContext.ObjectInstance.IsList())
            //{
            var song = (SongForManipulationDto)validationContext.ObjectInstance;

            if (song.DateRecorded.HasValue && song.DateReleased.HasValue)
            {
                if (song.DateRecorded.Value > song.DateReleased.Value)
                    return new ValidationResult(ErrorMessage,
                        new[] { nameof(SongForManipulationDto) });
            }
            //}
            //else
            //{
            //    foreach (var objectInstance in (List<ExpandoObject>)validationContext.ObjectInstance)
            //    {
            //        var song = (SongForManipulationDto)validationContext.ObjectInstance;

            //        if (song.DateRecorded.HasValue && song.DateReleased.HasValue)
            //        {
            //            if (song.DateRecorded.Value > song.DateReleased.Value)
            //                return new ValidationResult(ErrorMessage,
            //                    new[] { nameof(SongForManipulationDto) });
            //        }
            //    }
            //}
            return ValidationResult.Success;
        }
    }
}
