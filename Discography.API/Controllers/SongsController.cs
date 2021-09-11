using AutoMapper;
using Discography.API.ActionConstraints;
using Discography.API.Dtos;
using Discography.API.Enums;
using Discography.Core.Models;
using Discography.Data.Dtos.SongDtos;
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
    [Route("api/bands/{bandId}/albums/{albumId}/songs")]
    [ApiController]
    public class SongsController : ControllerBase
    {
        private readonly ISongRepository _songRepository;
        private readonly IAlbumRepository _albumRepository;
        private readonly IMusicianRepository _musicianRepository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;

        public SongsController(
            ISongRepository songRepository,
            IAlbumRepository albumRepository,
            IMusicianRepository musicianRepository,
            IMapper mapper,
            IPropertyMappingService propertyMappingService,
            IPropertyCheckerService propertyCheckerService)
        {
            _songRepository = songRepository ?? throw new ArgumentNullException(nameof(songRepository));
            _albumRepository = albumRepository ?? throw new ArgumentNullException(nameof(albumRepository));
            _musicianRepository = musicianRepository ?? throw new ArgumentNullException(nameof(musicianRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
        }

        [HttpGet(Name = "GetSongsForAlbum")]
        [HttpHead]
        public async Task<ActionResult<IEnumerable<SongDto>>> GetSongsForAlbumAsync(Guid bandId, Guid albumId, [FromQuery] SongResourceParameters resourceParameters)
        {
            if (!await _albumRepository.ExistsAsync(bandId, albumId))
            {
                return NotFound();
            }

            if (!_propertyMappingService.ValidMappingExistsFor<SongDto, Song>(resourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<SongDto>(resourceParameters.Fields))
            {
                return BadRequest();
            }

            var songsForBandFromRepo = await _songRepository.ListAsync(bandId, albumId, resourceParameters);

            var paginationMetadata = new
            {
                totalCount = songsForBandFromRepo.TotalCount,
                pageSize = songsForBandFromRepo.PageSize,
                currentPage = songsForBandFromRepo.CurrentPage,
                totalPages = songsForBandFromRepo.TotalPages
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            var links = CreateLinksForSongs(resourceParameters, songsForBandFromRepo.HasNext, songsForBandFromRepo.HasPrevious);

            var shapedSongs = _mapper.Map<IEnumerable<SongDto>>(songsForBandFromRepo).ShapeData(resourceParameters.Fields);

            var shapedSongsWithLinks = shapedSongs.Select(band =>
            {
                var bandAsDictionary = band as IDictionary<string, object>;
                //var bandLinks = CreateLinksForSong((Guid)bandAsDictionary["Id"], null);
                // bandAsDictionary.Add("links", bandLinks);
                return bandAsDictionary;
            });

            var linkedCollectionResource = new
            {
                value = shapedSongsWithLinks,
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
        [HttpGet("{songId}", Name = "GetSongForAlbum")]
        public async Task<ActionResult<SongDto>> GetSongForAlbumAsync(Guid bandId, Guid albumId, Guid songId, string fields,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType,
                out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<SongDto>(fields))
            {
                return BadRequest();
            }

            var songFromRepo = await _songRepository.GetByIdAsync(bandId, albumId, songId);

            if (songFromRepo == null)
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
            var friendlyResourceToReturn = _mapper.Map<SongDto>(songFromRepo)
                .ShapeData(fields) as IDictionary<string, object>;

            if (includeLinks)
            {
                var links = CreateLinksForSong(bandId, albumId, songId, fields);
                friendlyResourceToReturn.Add("links", links);
            }

            return Ok(friendlyResourceToReturn);
        }

        [HttpPost(Name = "CreateSongForBand")]
        [RequestHeaderMatchesMediaType("Content-Type",
            "application/json",
            "application/vnd.marvin.bandforcreation+json")]
        [Consumes("application/json",
            "application/vnd.marvin.bandforcreation+json")]
        public async Task<ActionResult<SongDto>> CreateSongForBandAsync(Guid bandId, Guid albumId,
            [FromBody] SongForCreationDto song)
        {
            if (!ModelState.IsValid)
            { 
                return BadRequest();
            }

            if (song.LyricistId.HasValue && !await _musicianRepository.ExistsAsync(song.LyricistId.Value))
            {
                return BadRequest();
            }
            if (song.ComposerId.HasValue && !await _musicianRepository.ExistsAsync(song.ComposerId.Value))
            {
                return BadRequest();
            }

            var songEntity = _mapper.Map<Song>(song);
            songEntity.BandId = bandId;
            songEntity.AlbumId = albumId;


            await _songRepository.AddAsync(songEntity);

            var songToReturn = _mapper.Map<SongDto>(songEntity);

            var links = CreateLinksForSong(bandId, albumId, songToReturn.Id, null);

            var linkedResourceToReturn = songToReturn.ShapeData(null)
                as IDictionary<string, object>;
            linkedResourceToReturn.Add("links", links);

            return CreatedAtRoute("GetSongForAlbum",
                new { bandId, albumId, songId = linkedResourceToReturn["Id"] },
                linkedResourceToReturn);
        }

        [HttpPut("{songId}", Name = "UpdateSongForAlbum")]
        public async Task<IActionResult> UpdateSongAsync(Guid bandId, Guid albumId, Guid songId, SongForUpdateDto song)
        {
            if (song.LyricistId.HasValue && !await _musicianRepository.ExistsAsync(song.LyricistId.Value))
            {
                return BadRequest();
            }
            if (song.ComposerId.HasValue && !await _musicianRepository.ExistsAsync(song.ComposerId.Value))
            {
                return BadRequest();
            }

            var songFromRepo = await _songRepository.GetByIdAsync(bandId, albumId, songId);

            if (songFromRepo == null)
            {
                var songToAdd = _mapper.Map<Song>(song);
                songToAdd.Id = songId;

                await _songRepository.AddAsync(songToAdd);


                var songToReturn = _mapper.Map<SongDto>(songToAdd);

                return CreatedAtRoute("GetSongForAlbum",
                    new { bandId, songId = songToReturn.Id },
                    songToReturn);
            }

            _mapper.Map(song, songFromRepo);

            await _songRepository.UpdateAsync(songFromRepo);

            return NoContent();
        }

        [HttpPatch("{songId}", Name = "PartiallyUpdateSongForAlbum")]
        public async Task<ActionResult> PartiallyUpdateSongAsync(Guid bandId, Guid songId, JsonPatchDocument<SongForUpdateDto> patchDocument)
        {
            var songFromRepo = await _songRepository.GetByIdAsync(bandId, songId);

            if (songFromRepo == null)
            {
                var songDto = new SongForUpdateDto();
                patchDocument.ApplyTo(songDto, ModelState);

                if (!TryValidateModel(songDto))
                {
                    return ValidationProblem(ModelState);
                }

                var songToAdd = _mapper.Map<Song>(songDto);
                songToAdd.Id = bandId;

                await _songRepository.AddAsync(songToAdd);

                var bandToReturn = _mapper.Map<SongDto>(songToAdd);

                return CreatedAtRoute("GetSongForAlbum",
                    new { bandId, songId = bandToReturn.Id },
                    bandToReturn);
            }

            var songToPatch = _mapper.Map<SongForUpdateDto>(songFromRepo);
            // add validation
            patchDocument.ApplyTo(songToPatch, ModelState);

            if (!TryValidateModel(songToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(songToPatch, songFromRepo);

            await _songRepository.UpdateAsync(songFromRepo);

            return NoContent();
        }

        [HttpDelete("{songId}", Name = "DeleteSongForAlbum")]
        public async Task<ActionResult> DeleteSongAsync(Guid bandId, Guid albumId, Guid songId)
        {
            if (!await _songRepository.ExistsAsync(bandId, albumId, songId))
            {
                return NotFound();
            }

            await _songRepository.DeleteAsync(songId);

            return NoContent();
        }

        [HttpOptions]
        public IActionResult GetSongOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST,PUT,PATCH");
            return Ok();
        }

        #region Helpers

        private string CreateSongsResourceUri(SongResourceParameters songResourceParameters, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link("GetSongsForAlbum",
                      new
                      {
                          fields = songResourceParameters.Fields,
                          orderBy = songResourceParameters.OrderBy,
                          pageNumber = songResourceParameters.PageNumber - 1,
                          pageSize = songResourceParameters.PageSize,
                          genre = songResourceParameters.Genre,
                          fromDateReleased = songResourceParameters.FromDateReleased,
                          toDateReleased = songResourceParameters.ToDateReleased,
                          searchQuery = songResourceParameters.SearchQuery
                      });
                case ResourceUriType.NextPage:
                    return Url.Link("GetSongsForAlbum",
                      new
                      {
                          fields = songResourceParameters.Fields,
                          orderBy = songResourceParameters.OrderBy,
                          pageNumber = songResourceParameters.PageNumber + 1,
                          pageSize = songResourceParameters.PageSize,
                          genre = songResourceParameters.Genre,
                          fromDateReleased = songResourceParameters.FromDateReleased,
                          toDateReleased = songResourceParameters.ToDateReleased,
                          searchQuery = songResourceParameters.SearchQuery
                      });
                case ResourceUriType.Current:
                default:
                    return Url.Link("GetSongsForAlbum",
                    new
                    {
                        fields = songResourceParameters.Fields,
                        orderBy = songResourceParameters.OrderBy,
                        pageNumber = songResourceParameters.PageNumber,
                        pageSize = songResourceParameters.PageSize,
                        genre = songResourceParameters.Genre,
                        fromDateReleased = songResourceParameters.FromDateReleased,
                        toDateReleased = songResourceParameters.ToDateReleased,
                        searchQuery = songResourceParameters.SearchQuery
                    });
            }

        }

        private IEnumerable<LinkDto> CreateLinksForSong(Guid bandId, Guid albumId, Guid songId, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(Url.Link("GetSongForAlbum", new { bandId, albumId, songId }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(Url.Link("GetSongsForAlbum", new { bandId, albumId, songId, fields }),
                  "self",
                  "GET"));
            }

            links.Add(
               new LinkDto(Url.Link("DeleteSongForAlbum", new { bandId, albumId, songId }),
               "delete_song",
               "DELETE"));

            links.Add(
                new LinkDto(Url.Link("CreateSongForBand", new { bandId, albumId }),
                "create_song",
                "POST"));

            links.Add(
                new LinkDto(Url.Link("UpdateSongForAlbum", new { bandId, albumId, songId }),
                "update_song",
                "PUT"));

            links.Add(
                new LinkDto(Url.Link("PartiallyUpdateSongForAlbum", new { bandId, albumId, songId }),
                "partially_update_song",
                "PATCH"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForSongs(SongResourceParameters songResourceParameters, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            // self 
            links.Add(
               new LinkDto(CreateSongsResourceUri(
                   songResourceParameters, ResourceUriType.Current),
               "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateSongsResourceUri(
                      songResourceParameters, ResourceUriType.NextPage),
                  "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new LinkDto(CreateSongsResourceUri(
                        songResourceParameters, ResourceUriType.PreviousPage),
                    "previousPage", "GET"));
            }

            return links;
        }

        #endregion
    }
}
