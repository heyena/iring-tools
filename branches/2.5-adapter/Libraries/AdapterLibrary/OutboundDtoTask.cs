using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using org.iringtools.mapping;
using org.iringtools.library;
using org.iringtools.adapter.projection;
using System.Xml.Linq;
using Microsoft.ServiceModel.Web;

namespace org.iringtools.adapter
{
  public class OutboundDtoTask
  {
    private ManualResetEvent _doneEvent;
    private DtoProjectionEngine _projectionLayer;
    private DataLayerGateway _dataLayerGateway;
    private GraphMap _graphMap;
    private DataObject _dataObject;
    private List<string> _identifiers;
    private DataTransferObjects _dataTransferObjects;

    public OutboundDtoTask(ManualResetEvent doneEvent, DtoProjectionEngine projectionLayer, DataLayerGateway dataLayerGateway,
      GraphMap graphMap, DataObject dataObject, List<string> identifiers)
    {
      _doneEvent = doneEvent;
      _projectionLayer = projectionLayer;
      _dataLayerGateway = dataLayerGateway;
      _graphMap = graphMap;
      _dataObject = dataObject;
      _identifiers = identifiers;
    }

    public void ThreadPoolCallback(object threadContext)
    {
      int threadIndex = (int)threadContext;

      if (_identifiers != null && _identifiers.Count > 0)
      {
        List<IDataObject> dataObjects = _dataLayerGateway.Get(_dataObject, _identifiers);

        if (dataObjects != null)
        {
          XDocument dtoDoc = _projectionLayer.ToXml(_graphMap.name, ref dataObjects);

          if (dtoDoc != null && dtoDoc.Root != null)
          {
            _dataTransferObjects = SerializationExtensions.ToObject<DataTransferObjects>(dtoDoc.Root);
          }
        }
      }

      _doneEvent.Set();
    }

    public DataTransferObjects DataTransferObjects
    {
      get { return _dataTransferObjects; }
    }
  }
}
