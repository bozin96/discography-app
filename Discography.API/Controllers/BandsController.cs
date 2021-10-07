using AutoMapper;
using Discography.API.ActionConstraints;
using Discography.API.Dtos;
using Discography.API.Enums;
using Discography.Core.Models;
using Discography.Data.Dtos.BandDtos;
using Discography.Data.Extensions;
using Discography.Data.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Discography.API.Controllers
{
    [ApiController]
    [Route("api/bands")]
    public class BandsController : ControllerBase
    {
        private readonly IBandRepository _bandRepository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPropertyCheckerService _propertyCheckerService;


        public BandsController(
                IBandRepository bandRepository,
                IMapper mapper,
                IPropertyMappingService propertyMappingService,
                IPropertyCheckerService propertyCheckerService)
        {
            _bandRepository = bandRepository ?? throw new ArgumentNullException(nameof(bandRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _propertyMappingService = propertyMappingService ?? throw new ArgumentNullException(nameof(propertyMappingService));
            _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
        }


        [HttpGet(Name = "GetBands")]
        [HttpHead]
        public async Task<IActionResult> GetBandsAsync([FromQuery] BandResourceParameters resourceParameters)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<BandDto, Band>(resourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<BandDto>(resourceParameters.Fields))
            {
                return BadRequest();
            }

            var bandsFromRepo = await _bandRepository.ListAsync(resourceParameters);

            var paginationMetadata = new
            {
                totalCount = bandsFromRepo.TotalCount,
                pageSize = bandsFromRepo.PageSize,
                currentPage = bandsFromRepo.CurrentPage,
                totalPages = bandsFromRepo.TotalPages
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            var links = CreateLinksForBands(resourceParameters, bandsFromRepo.HasNext, bandsFromRepo.HasPrevious);

            var shapedBands = _mapper.Map<IEnumerable<BandDto>>(bandsFromRepo).ShapeData(resourceParameters.Fields);

            var shapedBandsWithLinks = shapedBands.Select(band =>
            {
                var bandAsDictionary = band as IDictionary<string, object>;
                var bandLinks = CreateLinksForBand((Guid)bandAsDictionary["Id"], null);
                bandAsDictionary.Add("links", bandLinks);
                return bandAsDictionary;
            });


            var linkedCollectionResource = new
            {
                value = shapedBandsWithLinks,
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
        [HttpGet("{bandId}", Name = "GetBand")]
        public async Task<ActionResult<BandDto>> GetBandAsync(Guid bandId, string fields,
             [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!MediaTypeHeaderValue.TryParse(mediaType,
                out MediaTypeHeaderValue parsedMediaType))
            {
                return BadRequest();
            }

            if (!_propertyCheckerService.TypeHasProperties<BandDto>(fields))
            {
                return BadRequest();
            }

            var bandFromRepoo = await _bandRepository.GetByIdAsync(bandId);

            if (bandFromRepoo == null)
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
            var friendlyResourceToReturn = _mapper.Map<BandDto>(bandFromRepoo)
                .ShapeData(fields) as IDictionary<string, object>;

            if (includeLinks)
            {
                var links = CreateLinksForBand(bandId, fields);
                friendlyResourceToReturn.Add("links", links);
            }

            return Ok(friendlyResourceToReturn);
        }

        [HttpPost(Name = "CreateBand")]
        [RequestHeaderMatchesMediaType("Content-Type",
            "application/json",
            "application/vnd.marvin.bandforcreation+json")]
        [Consumes("application/json",
            "application/vnd.marvin.bandforcreation+json")]
        public async Task<ActionResult<BandDto>> CreateBandAsync(BandForCreationDto band)
        {
            var bandEntity = _mapper.Map<Band>(band);
            await _bandRepository.AddAsync(bandEntity);

            var bandToReturn = _mapper.Map<BandDto>(bandEntity);

            var links = CreateLinksForBand(bandToReturn.Id, null);

            var linkedResourceToReturn = bandToReturn.ShapeData(null)
                as IDictionary<string, object>;
            linkedResourceToReturn.Add("links", links);

            return CreatedAtRoute("GetBand",
                new { bandId = linkedResourceToReturn["Id"] },
                linkedResourceToReturn);
        }

        [HttpPut("{bandId}", Name = "UpdateBand")]
        public async Task<IActionResult> UpdateBandAsync(Guid bandId, BandForUpdateDto band)
        {
            var bandFromRepo = await _bandRepository.GetByIdAsync(bandId);

            if (bandFromRepo == null)
            {
                var bandToAdd = _mapper.Map<Band>(band);
                bandToAdd.Id = bandId;

                await _bandRepository.AddAsync(bandToAdd);


                var bandToReturn = _mapper.Map<BandDto>(bandToAdd);

                return CreatedAtRoute("GetBand",
                    new { bandId = bandToReturn.Id },
                    bandToReturn);
            }

            // map the entity to a BandDto
            // apply the updated field values to that dto
            // map the BandDto back to an entity
            _mapper.Map(band, bandFromRepo);

            await _bandRepository.UpdateAsync(bandFromRepo);

            return NoContent();
        }

        [HttpPatch("{bandId}", Name = "PartiallyUpdateBand")]
        public async Task<ActionResult> PartiallyUpdateBandAsync(Guid bandId, JsonPatchDocument<BandForUpdateDto> patchDocument)
        {
            var bandFromRepo = await _bandRepository.GetByIdAsync(bandId);

            if (bandFromRepo == null)
            {
                var bandDto = new BandForUpdateDto();
                patchDocument.ApplyTo(bandDto, ModelState);

                if (!TryValidateModel(bandDto))
                {
                    return ValidationProblem(ModelState);
                }

                var bandToAdd = _mapper.Map<Band>(bandDto);
                bandToAdd.Id = bandId;

                await _bandRepository.AddAsync(bandToAdd);

                var bandToReturn = _mapper.Map<BandDto>(bandToAdd);

                return CreatedAtRoute("GetBand",
                    new { bandId = bandToReturn.Id },
                    bandToReturn);
            }

            var bandToPatch = _mapper.Map<BandForUpdateDto>(bandFromRepo);
            // add validation
            patchDocument.ApplyTo(bandToPatch, ModelState);

            if (!TryValidateModel(bandToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(bandToPatch, bandFromRepo);

            await _bandRepository.UpdateAsync(bandFromRepo);

            return NoContent();
        }

        [HttpDelete("{bandId}", Name = "DeleteBand")]
        public async Task<ActionResult> DeleteBandAsync(Guid bandId)
        {
            if (!await _bandRepository.ExistsAsync(bandId))
            {
                return NotFound();
            }

            await _bandRepository.DeleteAsync(bandId);

            return NoContent();
        }

        [HttpOptions]
        public IActionResult GetBandOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST,PUT,PATCH");
            return Ok();
        }

        #region Helpers

        public override ActionResult ValidationProblem(
        [ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices
                .GetRequiredService<IOptions<ApiBehaviorOptions>>();
            return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }

        private string CreateBandsResourceUri(BandResourceParameters bandResourceParameters, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link("GetBands",
                      new
                      {
                          fields = bandResourceParameters.Fields,
                          orderBy = bandResourceParameters.OrderBy,
                          pageNumber = bandResourceParameters.PageNumber - 1,
                          pageSize = bandResourceParameters.PageSize,
                          yearOfFormation = bandResourceParameters.YearOfFormation,
                          genre = bandResourceParameters.Genre,
                          searchQuery = bandResourceParameters.SearchQuery
                      });
                case ResourceUriType.NextPage:
                    return Url.Link("GetBands",
                      new
                      {
                          fields = bandResourceParameters.Fields,
                          orderBy = bandResourceParameters.OrderBy,
                          pageNumber = bandResourceParameters.PageNumber + 1,
                          pageSize = bandResourceParameters.PageSize,
                          yearOfFormation = bandResourceParameters.YearOfFormation,
                          genre = bandResourceParameters.Genre,
                          searchQuery = bandResourceParameters.SearchQuery
                      });
                case ResourceUriType.Current:
                default:
                    return Url.Link("GetBands",
                    new
                    {
                        fields = bandResourceParameters.Fields,
                        orderBy = bandResourceParameters.OrderBy,
                        pageNumber = bandResourceParameters.PageNumber,
                        pageSize = bandResourceParameters.PageSize,
                        yearOfFormation = bandResourceParameters.YearOfFormation,
                        genre = bandResourceParameters.Genre,
                        searchQuery = bandResourceParameters.SearchQuery
                    });
            }

        }

        private IEnumerable<LinkDto> CreateLinksForBand(Guid bandId, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(Url.Link("GetBand", new { bandId }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(Url.Link("GetBand", new { bandId, fields }),
                  "self",
                  "GET"));
            }

            links.Add(
               new LinkDto(Url.Link("DeleteBand", new { bandId }),
               "delete_band",
               "DELETE"));

            links.Add(
                new LinkDto(Url.Link("CreateBand", new { bandId }),
                "create_band",
                "POST"));

            links.Add(
                new LinkDto(Url.Link("UpdateBand", new { bandId }),
                "update_band",
                "PUT"));

            links.Add(
                new LinkDto(Url.Link("PartiallyUpdateBand", new { bandId }),
                "partially_update_band",
                "PATCH"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForBands(BandResourceParameters bandsResourceParameters, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            // self 
            links.Add(
               new LinkDto(CreateBandsResourceUri(
                   bandsResourceParameters, ResourceUriType.Current)
               , "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateBandsResourceUri(
                      bandsResourceParameters, ResourceUriType.NextPage),
                  "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new LinkDto(CreateBandsResourceUri(
                        bandsResourceParameters, ResourceUriType.PreviousPage),
                    "previousPage", "GET"));
            }

            return links;
        }

        #endregion
    }
}
