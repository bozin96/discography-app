using Discography.Core.Enums;
using Discography.Core.Models;
using Discography.Data.Context;
using Discography.Data.Dtos.SongDtos;
using Discography.Data.Extensions;
using Discography.Data.Helpers;
using Discography.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discography.Data.Repositories
{
    public class SongRepository : GenericRepository<Song>, ISongRepository
    {
        private readonly IPropertyMappingService _propertyMappingService;

        public SongRepository(ApplicationDbContext context, IPropertyMappingService propertyMappingService) : base(context)
        {
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
        }

        public async Task<bool> ExistsAsync(Guid bandId, Guid songId)
        {
            if (bandId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(bandId));
            }
            if (songId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(songId));
            }

            return await _entities.AnyAsync(e => e.Id == songId && e.BandId == bandId);
        }

        public async Task<bool> ExistsAsync(Guid bandId, Guid albumId, Guid songId)
        {
            if (bandId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(bandId));
            }
            if (albumId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(albumId));
            }
            if (songId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(songId));
            }

            return await _entities.AnyAsync(e => e.Id == songId && e.BandId == bandId && e.AlbumId == albumId);
        }

        public async Task<Song> GetByIdAsync(Guid bandId, Guid songId)
        {
            if (bandId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(bandId));
            }
            if (songId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(songId));
            }

            return await _entities.FirstOrDefaultAsync(e => e.Id == songId && e.BandId == bandId);
        }

        public async Task<Song> GetByIdAsync(Guid bandId, Guid albumId, Guid songId)
        {
            if (bandId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(bandId));
            }
            if (albumId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(albumId));
            }
            if (songId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(songId));
            }

            return await _entities.FirstOrDefaultAsync(e => e.Id == songId && e.BandId == bandId && e.AlbumId == albumId);
        }

        public async Task<PagedList<Song>> ListAsync(Guid bandId, SongResourceParameters resourceParameters)
        {
            if (resourceParameters == null)
            {
                throw new ArgumentNullException(nameof(resourceParameters));
            }

            var collection = _entities as IQueryable<Song>;

            collection = collection.Where(e => e.BandId == bandId);

            // filter by genre
            if (!string.IsNullOrEmpty(resourceParameters.Genre))
            {
                var stringGenre = resourceParameters.Genre.Trim();
                Genres genre = EnumExtensions.GetValueFromName<Genres>(stringGenre);
                if (genre != Genres.Empty)
                {
                    collection = collection.Where(e => e.Genres.HasFlag(genre));
                }
            }

            // filter by from date release
            if (resourceParameters.FromDateReleased.HasValue)
            {
                collection = collection.Where(e => e.DateReleased > resourceParameters.FromDateReleased.Value);
            }

            // filter by to date release
            if (resourceParameters.ToDateReleased.HasValue)
            {
                collection = collection.Where(e => e.DateReleased < resourceParameters.ToDateReleased.Value);
            }

            if (!string.IsNullOrWhiteSpace(resourceParameters.SearchQuery))
            {
                var searchQuery = resourceParameters.SearchQuery.Trim();
                collection = collection.Where(e => e.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(resourceParameters.OrderBy))
            {
                // get property mapping dictionary
                var authorPropertyMappingDictionary =
                    _propertyMappingService.GetPropertyMapping<SongDto, Song>();

                collection = collection.ApplySort(resourceParameters.OrderBy, authorPropertyMappingDictionary);
            }

            return await PagedList<Song>.CreateAsync(collection,
                resourceParameters.PageNumber,
                resourceParameters.PageSize);
        }

        public async Task<PagedList<Song>> ListAsync(Guid bandId, Guid albumId, SongResourceParameters resourceParameters)
        {
            if (resourceParameters == null)
            {
                throw new ArgumentNullException(nameof(resourceParameters));
            }

            var collection = _entities as IQueryable<Song>;

            collection = collection.Where(e => e.BandId == bandId && e.AlbumId == albumId);

            // filter by genre
            if (!string.IsNullOrEmpty(resourceParameters.Genre))
            {
                var stringGenre = resourceParameters.Genre.Trim();
                Genres genre = EnumExtensions.GetValueFromName<Genres>(stringGenre);
                if (genre != Genres.Empty)
                {
                    collection = collection.Where(e => e.Genres.HasFlag(genre));
                }
            }

            // filter by from date release
            if (resourceParameters.FromDateReleased.HasValue)
            {
                collection = collection.Where(e => e.DateReleased > resourceParameters.FromDateReleased.Value);
            }

            // filter by to date release
            if (resourceParameters.ToDateReleased.HasValue)
            {
                collection = collection.Where(e => e.DateReleased < resourceParameters.ToDateReleased.Value);
            }

            if (!string.IsNullOrWhiteSpace(resourceParameters.SearchQuery))
            {
                var searchQuery = resourceParameters.SearchQuery.Trim();
                collection = collection.Where(e => e.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(resourceParameters.OrderBy))
            {
                // get property mapping dictionary
                var authorPropertyMappingDictionary =
                    _propertyMappingService.GetPropertyMapping<SongDto, Song>();

                collection = collection.ApplySort(resourceParameters.OrderBy, authorPropertyMappingDictionary);
            }

            return await PagedList<Song>.CreateAsync(collection,
                resourceParameters.PageNumber,
                resourceParameters.PageSize);
        }
    }
}
