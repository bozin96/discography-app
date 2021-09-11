using Discography.Core.Models;
using Discography.Data.Context;
using Discography.Data.Dtos.AlbumDtos;
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
    public class AlbumRepository : GenericRepository<Album>, IAlbumRepository
    {
        private readonly IPropertyMappingService _propertyMappingService;

        public AlbumRepository(ApplicationDbContext context, IPropertyMappingService propertyMappingService) : base(context)
        {
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
        }

        public async Task<bool> ExistsAsync(Guid bandId, Guid albumId)
        {
            if (bandId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(bandId));
            }
            if (albumId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(albumId));
            }

            return await _entities.AnyAsync(e => e.Id == albumId && e.BandId == bandId);
        }

        public async Task<Album> GetByIdAsync(Guid bandId, Guid albumId)
        {
            if (bandId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(bandId));
            }
            if (albumId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(albumId));
            }

            return await _entities.FirstOrDefaultAsync(e => e.Id == albumId && e.BandId == bandId);
        }

        public async Task<PagedList<Album>> ListAsync(Guid bandId, AlbumResourceParameters resourceParameters)
        {
            if (resourceParameters == null)
            {
                throw new ArgumentNullException(nameof(resourceParameters));
            }

            var collection = _entities as IQueryable<Album>;

            collection = collection.Where(a => a.BandId == bandId);

            // filter by label
            if (!string.IsNullOrEmpty(resourceParameters.Label))
            {
                var label = resourceParameters.Label.Trim();
                collection = collection.Where(a => a.Label == label);
            }

            if (!string.IsNullOrWhiteSpace(resourceParameters.SearchQuery))
            {

                var searchQuery = resourceParameters.SearchQuery.Trim();
                collection = collection.Where(a => a.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                                                   a.Label.Contains(searchQuery, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(resourceParameters.OrderBy))
            {
                // get property mapping dictionary
                var authorPropertyMappingDictionary =
                    _propertyMappingService.GetPropertyMapping<AlbumDto, Album>();

                collection = collection.ApplySort(resourceParameters.OrderBy, authorPropertyMappingDictionary);
            }

            return await PagedList<Album>.CreateAsync(collection,
                resourceParameters.PageNumber,
                resourceParameters.PageSize);
        }
    }

}
