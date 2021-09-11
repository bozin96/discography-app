using Discography.Core.Models;
using Discography.Data.Dtos.SongDtos;
using Discography.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Discography.Data.Interfaces
{
    public interface ISongRepository : IGenericRepository<Song>
    {
        Task<bool> ExistsAsync(Guid bandId, Guid songId);

        Task<bool> ExistsAsync(Guid bandId, Guid albumId, Guid songId);

        Task<Song> GetByIdAsync(Guid bandId, Guid songId);

        Task<Song> GetByIdAsync(Guid bandId, Guid albumId, Guid songId);

        Task<PagedList<Song>> ListAsync(Guid bandId, SongResourceParameters resourceParameters);

        Task<PagedList<Song>> ListAsync(Guid bandId, Guid albumId, SongResourceParameters resourceParameters);

    }
}
