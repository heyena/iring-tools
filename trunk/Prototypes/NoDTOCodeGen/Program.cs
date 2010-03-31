using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdapterPrototype
{
  class Program
  {
    static void Main(string[] args)
    {
      NHibernateDataLayer dataLayer = new NHibernateDataLayer();

      // get
      IList<object> valves = dataLayer.GetList("AdapterPrototype.Valve", null);
      foreach (object valve in valves)
      {
        String valveTag = dataLayer.GetPropertyValue<String>(valve, "tag");
        Single valveDiameter = dataLayer.GetPropertyValue<Single>(valve, "diameter");
        String valveSystem = dataLayer.GetPropertyValue<String>(valve, "system");
        Console.WriteLine(valveTag + ", " + valveDiameter + ", " + valveSystem);
      }

      // update
      object line = dataLayer.Get("AdapterPrototype.Line", null);
      Single diameter = dataLayer.GetPropertyValue<Single>(line, "diameter");
      dataLayer.SetPropertyValue<Single>(line, "diameter", 5);
      dataLayer.Post(line);

      // add
      object newLine = dataLayer.Create("AdapterPrototype.Line");
      dataLayer.SetPropertyValue<String>(newLine, "tag", "L0001");
      dataLayer.SetPropertyValue<Single>(newLine, "diameter", 1.5f);
      dataLayer.SetPropertyValue<String>(newLine, "system", "AB");
      dataLayer.Post(newLine);

      Console.WriteLine("Press any key to continue ...");
      Console.ReadKey();
    }
  }
}