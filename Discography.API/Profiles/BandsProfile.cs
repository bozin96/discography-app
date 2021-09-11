using AutoMapper;
using Discography.Core.Enums;
using Discography.Core.Models;
using Discography.Data.Dtos.BandDtos;
using Discography.Data.Extensions;
using System.Linq;

namespace Discography.API.Profiles
{
    public class BandsProfile : Profile
    {
        public BandsProfile()
        {
            CreateMap<Band, BandDto>()
                .ForMember(
                    dest => dest.ActivePeriods,
                    opt => opt.MapFrom(src => src.ActivePeriods.OrderBy(ap => ap.StartYear).Select(ap => $"{ap.StartYear}-{ap.EndYear}")))
                .ForMember(
                    dest => dest.Genres,
                    opt => opt.MapFrom(src => src.Genres.GetFlagsAsListOfStrings()));

            CreateMap<BandForCreationDto, Band>()
                .ForMember(
                    dest => dest.Genres,
                    opt => opt.MapFrom(src => EnumExtensions.GetFlagsValueFromStringList<Genres>(src.Genres)));

            CreateMap<BandForUpdateDto, Band>()
                .ForMember(
                    dest => dest.Genres,
                    opt => opt.MapFrom(src => EnumExtensions.GetFlagsValueFromStringList<Genres>(src.Genres)));

            CreateMap<Band, BandForUpdateDto>()
                .ForMember(
                    dest => dest.Genres,
                    opt => opt.MapFrom(src => src.Genres.GetFlagsAsListOfStrings()));

        }


    }
}
