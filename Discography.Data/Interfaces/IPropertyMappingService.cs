using Discography.Data.Helpers;
using System.Collections.Generic;

namespace Discography.Data.Interfaces
{
    public interface IPropertyMappingService
    {
        Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>();
        bool ValidMappingExistsFor<TSource, TDestination>(string fields);
    }
}