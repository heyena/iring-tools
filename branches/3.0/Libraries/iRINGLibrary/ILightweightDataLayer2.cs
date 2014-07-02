using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using log4net;
using org.iringtools.adapter;
using org.iringtools.utility;
namespace org.iringtools.library
{
    interface ILightweightDataLayer2 : ILightweightDataLayer
    {
        IList<IDataObject> Search(string objectType, string query, int pageSize, int startIndex);
        IList<IDataObject> Search(string objectType, string query, DataFilter filter, int pageSize, int startIndex);
    }

    public abstract class BaseLightweightDataLayer2 : BaseLightweightDataLayer, ILightweightDataLayer2
    {
        public BaseLightweightDataLayer2(AdapterSettings settings)
            : base(settings)
        {
            _settings = settings;
        }

        public abstract IList<IDataObject> Search(string objectType, string query, int pageSize, int startIndex);
        public abstract IList<IDataObject> Search(string objectType, string query, DataFilter filter, int pageSize, int startIndex);
    }
}
