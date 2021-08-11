using Discography.Core.Models;
using Discography.Data.Dtos;
using Discography.Data.Helpers;
using Discography.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Discography.Data.Interfaces
{
    public interface IBandRepository : IGenericRepository<Band>
    {
        Task<PagedList<Band>> ListAsync(BandResourceParameters resourceParameters);
    }
}
