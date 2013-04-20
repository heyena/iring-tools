using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using org.iringtools.mapping;
using org.iringtools.library;
using org.iringtools.adapter.projection;
using org.iringtools.utility;

namespace org.iringtools.adapter
{
  public class DtiTask
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DtiTask));

    private ManualResetEvent _doneEvent;
    private DtoProjectionEngine _projectionLayer;
    private IDataLayer _dataLayer;
    private GraphMap _graphMap;
    private DataFilter _filter;
    private int _pageSize;
    private int _startIndex;
    private DataTransferIndices _dataTransferIndices;

    public DtiTask(ManualResetEvent doneEvent, DtoProjectionEngine projectionLayer, IDataLayer dataLayer, 
      GraphMap graphMap, DataFilter filter, int pageSize, int startIndex)
    {
      _doneEvent = doneEvent;
      _projectionLayer = projectionLayer;
      _dataLayer = dataLayer;
      _graphMap = graphMap;
      _filter = Utility.CloneDataContractObject<DataFilter>(filter);
      _pageSize = pageSize;
      _startIndex = startIndex;
    }

    public void ThreadPoolCallback(object threadContext)
    {
      _logger.Debug(string.Format("Starting worker process for getting paged data {0}-{1}.", _startIndex, _startIndex + _pageSize));
        
      int threadIndex = (int)threadContext;
      IList<IDataObject> dataObjects = _dataLayer.Get(_graphMap.dataObjectName, _filter, _pageSize, _startIndex);

      _logger.Debug(string.Format("Worker process for getting paged data {0}-{1} received {2} data objects", _startIndex, _startIndex + _pageSize, dataObjects.Count));

      if (dataObjects != null)
      {
        _dataTransferIndices = _projectionLayer.GetDataTransferIndices(_graphMap, dataObjects, string.Empty);
      }

      _logger.Debug(string.Format("Worker process for getting paged data {0}-{1} completed.", _startIndex, _startIndex + _pageSize));

      _doneEvent.Set();
    }

    public DataTransferIndices DataTransferIndices
    {
      get { return _dataTransferIndices; }
    }
  }
}
