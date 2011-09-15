using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestLibrary;

namespace TestClient
{
  class Program
  {
    /*
     * NOTES: 
     *   TestClient references TestLibrary from "TestLibrary" project.
     *   Thus, always get most recent updates from ITest interface.
     */
    static void Main(string[] args)
    {
      Type testType = Type.GetType("TestModule.ConcreteTest, TestModule");
      ITest test = (ITest) Activator.CreateInstance(testType);

      Console.WriteLine(test.SayHello("iRING"));
      Console.WriteLine(test.SayHello2("iRING"));
      Console.WriteLine(Util.SaySomething("something else!"));

      Console.ReadKey();
    }
  }
}
