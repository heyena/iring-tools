using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.utility;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

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

      //// mapping + dto
      //Graph graph = new Graph();
      //graph.Name = "Valves";
      //graph.Id = "R19192462550";

      //graph.DataObjects = new Dictionary<string, object>();
      //object aValve = dataLayer.Get("AdapterPrototype.Valve", null);
      //String tag = dataLayer.GetPropertyValue<String>(aValve, "tag");
      //graph.DataObjects.Add("Valve", aValve);

      //graph.Templates = new Dictionary<string, Template>();
      //Template template = new Template()
      //{
      //  Id = "R66921101783",
      //  Name = "IdentificationByTag"
      //};

      //PropertyRole role1 = new PropertyRole();
      //role1.Id = "R22674749688";
      //role1.Name = "valIdentifier";
      //role1.Property = "Valve.tag";
      //role1.DataType = "string";
      //role1.Value = tag;
      //graph.Identifier = tag;
      //template.Roles = new List<Role>();
      //template.Roles.Add(role1);

      //PropertyRole role2 = new PropertyRole();
      //role2.Id = "R77443358818";
      //role2.Name = "valValue";
      //role2.Property = "Valve.diameter";
      //role2.DataType = "string";
      //role2.Value = Convert.ToString(dataLayer.GetPropertyValue<Single>(aValve, "diameter"));
      //template.Roles.Add(role2);

      //ReferenceRole role3 = new ReferenceRole();
      //role3.Id = "R30790108016";
      //role3.Reference = "R40471041754";
      //template.Roles.Add(role3);

      //graph.Templates.Add("R66921101783", template);

      //string path = @"C:\Development\VS08 Projects\AdapterPrototype\AdapterPrototype\graph.xml";
      //Utility.Write<Graph>(graph, path, false);

      Console.WriteLine("Press any key to continue ...");
      Console.ReadKey();
    }
  }
}