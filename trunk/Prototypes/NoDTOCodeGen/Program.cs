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
      //NHibernateDataLayer dataLayer = new NHibernateDataLayer();

      // get
      //DateTime b = DateTime.Now;
      //IList<DataObject> valves = dataLayer.GetList("AdapterPrototype.Valve", null);
      ////DateTime e = DateTime.Now;
      ////TimeSpan d = e.Subtract(b);
      ////Console.WriteLine(String.Format("Execution time {0}:{1}.{2} of {3} rows", d.Minutes, d.Seconds, d.Milliseconds, valves.Count));
      //foreach (DataObject valve in valves)
      //{
      //  String valveTag = (String)(dataLayer.GetPropertyValue(valve, "tag"));
      //  Single? valveDiameter = (Single?)(dataLayer.GetPropertyValue(valve, "diameter"));
      //  String valveSystem = (String)(dataLayer.GetPropertyValue(valve, "system"));
      //  Console.WriteLine(valveTag + ", " + valveDiameter + ", " + valveSystem);
      //}

      //// update
      //object line = dataLayer.Get("AdapterPrototype.Line", null);
      //Single diameter = dataLayer.GetPropertyValue<Single>(line, "diameter");
      //dataLayer.SetPropertyValue<Single>(line, "diameter", 5);
      //dataLayer.Post(line);

      //// add
      //object newLine = dataLayer.Create("AdapterPrototype.Line");
      //dataLayer.SetPropertyValue<String>(newLine, "tag", "L0001");
      //dataLayer.SetPropertyValue<Single>(newLine, "diameter", 1.5f);
      //dataLayer.SetPropertyValue<String>(newLine, "system", "AB");
      //dataLayer.Post(newLine);

      // performance test against 10,000 new valves
      //DateTime b = DateTime.Now;
      //List<object> newValves = new List<object>();
      //for (int i = 1; i <= 500000; i++)
      //{
      //  object newValve = dataLayer.Create("AdapterPrototype.Valve");
      //  dataLayer.SetPropertyValue(newValve, "tag", "VA-" + i);
      //  dataLayer.SetPropertyValue(newValve, "componentType", "Valve");
      //  dataLayer.SetPropertyValue(newValve, "diameter", i);
      //  dataLayer.SetPropertyValue(newValve, "uomDiameter", "INCH" + i);
      //  dataLayer.SetPropertyValue(newValve, "rating", i + "lb");
      //  dataLayer.SetPropertyValue(newValve, "unit", "U" + i);
      //  dataLayer.SetPropertyValue(newValve, "projectNumber", "PN" + i);
      //  dataLayer.SetPropertyValue(newValve, "pid", "M2-AB" + i);
      //  dataLayer.SetPropertyValue(newValve, "lineTag", "LA-" + i);
      //  dataLayer.SetPropertyValue(newValve, "quantity", i);
      //  dataLayer.SetPropertyValue(newValve, "isCloned", false);
      //  newValves.Add(newValve);
      //}

      //DateTime b = DateTime.Now;
      //List<DataObject> newValves = new List<DataObject>();
      //for (int i = 1; i <= 500000; i++)
      //{
      //  DataObject newValve = dataLayer.Create("AdapterPrototype.Valve");
      //  dataLayer.SetPropertyValue(newValve, "tag", "VA-" + i);
      //  dataLayer.SetPropertyValue(newValve, "componentType", "Valve");
      //  dataLayer.SetPropertyValue(newValve, "diameter", i);
      //  dataLayer.SetPropertyValue(newValve, "uomDiameter", "INCH" + i);
      //  dataLayer.SetPropertyValue(newValve, "rating", i + "lb");
      //  dataLayer.SetPropertyValue(newValve, "unit", "U" + i);
      //  dataLayer.SetPropertyValue(newValve, "projectNumber", "PN" + i);
      //  dataLayer.SetPropertyValue(newValve, "pid", "M2-AB" + i);
      //  dataLayer.SetPropertyValue(newValve, "lineTag", "LA-" + i);
      //  dataLayer.SetPropertyValue(newValve, "system", "AB-" + i);
      //  dataLayer.SetPropertyValue(newValve, "quantity", i);
      //  dataLayer.SetPropertyValue(newValve, "isCloned", false);
      //  newValves.Add(newValve);
      //}

      //List<Valve> newValves = new List<Valve>();
      //for (int i = 1; i <= 500000; i++)
      //{
      //  Valve newValve = new Valve();
      //  newValve.tag = "VA-" + i;
      //  newValve.componentType = "Valve";
      //  newValve.diameter = i;
      //  newValve.uomDiameter = "INCH" + i;
      //  newValve.rating = i + "lb";
      //  newValve.unit = "U" + i;
      //  newValve.projectNumber = "PN" + i;
      //  newValve.pid = "M2-AB" + i;
      //  newValve.lineTag = "LA-" + i;
      //  newValve.system = "AB-" + i;
      //  newValve.quantity = i;
      //  newValve.isCloned = false;
      //  newValves.Add(newValve);
      //}

      //DateTime e = DateTime.Now;
      //TimeSpan d = e.Subtract(b);
      //Console.WriteLine(String.Format("Creation time {0}:{1}.{2} of {3} objects", d.Minutes, d.Seconds, d.Milliseconds, newValves.Count));

      //dataLayer.PostList(newValves);
      //DateTime e2 = DateTime.Now;
      //TimeSpan d2 = e2.Subtract(e);
      //Console.WriteLine(String.Format("Posting time {0}:{1}.{2} of {3} objects", d2.Minutes, d2.Seconds, d2.Milliseconds, newValves.Count));
      
      // graph
      NHibernateDataLayer3 dataLayer = new NHibernateDataLayer3();
      object aValve = dataLayer.Get("AdapterPrototype.Valve", null);
      
      Graph graph = new Graph();
      graph.Name = "Valves";
      graph.ClassId = Graph.RDL + "R97295617945";
      graph.Identifier = "tag";
      graph.IdentifierValue = Convert.ToString(dataLayer.GetPropertyValue(aValve, "tag"));

      String tag = (String)dataLayer.GetPropertyValue(aValve, "tag");
      graph.DataObjects.Add("Valve", aValve);

      Template template = new Template()
      {
        TemplateId = Graph.TPL + "R31178164691",
        Name = "ClassifiedIdentification",
        ClassRoleId = Graph.TPL + "R47421421239"
      };

      Role role1 = new Role();
      role1.RoleId = Graph.TPL + "R17963188524";
      role1.Name = "identifier";
      role1.PropertyName = "Valve.tag";
      role1.DataType = Graph.XSD + "string";
      role1.Value = tag;
      template.Roles.Add(role1);

      Role role2 = new Role();
      role2.RoleId = Graph.TPL + "R43718740838";
      role2.Name = "identificationType";
      role2.Reference = Graph.RDL + "R92093626759";
      template.Roles.Add(role2);

      graph.Templates.Add("guid2", template);

      string path = @"..\..\graph.xml";
      Utility.Write<Graph>(graph, path, false);

      Console.WriteLine("Press any key to continue ...");
      Console.ReadKey();
    }
  }
}