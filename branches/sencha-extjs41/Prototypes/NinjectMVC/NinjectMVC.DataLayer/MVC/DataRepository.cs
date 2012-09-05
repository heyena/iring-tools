using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NinjectMVC.DataLayer
{
  public interface IDataRepository
  {
    string GetMessage();
  }

  public class DataRepository : IDataRepository
  {
    private DataProvider _provider = null; 

    public DataRepository()
    {
      _provider = new DataProvider();
    }

    public string GetMessage()
    {
      return _provider.GetMessage();
    }
  }
}
