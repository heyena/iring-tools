using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Web;

using iRINGTools.Data;

namespace iRINGTools.Tests
{

  public class TestProjectionEngineRepository: IProjectionEngineRepository  
  {
    private List<IProjectionEngine> _projectionEngineList;

    public TestProjectionEngineRepository()
    {
      _projectionEngineList = new List<IProjectionEngine>();

      for (int i = 1; i <= 5; i++)
      {
        var d = new ProjectionEngine
        { 
          Format = "ProjectionEngine" + i.ToString()
        };

        _projectionEngineList.Add(d);
      }
    }

    public IQueryable<IProjectionEngine> GetProjectionEngines()
    {
      return _projectionEngineList.AsQueryable();
    }
  }
}
