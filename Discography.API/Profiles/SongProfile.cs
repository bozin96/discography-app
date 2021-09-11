using AutoMapper;
using Discography.Core.Enums;
using Discography.Core.Models;
using Discography.Data.Dtos.SongDtos;
using Discography.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Discography.API.Profiles
{
    public class SongProfile : Profile
    {
        public SongProfile()
        {
            CreateMap<Song, SongDto>()
                .ForMember(
                    dest => dest.Duration,
                    opt => opt.MapFrom(src => TimeSpan.FromSeconds(src.DurationInSeconds).ToString(@"mm\:ss")))
                .ForMember(
                    dest => dest.Genres,
                    opt => opt.MapFrom(src => src.Genres.GetFlagsAsListOfStrings()));

            CreateMap<SongForCreationDto, Song>()
                .ForMember(
                    dest => dest.DurationInSeconds,
                    opt => opt.MapFrom(src => (int)TimeSpan.ParseExact(src.Duration, @"mm\:ss", CultureInfo.InvariantCulture).TotalSeconds))
                .ForMember(
                    dest => dest.Genres,
                    opt => opt.MapFrom(src => EnumExtensions.GetFlagsValueFromStringList<Genres>(src.Genres)));

            CreateMap<SongForUpdateDto, Song>()
                .ForMember(
                    dest => dest.DurationInSeconds,
                    opt => opt.MapFrom(src => (int)TimeSpan.ParseExact(src.Duration, @"mm\:ss", CultureInfo.InvariantCulture).TotalSeconds))
                .ForMember(
                    dest => dest.Genres,
                    opt => opt.MapFrom(src => EnumExtensions.GetFlagsValueFromStringList<Genres>(src.Genres)));

            CreateMap<Song, SongForUpdateDto>()
                .ForMember(
                    dest => dest.Duration,
                    opt => opt.MapFrom(src => TimeSpan.FromSeconds(src.DurationInSeconds).ToString(@"mm\:ss")))
                .ForMember(
                    dest => dest.Genres,
                    opt => opt.MapFrom(src => src.Genres.GetFlagsAsListOfStrings()));
        }
    }
}
