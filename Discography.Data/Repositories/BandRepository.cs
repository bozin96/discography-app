using Discography.Core.Enums;
using Discography.Core.Models;
using Discography.Data.Context;
using Discography.Data.Dtos;
using Discography.Data.Extensions;
using Discography.Data.Helpers;
using Discography.Data.Interfaces;
using Discography.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discography.Data.Repositories
{
    public class BandRepository : GenericRepository<Band>, IBandRepository
    {
        private readonly IPropertyMappingService _propertyMappingService;

        public BandRepository(ApplicationDbContext context, IPropertyMappingService propertyMappingService) : base(context)
        {
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
        }

        public async Task<PagedList<Band>> ListAsync(BandResourceParameters resourceParameters)
        {
            if (resourceParameters == null)
            {
                throw new ArgumentNullException(nameof(resourceParameters));
            }

            var collection = _entities as IQueryable<Band>;

            // filter by year of formation
            if (resourceParameters.YearOfFormation != 0)
            {
                collection = collection.Where(b => b.YearOfFormation == resourceParameters.YearOfFormation);
            }

            if (!string.IsNullOrEmpty(resourceParameters.Genre))
            {
                var stringGenre = resourceParameters.Genre.Trim();
                Genres genre = EnumExtensions.GetValueFromName<Genres>(stringGenre);
                if (genre != Genres.Empty)
                {
                    collection = collection.Where(b => b.Genres.HasFlag(genre));
                }
            }

            if (!string.IsNullOrWhiteSpace(resourceParameters.SearchQuery))
            {

                var searchQuery = resourceParameters.SearchQuery.Trim();
                collection = collection.Where(b => b.Name.Contains(searchQuery) ||
                                                   b.AlsoKnownAs.Contains(searchQuery));
            }

            if (!string.IsNullOrWhiteSpace(resourceParameters.OrderBy))
            {
                // get property mapping dictionary
                var authorPropertyMappingDictionary =
                    _propertyMappingService.GetPropertyMapping<BandDto, Band>();

                collection = collection.ApplySort(resourceParameters.OrderBy, authorPropertyMappingDictionary);
            }

            return await PagedList<Band>.CreateAsync(collection,
                resourceParameters.PageNumber,
                resourceParameters.PageSize);
        }
    }
}
