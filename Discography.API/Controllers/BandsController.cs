using AutoMapper;
using Discography.API.Dtos;
using Discography.API.Enums;
using Discography.Core.Models;
using Discography.Data.Dtos;
using Discography.Data.Extensions;
using Discography.Data.Interfaces;
using Discography.Data.Models;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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


            var links = CreateLinksForAuthors(resourceParameters, bandsFromRepo.HasNext, bandsFromRepo.HasPrevious);

            var shapedAuthors = _mapper.Map<IEnumerable<BandDto>>(bandsFromRepo).ShapeData(resourceParameters.Fields);

            var shapedBandsWithLinks = shapedAuthors.Select(band =>
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

            return Ok(_mapper.Map<List<BandDto>>(bandsFromRepo));
        }

        [HttpGet("{bandId}", Name = "GetBand")]
        // [ResponseCache(Duration = 120)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = 1000)]
        [HttpCacheValidation(MustRevalidate = false)]
        public async Task<ActionResult<BandDto>> GetBandAsync(Guid bandId)
        {
            var bandFromRepoo = await _bandRepository.GetByIdAsync(bandId);

            if (bandFromRepoo == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<BandDto>(bandFromRepoo));
        }

        [HttpPost(Name = "CreateBand")]
        public async Task<ActionResult<BandDto>> CreateBandAsync(BandForCreationDto band)
        {
            var bandEntity = _mapper.Map<Band>(band);
            await _bandRepository.AddAsync(bandEntity);

            var bandToReturn = _mapper.Map<BandDto>(bandEntity);
            return CreatedAtRoute("GetBand",
                new { bandId = bandToReturn.Id },
                bandToReturn);
        }

        [HttpPut("{bandId}")]
        public async Task<IActionResult> UpdateCourseForAuthor(Guid bandId, BandForUpdateDto band)
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

        [HttpPatch("{bandId}")]
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

        [HttpDelete("{bandId}")]
        public async Task<ActionResult> DeleteCourseForAuthor(Guid bandId)
        {
            if (!await _bandRepository.ExistsAsync(bandId))
            {
                return NotFound();
            }

            await _bandRepository.DeleteAsync(bandId);

            return NoContent();
        }

        public override ActionResult ValidationProblem(
        [ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices
                .GetRequiredService<IOptions<ApiBehaviorOptions>>();
            return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }

        /// Validacija kod kreiranja i upserting za name i sta vec treba
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

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

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForAuthors(BandResourceParameters authorsResourceParameters, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            // self 
            links.Add(
               new LinkDto(CreateBandsResourceUri(
                   authorsResourceParameters, ResourceUriType.Current)
               , "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateBandsResourceUri(
                      authorsResourceParameters, ResourceUriType.NextPage),
                  "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new LinkDto(CreateBandsResourceUri(
                        authorsResourceParameters, ResourceUriType.PreviousPage),
                    "previousPage", "GET"));
            }

            return links;
        }

    }
}
