using AutoMapper;
using Discography.API.ActionConstraints;
using Discography.API.Dtos;
using Discography.API.Enums;
using Discography.Core.Models;
using Discography.Data.Dtos.AlbumDtos;
using Discography.Data.Extensions;
using Discography.Data.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Discography.API.Controllers
{
    [ApiController]
    [Route("api/bands/{bandId}/albums")]
    public class AlbumsController : ControllerBase
    {
        private readonly IAlbumRepository _albumRepository;
        private readonly IBandRepository _bandRepository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;

        public AlbumsController(
            IAlbumRepository albumRepository,
            IBandRepository bandRepository,
            IMapper mapper,
            IPropertyMappingService propertyMappingService,
            IPropertyCheckerService propertyCheckerService)
        {
            _albumRepository = albumRepository ?? throw new ArgumentNullException(nameof(albumRepository));
            _bandRepository = bandRepository ?? throw new ArgumentNullException(nameof(bandRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
        }

        [HttpGet(Name = "GetAlbumsForBand")]
        [HttpHead]
        public async Task<ActionResult<IEnumerable<AlbumDto>>> GetAlbumsForBandAsync(Guid bandId, [FromQuery] AlbumResourceParameters resourceParameters)
        {
            if (!await _bandRepository.ExistsAsync(bandId))
            {
                return NotFound();
            }

            if (!_propertyMappingService.ValidMappingExistsFor<AlbumDto, Album>(resourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<AlbumDto>(resourceParameters.Fields))
            {
                return BadRequest();
            }

            var AlbumsForBandFromRepo = await _albumRepository.ListAsync(bandId, resourceParameters);

            var paginationMetadata = new
            {
                totalCount = AlbumsForBandFromRepo.TotalCount,
                pageSize = AlbumsForBandFromRepo.PageSize,
                currentPage = AlbumsForBandFromRepo.CurrentPage,
                totalPages = AlbumsForBandFromRepo.TotalPages
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            var links = CreateLinksForAlbums(resourceParameters, AlbumsForBandFromRepo.HasNext, AlbumsForBandFromRepo.HasPrevious);

            var shapedAlbums = _mapper.Map<IEnumerable<AlbumDto>>(AlbumsForBandFromRepo).ShapeData(resourceParameters.Fields);

            var shapedAlbumsWithLinks = shapedAlbums.Select(band =>
            {
                var bandAsDictionary = band as IDictionary<string, object>;
                //var bandLinks = CreateLinksForAlbum((Guid)bandAsDictionary["Id"], null);
                // bandAsDictionary.Add("links", bandLinks);
                return bandAsDictionary;
            });

            var linkedCollectionResource = new
            {
                value = shapedAlbumsWithLinks,
                links
            };

            return Ok(linkedCollectionResource);
        }

        [Produces("application/json",
            "application/vnd.marvin.hateoas+json",
            "application/vnd.marvin.band.full+json",
            "application/vnd.marvin.band.full.hateoas+json",
            "application/vnd.marvin.band.friendly+json",
            "application/vnd.marvin.band.friendly.hateoas+json")]
        [HttpGet("{AlbumId}", Name = "GetAlbumForBand")]
        public async Task<ActionResult<AlbumDto>> GetAlbumForBandAsync(Guid bandId, Guid albumId, string fields,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType,
                out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<AlbumDto>(fields))
            {
                return BadRequest();
            }

            var AlbumFromRepo = await _albumRepository.GetByIdAsync(bandId, albumId);

            if (AlbumFromRepo == null)
            {
                return NotFound();
            }

            var includeLinks = parsedMediaType.SubTypeWithoutSuffix
               .EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);

            var primaryMediaType = includeLinks ?
                parsedMediaType.SubTypeWithoutSuffix
                .Substring(0, parsedMediaType.SubTypeWithoutSuffix.Length - 8)
                : parsedMediaType.SubTypeWithoutSuffix;

            // friendly band
            var friendlyResourceToReturn = _mapper.Map<AlbumDto>(AlbumFromRepo)
                .ShapeData(fields) as IDictionary<string, object>;

            if (includeLinks)
            {
                var links = CreateLinksForAlbum(bandId, albumId, fields);
                friendlyResourceToReturn.Add("links", links);
            }

            return Ok(friendlyResourceToReturn);
        }

        [HttpPost(Name = "CreateAlbumForBand")]
        [RequestHeaderMatchesMediaType("Content-Type",
            "application/json",
            "application/vnd.marvin.bandforcreation+json")]
        [Consumes("application/json",
            "application/vnd.marvin.bandforcreation+json")]
        public async Task<ActionResult<AlbumDto>> CreateAlbumForBandAsync(Guid bandId,
            [FromBody] AlbumForCreationDto album)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var albumEntity = _mapper.Map<Album>(album);
            albumEntity.BandId = bandId;
            await _albumRepository.AddAsync(albumEntity);

            var albumToReturn = _mapper.Map<AlbumDto>(albumEntity);

            var links = CreateLinksForAlbum(bandId, albumToReturn.Id, null);

            var linkedResourceToReturn = albumToReturn.ShapeData(null)
                as IDictionary<string, object>;
            linkedResourceToReturn.Add("links", links);

            return CreatedAtRoute("GetAlbumForBand",
                new { bandId, albumId = linkedResourceToReturn["Id"] },
                linkedResourceToReturn);
        }

        [HttpPut("{AlbumId}", Name = "UpdateAlbumForBand")]
        public async Task<IActionResult> UpdateAlbumAsync(Guid bandId, Guid albumId, AlbumForUpdateDto album)
        {
            var albumFromRepo = await _albumRepository.GetByIdAsync(bandId, albumId);

            if (albumFromRepo == null)
            {
                var albumToAdd = _mapper.Map<Album>(album);
                albumToAdd.Id = albumId;

                await _albumRepository.AddAsync(albumToAdd);


                var albumToReturn = _mapper.Map<AlbumDto>(albumToAdd);

                return CreatedAtRoute("GetAlbumForBand",
                    new { bandId, albumId = albumToReturn.Id },
                    albumToReturn);
            }

            _mapper.Map(album, albumFromRepo);

            await _albumRepository.UpdateAsync(albumFromRepo);

            return NoContent();
        }

        [HttpPatch("{AlbumId}", Name = "PartiallyUpdateAlbumForBand")]
        public async Task<ActionResult> PartiallyUpdateAlbumAsync(Guid bandId, Guid albumId, JsonPatchDocument<AlbumForUpdateDto> patchDocument)
        {
            var albumFromRepo = await _albumRepository.GetByIdAsync(bandId, albumId);

            if (albumFromRepo == null)
            {
                var albumDto = new AlbumForUpdateDto();
                patchDocument.ApplyTo(albumDto, ModelState);

                if (!TryValidateModel(albumDto))
                {
                    return ValidationProblem(ModelState);
                }

                var albumToAdd = _mapper.Map<Album>(albumDto);
                albumToAdd.Id = bandId;

                await _albumRepository.AddAsync(albumToAdd);

                var bandToReturn = _mapper.Map<AlbumDto>(albumToAdd);

                return CreatedAtRoute("GetAlbumForBand",
                    new { bandId, albumId = bandToReturn.Id },
                    bandToReturn);
            }

            var albumToPatch = _mapper.Map<AlbumForUpdateDto>(albumFromRepo);
            // add validation
            patchDocument.ApplyTo(albumToPatch, ModelState);

            if (!TryValidateModel(albumToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(albumToPatch, albumFromRepo);

            await _albumRepository.UpdateAsync(albumFromRepo);

            return NoContent();
        }

        [HttpDelete("{AlbumId}", Name = "DeleteAlbumForBand")]
        public async Task<ActionResult> DeleteAlbumAsync(Guid bandId, Guid albumId)
        {
            if (!await _albumRepository.ExistsAsync(bandId, albumId))
            {
                return NotFound();
            }

            await _albumRepository.DeleteAsync(albumId);

            return NoContent();
        }

        [HttpOptions]
        public IActionResult GetAlbumOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST,PUT,PATCH");
            return Ok();
        }

        #region Helpers

        private string CreateAlbumsResourceUri(AlbumResourceParameters albumResourceParameters, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link("GetAlbumsForBand",
                      new
                      {
                          fields = albumResourceParameters.Fields,
                          orderBy = albumResourceParameters.OrderBy,
                          pageNumber = albumResourceParameters.PageNumber - 1,
                          pageSize = albumResourceParameters.PageSize,
                          label = albumResourceParameters.Label,
                          searchQuery = albumResourceParameters.SearchQuery
                      });
                case ResourceUriType.NextPage:
                    return Url.Link("GetAlbumsForBand",
                      new
                      {
                          fields = albumResourceParameters.Fields,
                          orderBy = albumResourceParameters.OrderBy,
                          pageNumber = albumResourceParameters.PageNumber + 1,
                          pageSize = albumResourceParameters.PageSize,
                          label = albumResourceParameters.Label,
                          searchQuery = albumResourceParameters.SearchQuery
                      });
                case ResourceUriType.Current:
                default:
                    return Url.Link("GetAlbumsForBand",
                    new
                    {
                        fields = albumResourceParameters.Fields,
                        orderBy = albumResourceParameters.OrderBy,
                        pageNumber = albumResourceParameters.PageNumber,
                        pageSize = albumResourceParameters.PageSize,
                        label = albumResourceParameters.Label,
                        searchQuery = albumResourceParameters.SearchQuery
                    });
            }

        }

        private IEnumerable<LinkDto> CreateLinksForAlbum(Guid bandId, Guid albumId, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(Url.Link("GetAlbumForBand", new { bandId, albumId }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(Url.Link("GetAlbumForBand", new { bandId, albumId, fields }),
                  "self",
                  "GET"));
            }

            links.Add(
               new LinkDto(Url.Link("DeleteAlbumForBand", new { bandId, albumId }),
               "delete_album",
               "DELETE"));

            links.Add(
                new LinkDto(Url.Link("CreateAlbumForBand", new { bandId }),
                "create_album",
                "POST"));

            links.Add(
                new LinkDto(Url.Link("UpdateAlbumForBand", new { bandId, albumId }),
                "update_album",
                "PUT"));

            links.Add(
                new LinkDto(Url.Link("PartiallyUpdateAlbumForBand", new { bandId, albumId }),
                "partially_update_album",
                "PATCH"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForAlbums(AlbumResourceParameters AlbumResourceParameters, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            // self 
            links.Add(
               new LinkDto(CreateAlbumsResourceUri(
                   AlbumResourceParameters, ResourceUriType.Current),
               "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateAlbumsResourceUri(
                      AlbumResourceParameters, ResourceUriType.NextPage),
                  "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new LinkDto(CreateAlbumsResourceUri(
                        AlbumResourceParameters, ResourceUriType.PreviousPage),
                    "previousPage", "GET"));
            }

            return links;
        }

        #endregion
    }
}
