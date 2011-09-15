using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestLibrary;

namespace TestClient
{
  class Program
  {
    static void Main(string[] args)
    {
      Type testType = Type.GetType("TestModule.ConcreteTest, TestModule");
      ITest test = (ITest) Activator.CreateInstance(testType);
      Console.WriteLine(test.SayHello("iRING"));

      Console.ReadKey();
    }
  }
}
