using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.utility;
using System.Xml.Linq;

namespace NUnit.Tests
{
  public abstract class BaseTest
  {
    public void ResetDatabase()
    {
      try
      {
        string sql = Utility.ReadString(@"..\..\iRINGTools.Services\App_Data\ABC.Data.Complete.sql");

        XDocument nhConfig = XDocument.Load(@".\XML\nh-configuration.12345_000.ABC.xml");

        var properties = nhConfig
          .Element("configuration")
          .Element("{urn:nhibernate-configuration-2.2}hibernate-configuration")
          .Element("{urn:nhibernate-configuration-2.2}session-factory")
          .Descendants("{urn:nhibernate-configuration-2.2}property");

        var property = from p in properties
                       where p.Attribute("name").Value == "connection.connection_string"
                       select p;

        string connectionString = property.FirstOrDefault().Value;

        if (!connectionString.ToUpper().Contains("DATA SOURCE"))
        {
          connectionString = EncryptionUtility.Decrypt(connectionString);
        }

        Utility.ExecuteSQL(sql, connectionString);
      }
      catch (Exception ex)
      {
        string message = "Error cleaning up Database: " + ex;
        throw new Exception(message);
      }
    }
  }
}
