using Discography.Core.Models;
using Discography.Data.Dtos.AlbumDtos;
using Discography.Data.Dtos.BandDtos;
using Discography.Data.Dtos.MusicianDtos;
using Discography.Data.Dtos.SongDtos;
using Discography.Data.Helpers;
using Discography.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Discography.Data.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private Dictionary<string, PropertyMappingValue> _bandPropertyMapping =
          new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
          {
               { "Id", new PropertyMappingValue(new List<string>() { "Id" }) },
               { "Name", new PropertyMappingValue(new List<string>() { "Name" }) },
               { "YearOfFormation", new PropertyMappingValue(new List<string>() { "YearOfFormation" }) },
               { "AlsoKnownAs", new PropertyMappingValue(new List<string>() { "AlsoKnownAs" }) },
               { "ActivePeriods", new PropertyMappingValue(new List<string>() { "ActivePeriods" }, true) }
              // moze eventualno po broju clanova ako dodas i tu listu band, kao i po zanrovima
          };

        private Dictionary<string, PropertyMappingValue> _musicianPropertyMapping =
          new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
          {
               { "Id", new PropertyMappingValue(new List<string>() { "Id" }) },
               { "Name", new PropertyMappingValue(new List<string>() {  "FirstName", "MiddleName" ,"LastName" }) },
               { "AlsoKnownAs", new PropertyMappingValue(new List<string>() { "AlsoKnownAs" }) },
               { "DateOfBirth", new PropertyMappingValue(new List<string>() { "DateOfBirth" }) },
               { "DateOfDeath", new PropertyMappingValue(new List<string>() { "DateOfDeath" }) }
          };

        private Dictionary<string, PropertyMappingValue> _albumPropertyMapping =
          new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
          {
               { "Id", new PropertyMappingValue(new List<string>() { "Id" }) },
               { "Title", new PropertyMappingValue(new List<string>() {  "Title"}) },
               { "Label", new PropertyMappingValue(new List<string>() { "Label" }) },
               { "DateReleased", new PropertyMappingValue(new List<string>() { "DateReleased" }) }
          };

        private Dictionary<string, PropertyMappingValue> _songPropertyMapping =
        new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
        {
               { "Id", new PropertyMappingValue(new List<string>() { "Id" }) },
               { "Title", new PropertyMappingValue(new List<string>() {  "Title"}) },
               { "Duration", new PropertyMappingValue(new List<string>() { "DurationInSeconds" }) },
               { "DateRecorded", new PropertyMappingValue(new List<string>() { "DateRecorded" }) },
               { "DateReleased", new PropertyMappingValue(new List<string>() { "DateReleased" }) }
        };

        private IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            _propertyMappings.Add(new PropertyMapping<BandDto, Band>(_bandPropertyMapping));
            _propertyMappings.Add(new PropertyMapping<MusicianDto, Musician>(_musicianPropertyMapping));
            _propertyMappings.Add(new PropertyMapping<AlbumDto, Album>(_albumPropertyMapping));
            _propertyMappings.Add(new PropertyMapping<SongDto, Song>(_songPropertyMapping));
        }

        public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                return true;
            }

            // the string is separated by ",", so we split it.
            var fieldsAfterSplit = fields.Split(',');

            // run through the fields clauses
            foreach (var field in fieldsAfterSplit)
            {
                // trim
                var trimmedField = field.Trim();

                // remove everything after the first " " - if the fields 
                // are coming from an orderBy string, this part must be 
                // ignored
                var indexOfFirstSpace = trimmedField.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ?
                    trimmedField : trimmedField.Remove(indexOfFirstSpace);

                // find the matching property
                if (!propertyMapping.ContainsKey(propertyName))
                {
                    return false;
                }
            }
            return true;
        }


        public Dictionary<string, PropertyMappingValue> GetPropertyMapping
           <TSource, TDestination>()
        {
            // get matching mapping
            var matchingMapping = _propertyMappings
                .OfType<PropertyMapping<TSource, TDestination>>();

            if (matchingMapping.Count() == 1)
            {
                return matchingMapping.First()._mappingDictionary;
            }

            throw new Exception($"Cannot find exact property mapping instance " +
                $"for <{typeof(TSource)},{typeof(TDestination)}");
        }
    }
}
