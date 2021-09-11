using AutoMapper;
using Discography.Core.Models;
using Discography.Data.Dtos.AlbumDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discography.API.Profiles
{
    public class AlbumsProfile : Profile
    {
        public AlbumsProfile()
        {
            CreateMap<Album, AlbumDto>();

            CreateMap<AlbumForCreationDto, Album>();

            CreateMap<AlbumForUpdateDto, Album>();

            CreateMap<Album, AlbumForUpdateDto>();

        }
    }
}
