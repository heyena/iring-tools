using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using Ciloci.Flee;
using log4net;
using Ninject;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;

namespace iRINGTools.SDK.SP3DDataLayer
{
    public class SP3DDataLayer : BaseDataLayer, IDataLayer2
    {
        [Inject]
        public SP3DDataLayer(AdapterSettings settings, IKernel kernel)
        {
            _settings = settings;
        }

        public override long GetCount(string objectType, DataFilter filter)
        {
            try
            {
                //NOTE: pageSize of 0 indicates that all rows should be returned.
                IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

                return dataObjects.Count();
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetIdentifiers: " + ex);

                throw new Exception(
                  "Error while getting a count of type [" + objectType + "].",
                  ex
                );
            }
        }

        public override IList<string> GetIdentifiers(string objectType, DataFilter filter)
        {
            try
            {
                List<string> identifiers = new List<string>();
                return identifiers;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetIdentifiers: " + ex);

                throw new Exception(
                  "Error while getting a list of identifiers of type [" + objectType + "].",
                  ex
                );
            }
        }

        public override IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
        {
            throw new NotImplementedException();
        }

        public override Response Post(IList<IDataObject> dataObjects)
        {
            Response response = new Response();

            return response;


        }

        public override Response Delete(string objectType, IList<string> identifiers)
        {
            // Not gonna do it. Wouldn't be prudent.
            Response response = new Response();

            return response;
        }

        public override Response Delete(string objectType, DataFilter filter)
        {
            // Not gonna do it. Wouldn't be prudent with a filter either.
            Response response = new Response();

            return response;
        }

        public override DataDictionary GetDictionary()
        {
            DataDictionary dataDictionary = new DataDictionary();

            return dataDictionary;
        }

        public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
        {
            try
            {
                IList<IDataObject> _dataObjects = new List<IDataObject>();

                return _dataObjects;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetList: " + ex);
                throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
            }
        }

        public override IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
        {
            try
            {
                IList<IDataObject> _dataObjects = new List<IDataObject>();

                return _dataObjects;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetList: " + ex);

                throw new Exception(
                  "Error while getting a list of data objects of type [" + objectType + "].",
                  ex
                );
            }
        }
    }
}
