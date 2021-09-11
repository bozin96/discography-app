using Discography.Core.Models;
using Discography.Data.Dtos.MusicianDtos;
using Discography.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Discography.Data.Interfaces
{
    public interface IMusicianRepository : IGenericRepository<Musician>
    {
        Task<bool> ExistsAsync(Guid bandId, Guid musicianId);
        
        Task<Musician> GetByIdAsync(Guid bandId, Guid musicianId);

        Task<PagedList<Musician>> ListAsync(Guid bandId, MusicianResourceParameters resourceParameters);
    }
}
