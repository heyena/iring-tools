using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using org.iringtools.mapping;
using org.iringtools.library;

namespace org.iringtools.adapter
{
  public class DataObjectTask
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DataObjectTask));
    
    private ManualResetEvent _doneEvent;
    private IDataLayer _dataLayer;
    private string _objectType;
    private DataFilter _filter;
    private int _pageSize;
    private int _startIndex;
    private IList<IDataObject> _dataObjects;

    public DataObjectTask(ManualResetEvent doneEvent, IDataLayer dataLayer, string objectType, DataFilter filter,
      int pageSize, int startIndex)
    {
      _doneEvent = doneEvent;
      _dataLayer = dataLayer;
      _objectType = objectType;
      _filter = filter;
      _pageSize = pageSize;
      _startIndex = startIndex;
    }

    public void ThreadPoolCallback(object threadContext)
    {
      int threadIndex = (int)threadContext;
      _logger.Debug(string.Format("DataObjectTask {0} started.", threadIndex));
      _dataObjects = _dataLayer.Get(_objectType, _filter, _pageSize, _startIndex);
      _logger.Debug(string.Format("DataObjectTask {0} completed.", threadIndex));
      _doneEvent.Set();
    }

    public IList<IDataObject> DataObjects
    {
      get { return _dataObjects; }
    }
  }
}
