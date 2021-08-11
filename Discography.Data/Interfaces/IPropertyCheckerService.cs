using System;
using System.Collections.Generic;
using System.Text;

namespace Discography.Data.Interfaces
{
    public interface IPropertyCheckerService
    {
        bool TypeHasProperties<T>(string fields);
    }
}
