using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.utility;
using System.Xml.Xsl;

namespace SSDLTransform
{
  class Program
  {
    static void Main(string[] args)
    {
      string ssdlPath = String.Empty;
      string resultPath = String.Empty;

      if (args.Length >= 2)
      {
        ssdlPath = args[0];
        resultPath = args[1];
      }
      else
      {
        ssdlPath = ConfigurationManager.AppSettings["SSDLFilePath"];
        resultPath = ConfigurationManager.AppSettings["ResultFilePath"];
      }

      if (String.IsNullOrEmpty(ssdlPath))
      {
        Console.WriteLine("Usage: \n\nSSDLTransform.exe SSDLFilePath [ResultFilePath]\n");
      }
      else
      {
        try
        {
          string ssdl = Utility.ReadString(ssdlPath);
          string result = Utility.Transform(ssdl, @"C:\iring-tools-1.2\Utilities\SSDLTransform\SSDL2DBDictionary.xsl", null);
          Utility.WriteString(result, resultPath);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex);
          Console.ReadKey();
        }
      }
    }
  }
}
