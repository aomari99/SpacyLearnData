using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpacyLearnData
{
    public static class Extension
    {
        public static bool notEmpty<T>( this ObservableCollection<T> value )
        {
            return value.Count > 0 ? true : false;
        }

    }
}
