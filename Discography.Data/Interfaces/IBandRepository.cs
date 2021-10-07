using Discography.Core.Models;
using Discography.Data.Dtos.BandDtos;
using Discography.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discography.Data.Interfaces
{
    public interface IBandRepository : IGenericRepository<Band>
    {
        Task<PagedList<Band>> ListAsync(BandResourceParameters resourceParameters);

        Task<IEnumerable<Band>> GetBandsAsync(IEnumerable<Guid> bandIds);
    }
}
