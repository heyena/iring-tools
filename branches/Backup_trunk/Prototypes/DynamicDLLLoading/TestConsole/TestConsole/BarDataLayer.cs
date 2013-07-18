using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClassLibrary;
using Ninject;

namespace TestConsole
{
  public class BarDataLayer : IDataLayer
  {
    private GlobalSettings globalSettings;

    [Inject]
    public BarDataLayer(GlobalSettings globalSettings)
    {
      this.globalSettings = globalSettings;
    }

    public string GetName()
    {
      return globalSettings["AppContext"] + ": bar data layer";
    }
  }
}
