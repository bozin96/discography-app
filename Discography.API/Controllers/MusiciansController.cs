using AutoMapper;
using Discography.API.ActionConstraints;
using Discography.API.Dtos;
using Discography.API.Enums;
using Discography.Core.Models;
using Discography.Data.Dtos.MusicianDtos;
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
    [Route("api/bands/{bandId}/musicians")]
    public class MusiciansController : ControllerBase
    {
        private readonly IMusicianRepository _musicianRepository;
        private readonly IBandRepository _bandRepository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;

        public MusiciansController(
            IMusicianRepository musicianRepository,
            IBandRepository bandRepository,
            IMapper mapper,
            IPropertyMappingService propertyMappingService,
            IPropertyCheckerService propertyCheckerService)
        {
            _musicianRepository = musicianRepository ?? throw new ArgumentNullException(nameof(musicianRepository));
            _bandRepository = bandRepository ?? throw new ArgumentNullException(nameof(bandRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
        }

        [HttpGet(Name = "GetMusiciansForBand")]
        [HttpHead]
        public async Task<ActionResult<IEnumerable<MusicianDto>>> GetMusiciansForBandAsync(Guid bandId, [FromQuery] MusicianResourceParameters resourceParameters)
        {
            if (!await _bandRepository.ExistsAsync(bandId))
            {
                return NotFound();
            }

            if (!_propertyMappingService.ValidMappingExistsFor<MusicianDto, Musician>(resourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<MusicianDto>(resourceParameters.Fields))
            {
                return BadRequest();
            }

            var musiciansForBandFromRepo = await _musicianRepository.ListAsync(bandId, resourceParameters);

            var paginationMetadata = new
            {
                totalCount = musiciansForBandFromRepo.TotalCount,
                pageSize = musiciansForBandFromRepo.PageSize,
                currentPage = musiciansForBandFromRepo.CurrentPage,
                totalPages = musiciansForBandFromRepo.TotalPages
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            var links = CreateLinksForMusicians(resourceParameters, musiciansForBandFromRepo.HasNext, musiciansForBandFromRepo.HasPrevious);

            var shapedMusicians = _mapper.Map<IEnumerable<MusicianDto>>(musiciansForBandFromRepo).ShapeData(resourceParameters.Fields);

            var shapedMusiciansWithLinks = shapedMusicians.Select(band =>
            {
                var bandAsDictionary = band as IDictionary<string, object>;
                //var bandLinks = CreateLinksForMusician((Guid)bandAsDictionary["Id"], null);
                // bandAsDictionary.Add("links", bandLinks);
                return bandAsDictionary;
            });

            var linkedCollectionResource = new
            {
                value = shapedMusiciansWithLinks,
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
        [HttpGet("{musicianId}", Name = "GetMusicianForBand")]
        public async Task<ActionResult<MusicianDto>> GetMusicianForBandAsync(Guid bandId, Guid musicianId, string fields,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType,
                out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<MusicianDto>(fields))
            {
                return BadRequest();
            }

            var musicianFromRepo = await _musicianRepository.GetByIdAsync(bandId, musicianId);

            if (musicianFromRepo == null)
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
            var friendlyResourceToReturn = _mapper.Map<MusicianDto>(musicianFromRepo)
                .ShapeData(fields) as IDictionary<string, object>;

            if (includeLinks)
            {
                var links = CreateLinksForMusician(bandId, musicianId, fields);
                friendlyResourceToReturn.Add("links", links);
            }

            return Ok(friendlyResourceToReturn);
        }

        [HttpPost(Name = "CreateMusicianForBand")]
        [RequestHeaderMatchesMediaType("Content-Type",
            "application/json",
            "application/vnd.marvin.bandforcreation+json")]
        [Consumes("application/json",
            "application/vnd.marvin.bandforcreation+json")]
        public async Task<ActionResult<MusicianDto>> CreateMusicianForBandAsync(Guid bandId,
            [FromBody] MusicianForCreationDto musician)
        {
            var musicianEntity = _mapper.Map<Musician>(musician);
            musicianEntity.BandId = bandId;
            await _musicianRepository.AddAsync(musicianEntity);

            var musicianToReturn = _mapper.Map<MusicianDto>(musicianEntity);

            var links = CreateLinksForMusician(bandId, musicianToReturn.Id, null);

            var linkedResourceToReturn = musicianToReturn.ShapeData(null)
                as IDictionary<string, object>;
            linkedResourceToReturn.Add("links", links);

            return CreatedAtRoute("GetMusicianForBand",
                new { bandId, musicianId = linkedResourceToReturn["Id"] },
                linkedResourceToReturn);
        }

        [HttpPut("{musicianId}", Name = "UpdateMusicianForBand")]
        public async Task<IActionResult> UpdateMusicianAsync(Guid bandId, Guid musicianId, MusicianForUpdateDto musician)
        {
            var musicianFromRepo = await _musicianRepository.GetByIdAsync(bandId, musicianId);

            if (musicianFromRepo == null)
            {
                var musicianToAdd = _mapper.Map<Musician>(musician);
                musicianToAdd.Id = musicianId;

                await _musicianRepository.AddAsync(musicianToAdd);


                var musicianToReturn = _mapper.Map<MusicianDto>(musicianToAdd);

                return CreatedAtRoute("GetMusicianForBand",
                    new { bandId, musicianId = musicianToReturn.Id },
                    musicianToReturn);
            }

            _mapper.Map(musician, musicianFromRepo);

            await _musicianRepository.UpdateAsync(musicianFromRepo);

            return NoContent();
        }

        [HttpPatch("{musicianId}", Name = "PartiallyUpdateMusicianForBand")]
        public async Task<ActionResult> PartiallyUpdateMusicianAsync(Guid bandId, Guid musicianId, JsonPatchDocument<MusicianForUpdateDto> patchDocument)
        {
            var musicianFromRepo = await _musicianRepository.GetByIdAsync(bandId, musicianId);

            if (musicianFromRepo == null)
            {
                var musicianDto = new MusicianForUpdateDto();
                patchDocument.ApplyTo(musicianDto, ModelState);

                if (!TryValidateModel(musicianDto))
                {
                    return ValidationProblem(ModelState);
                }

                var musicianToAdd = _mapper.Map<Musician>(musicianDto);
                musicianToAdd.Id = bandId;

                await _musicianRepository.AddAsync(musicianToAdd);

                var bandToReturn = _mapper.Map<MusicianDto>(musicianToAdd);

                return CreatedAtRoute("GetMusicianForBand",
                    new { bandId, musicianId = bandToReturn.Id },
                    bandToReturn);
            }

            var musicianToPatch = _mapper.Map<MusicianForUpdateDto>(musicianFromRepo);
            // add validation
            patchDocument.ApplyTo(musicianToPatch, ModelState);

            if (!TryValidateModel(musicianToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(musicianToPatch, musicianFromRepo);

            await _musicianRepository.UpdateAsync(musicianFromRepo);

            return NoContent();
        }

        [HttpDelete("{musicianId}", Name = "DeleteMusicianForBand")]
        public async Task<ActionResult> DeleteMusicianAsync(Guid bandId, Guid musicianId)
        {
            if (!await _musicianRepository.ExistsAsync(bandId, musicianId))
            {
                return NotFound();
            }

            await _musicianRepository.DeleteAsync(musicianId);

            return NoContent();
        }

        [HttpOptions]
        public IActionResult GetMusicianOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST,PUT,PATCH");
            return Ok();
        }


        #region Helpers

        private string CreateBandsResourceUri(MusicianResourceParameters musicianResourceParameters, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link("GetMusiciansForBand",
                      new
                      {
                          fields = musicianResourceParameters.Fields,
                          orderBy = musicianResourceParameters.OrderBy,
                          pageNumber = musicianResourceParameters.PageNumber - 1,
                          pageSize = musicianResourceParameters.PageSize,
                          instrument = musicianResourceParameters.Instrument,
                          searchQuery = musicianResourceParameters.SearchQuery
                      });
                case ResourceUriType.NextPage:
                    return Url.Link("GetMusiciansForBand",
                      new
                      {
                          fields = musicianResourceParameters.Fields,
                          orderBy = musicianResourceParameters.OrderBy,
                          pageNumber = musicianResourceParameters.PageNumber + 1,
                          pageSize = musicianResourceParameters.PageSize,
                          instrument = musicianResourceParameters.Instrument,
                          searchQuery = musicianResourceParameters.SearchQuery
                      });
                case ResourceUriType.Current:
                default:
                    return Url.Link("GetMusiciansForBand",
                    new
                    {
                        fields = musicianResourceParameters.Fields,
                        orderBy = musicianResourceParameters.OrderBy,
                        pageNumber = musicianResourceParameters.PageNumber,
                        pageSize = musicianResourceParameters.PageSize,
                        instrument = musicianResourceParameters.Instrument,
                        searchQuery = musicianResourceParameters.SearchQuery
                    });
            }

        }

        private IEnumerable<LinkDto> CreateLinksForMusician(Guid bandId, Guid musicianId, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(Url.Link("GetMusicianForBand", new { bandId, musicianId }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(Url.Link("GetMusicianForBand", new { bandId, musicianId, fields }),
                  "self",
                  "GET"));
            }

            links.Add(
               new LinkDto(Url.Link("DeleteMusicianForBand", new { bandId, musicianId }),
               "delete_musician",
               "DELETE"));

            links.Add(
                new LinkDto(Url.Link("CreateMusicianForBand", new { bandId }),
                "create_musician",
                "POST"));

            links.Add(
                new LinkDto(Url.Link("UpdateMusicianForBand", new { bandId, musicianId }),
                "update_musician",
                "PUT"));

            links.Add(
                new LinkDto(Url.Link("PartiallyUpdateMusicianForBand", new { bandId, musicianId }),
                "partially_update_musician",
                "PATCH"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForMusicians(MusicianResourceParameters musicianResourceParameters, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            // self 
            links.Add(
               new LinkDto(CreateBandsResourceUri(
                   musicianResourceParameters, ResourceUriType.Current),
               "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateBandsResourceUri(
                      musicianResourceParameters, ResourceUriType.NextPage),
                  "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new LinkDto(CreateBandsResourceUri(
                        musicianResourceParameters, ResourceUriType.PreviousPage),
                    "previousPage", "GET"));
            }

            return links;
        }

        #endregion
    }
}
