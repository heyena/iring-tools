using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRINGTools.Data
{
  public interface IProjectionEngineRepository
  {
    IQueryable<IProjectionEngine> GetProjectionEngines();
  }
}
