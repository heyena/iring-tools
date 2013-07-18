using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using ClassLibrary;

namespace TestConsole
{
  public class Projection
  {
    private IDataLayer dataLayer;

    [Inject]
    public Projection(IDataLayer dataLayer)
    {
      this.dataLayer = dataLayer;
    }

    public String ProjectName()
    {
      return dataLayer.GetName();
    }
  }
}
