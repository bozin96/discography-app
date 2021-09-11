using Discography.Core.Models;
using Discography.Data.Dtos.AlbumDtos;
using Discography.Data.Helpers;
using System;
using System.Threading.Tasks;

namespace Discography.Data.Interfaces
{
    public interface IAlbumRepository : IGenericRepository<Album>
    {
        Task<bool> ExistsAsync(Guid bandId, Guid albumId);

        Task<Album> GetByIdAsync(Guid bandId, Guid albumId);

        Task<PagedList<Album>> ListAsync(Guid bandId, AlbumResourceParameters resourceParameters);
    }
}
