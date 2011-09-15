using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestLibrary;

namespace TestModule
{
  public class ConcreteTest : BaseTest
  {
    public override String SayHello(String name)
    {
      return "ConcreteTest says hello " + name;
    }
  }
}
