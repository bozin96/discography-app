using Discography.Core.Enums;
using Discography.Core.Models;
using Discography.Data.Context;
using Discography.Data.Dtos.MusicianDtos;
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
    public class MusicianRepository : GenericRepository<Musician>, IMusicianRepository
    {
        private readonly IPropertyMappingService _propertyMappingService;

        public MusicianRepository(ApplicationDbContext context, IPropertyMappingService propertyMappingService) : base(context)
        {
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
        }

        public async Task<bool> ExistsAsync(Guid bandId, Guid musicianId)
        {
            if (bandId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(bandId));
            }
            if (musicianId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(musicianId));
            }

            return await _entities.AnyAsync(e => e.Id == musicianId && e.BandId == bandId);
        }

        public async Task<Musician> GetByIdAsync(Guid bandId, Guid musicianId)
        {
            if (bandId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(bandId));
            }
            if (musicianId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(musicianId));
            }

            return await _entities.FirstOrDefaultAsync(e => e.Id == musicianId && e.BandId == bandId);
        }

        public async Task<PagedList<Musician>> ListAsync(Guid bandId, MusicianResourceParameters resourceParameters)
        {
            if (resourceParameters == null)
            {
                throw new ArgumentNullException(nameof(resourceParameters));
            }

            var collection = _entities as IQueryable<Musician>;

            collection = collection.Where(m => m.BandId == bandId);

            // filter by instrument
            if (!string.IsNullOrEmpty(resourceParameters.Instrument))
            {
                var stringInstrument = resourceParameters.Instrument.Trim();
                Instruments instrument = EnumExtensions.GetValueFromName<Instruments>(stringInstrument);
                if (instrument != Instruments.Empty)
                {
                    collection = collection.Where(m => m.Instruments.HasFlag(instrument));
                }
            }

            if (!string.IsNullOrWhiteSpace(resourceParameters.SearchQuery))
            {

                var searchQuery = resourceParameters.SearchQuery.Trim();
                collection = collection.Where(m => m.FirstName.Contains(searchQuery) ||
                                                   m.LastName.Contains(searchQuery) ||
                                                   m.MiddleName.Contains(searchQuery) ||
                                                   m.AlsoKnownAs.Contains(searchQuery));
            }

            if (!string.IsNullOrWhiteSpace(resourceParameters.OrderBy))
            {
                // get property mapping dictionary
                var authorPropertyMappingDictionary =
                    _propertyMappingService.GetPropertyMapping<MusicianDto, Musician>();

                collection = collection.ApplySort(resourceParameters.OrderBy, authorPropertyMappingDictionary);
            }

            return await PagedList<Musician>.CreateAsync(collection,
                resourceParameters.PageNumber,
                resourceParameters.PageSize);
        }
    }
}
