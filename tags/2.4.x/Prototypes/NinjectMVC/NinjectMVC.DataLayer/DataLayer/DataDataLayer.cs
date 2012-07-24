using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NinjectMVC.DataLayer
{
  public class DataDataLayer : NinjectMVC.Library.IDataLayer
  {
    private DataProvider _provider = null;

    public DataDataLayer()
    {
      _provider = new DataProvider();
    }

    public string GetMessage()
    {
      return _provider.GetMessage();
    }
  }
}
