using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClassLibrary;
using OtherClassLibrary;
using Ninject;

namespace FooDataLayer
{
  public class FooDataLayer : IDataLayer
  {
    [Inject]
    public GlobalSettings globalSettings { set; private get; }

    public string GetName()
    {
      SomeClass someClass = new SomeClass();
      return globalSettings["AppContext"] + ": foo data layer " + someClass.doSomething();
    }
  }
}
