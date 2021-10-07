using AutoMapper;
using Discography.API.Helpers;
using Discography.Core.Models;
using Discography.Data.Dtos.BandDtos;
using Discography.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discography.API.Controllers
{
    [ApiController]
    [Route("api/bandcollections")]
    public class BandCollectionsController : ControllerBase
    {
        private readonly IBandRepository _bandRepository;
        private readonly IMapper _mapper;

        public BandCollectionsController(IBandRepository bandRepository,
            IMapper mapper)
        {
            _bandRepository = bandRepository ??
                throw new ArgumentNullException(nameof(bandRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("({ids})", Name = "GetBandCollection")]
        public async Task<IActionResult> GetBandCollectionAsync(
        [FromRoute]
        [ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return BadRequest();
            }

            var bandEntities = await _bandRepository.GetBandsAsync(ids);

            if (ids.Count() != bandEntities.Count())
            {
                return NotFound();
            }

            var bandsToReturn = _mapper.Map<IEnumerable<BandDto>>(bandEntities);

            return Ok(bandsToReturn);
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<BandDto>>> CreateBandCollection(
            IEnumerable<BandForCreationDto> bandCollection)
        {
            var Entitiesband = _mapper.Map<IEnumerable<Band>>(bandCollection);
            foreach (var band in Entitiesband)
            {
                await _bandRepository.AddAsync(band);
            }

            var bandCollectionToReturn = _mapper.Map<IEnumerable<BandDto>>(Entitiesband);
            var idsAsString = string.Join(",", bandCollectionToReturn.Select(a => a.Id));
            return CreatedAtRoute("GetBandCollection",
             new { ids = idsAsString },
             bandCollectionToReturn);
        }
    }
}
