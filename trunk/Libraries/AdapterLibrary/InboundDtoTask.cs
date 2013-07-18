using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using org.iringtools.mapping;
using org.iringtools.library;
using org.iringtools.adapter.projection;

namespace org.iringtools.adapter
{
  public class DataTransferObjectsTask
  {
    private ManualResetEvent _doneEvent;
    private DtoProjectionEngine _projectionLayer;
    private DataLayerGateway _dataLayerGateway;
    private GraphMap _graphMap;
    private DataObject _objectType;
    private DataTransferObjects _dataTransferObjects;
    private Response _response;

    public DataTransferObjectsTask(ManualResetEvent doneEvent, DtoProjectionEngine projectionLayer, DataLayerGateway dataLayerGateway,
      GraphMap graphMap, DataObject objectType, DataTransferObjects dataTransferObjects)
    {
      _doneEvent = doneEvent;
      _projectionLayer = projectionLayer;
      _dataLayerGateway = dataLayerGateway;
      _objectType = objectType;
      _graphMap = graphMap;
      _dataTransferObjects = dataTransferObjects;
    }

    public void ThreadPoolCallback(object threadContext)
    {
      int threadIndex = (int)threadContext;

      if (_dataTransferObjects != null)
      {
        List<IDataObject> dataObjects = _projectionLayer.ToDataObjects(_graphMap, ref _dataTransferObjects);
        
        if (dataObjects != null)
        {
          _response = _dataLayerGateway.Update(_objectType, dataObjects);
        }
      }

      _doneEvent.Set();
    }

    public Response Response
    {
      get { return _response; }
    }
  }
}
