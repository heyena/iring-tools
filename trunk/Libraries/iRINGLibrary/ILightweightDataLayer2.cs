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
    public interface ILightweightDataLayer2 : ILightweightDataLayer
    {
        List<SerializableDataObject> GetIndex(DataObject objectType);
        List<SerializableDataObject> GetPage(DataObject objectType, List<SerializableDataObject> identifiers);
    }

    public abstract class BaseLightweightDataLayer2 : BaseLightweightDataLayer, ILightweightDataLayer2
    {
        public BaseLightweightDataLayer2(AdapterSettings settings)
            : base(settings)
        {
            _settings = settings;
        }

        public abstract List<SerializableDataObject> GetIndex(DataObject objectType);
        public abstract List<SerializableDataObject> GetPage(DataObject objectType, List<SerializableDataObject> identifiers);
    }
}
