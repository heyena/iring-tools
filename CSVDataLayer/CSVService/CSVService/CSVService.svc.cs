using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Configuration;
using org.iringtools.utility;

namespace CSVService
{
  public class CSVService : ICSVService
  {
    public string Save(string fileName, string content)
    {
      try
      {
        string path = ConfigurationManager.AppSettings["targetLocation"];
        Utility.WriteString(content, path + fileName);

        return "File \"" + path + fileName + "\" has been saved successfully.";
      }
      catch (Exception ex)
      {
        return "ERROR: " + ex;
      }
    }
  }
}
