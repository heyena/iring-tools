using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestLibrary
{
  public class BaseTest : ITest
  {
    public virtual String SayHello(String name)
    {
      return "BaseTest says hello " + name;
    }

    public virtual String SayHello2(String name)
    {
      return "BaseTest says hello 2 " + name;
    }
  }
}
