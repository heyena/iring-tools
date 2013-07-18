using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestLibrary;

namespace TestModule
{
  /*
   * NOTES: 
   *   ConcreteTest inherits from the old BaseTest class, 
   *   which does not have new method SayHello2.
   *   Visit BaseTest definition to see it yourself. 
   */
  public class ConcreteTest : BaseTest, ITest
  {
    public override String SayHello(String name)
    {
      return "ConcreteTest says hello " + name + ", " + Util.SaySomething();
    }
  }
}
