using AutoMapper;
using Discography.Core.Enums;
using Discography.Core.Models;
using Discography.Data.Dtos.MusicianDtos;
using Discography.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discography.API.Profiles
{
    public class MusiciansProfile : Profile
    {
        public MusiciansProfile()
        {
            CreateMap<Musician, MusicianDto>()
                    .ForMember(
                        dest => dest.Instruments,
                        opt => opt.MapFrom(src => src.Instruments.GetFlagsAsListOfStrings()))
                    .ForMember(
                        dest => dest.Name,
                        opt => opt.MapFrom(src => @$"{src.FirstName} {(string.IsNullOrEmpty(src.MiddleName) ? "" : (src.MiddleName + " "))}{src.LastName}"));

            CreateMap<MusicianForCreationDto, Musician>()
                    .ForMember(
                        dest => dest.Instruments,
                        opt => opt.MapFrom(src => EnumExtensions.GetFlagsValueFromStringList<Instruments>(src.Instruments)));

            CreateMap<MusicianForUpdateDto, Musician>()
                    .ForMember(
                        dest => dest.Instruments,
                        opt => opt.MapFrom(src => EnumExtensions.GetFlagsValueFromStringList<Instruments>(src.Instruments)));

            CreateMap<Musician, MusicianForUpdateDto>()
                    .ForMember(
                        dest => dest.Instruments,
                        opt => opt.MapFrom(src => src.Instruments.GetFlagsAsListOfStrings()));
        }
    }
}
