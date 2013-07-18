using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using ClassLibrary;
using System.Reflection;

namespace TestConsole
{
  class Program
  {
    static void Main(string[] args)
    {
      var settings = new NinjectSettings { LoadExtensions = false };
      IKernel kernel = new StandardKernel(settings, new TestModule());
      
      // load dll from bin folder
      IDataLayer dataLayer = kernel.Get<IDataLayer>("bar");
      kernel.Rebind<IDataLayer>().ToConstant(dataLayer);
      Projection projection = kernel.Get<Projection>();
      Console.WriteLine(projection.ProjectName());

      // load dll from other location than bin
      string baseDir = System.AppDomain.CurrentDomain.BaseDirectory;
      string assemblyPath = baseDir + @"..\..\..\..\FooDataLayer\FooDataLayer\bin\Debug\FooDataLayer.dll";
      Assembly assembly = Assembly.LoadFrom(assemblyPath);
      Type type = assembly.GetType("FooDataLayer.FooDataLayer");      
      dataLayer = (IDataLayer) Activator.CreateInstance(type); ;
      kernel.Rebind<IDataLayer>().ToConstant(dataLayer);
      projection = kernel.Get<Projection>();
      Console.WriteLine(projection.ProjectName());

      Console.ReadKey();
    }
  }
}
