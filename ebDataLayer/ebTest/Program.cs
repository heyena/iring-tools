using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.datalayer;

namespace ebTest
{
  class Program
  {
    static void Main(string[] args)
    {
      try
      {
        string dataPath = @"C:\development\csharp\ebTest\EBLibrary\App_Data\";

        AdapterSettings settings = new AdapterSettings();
        settings["AppDataPath"] = dataPath;
        settings["ProjectName"] = "MPower";
        settings["ApplicationName"] = "Pilot";
        settings["eb_server"] = "chist95004.amers.ibechtel.com";
        settings["eb_datasource"] = "ebInsight";
        settings["eb_username"] = "admin";
        settings["eb_password"] = "wZf4K0P6bBju40XrrV66vg==";

        ebDataLayer dataLayer = new ebDataLayer(settings);
        DataDictionary ebDictionary = dataLayer.GetDictionary();

        IList<IDataObject> objects = dataLayer.Get("T0046", null, 0, 0);
      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
      }        
      
      //try
      //{
        //eb.proxy.begin_trx();

        //int objId = eb.proxy.create_from_template(eb.GetTemplateId("T0001-SY-1"));
        //if (objId < 0)
        //{
        //  throw new Exception(eb.proxy.get_error(objId));
        //}

        ////eB.Data.Tag tag = new eB.Data.Tag(eb.session);
        ////tag.Retrieve(objId, "Header;Attributes");
        ////tag.Code = "tag001";
        ////tag.Save();

        ////dl.session.create

        //string tag = "p-002";
        //int ret;

        ////if (eb.GetTagId("SY-1", "001", tag, "N/A") > 0)
        //{
        //  ret = eb.proxy.chg_tag(objId, -1, tag, "%", -1, "%", "%", "%", "%", -1, -1);
        //  if (ret < 0)
        //  {
        //    throw new Exception(eb.proxy.get_error(ret));
        //  }
        //}
        ////else {
        //  //int itemId = eb.GetItemId("SY-1", "001");
        //  //ret = eb.proxy.add_tag(itemId, tag, "001", 

        //  //if (ret < 0)
        //  //{
        //  //  throw new Exception(eb.proxy.get_error(ret));
        //  //}
        ////}

        //int charId = eb.GetCharId("Seismic Rating");
        //ret = eb.proxy.chg_char_data(objId, charId, "A");

        //int docId = eb.GetDocId("11111-001-M6-AF-00004", null);
        //int rdlId = eb.proxy.create_from_template(eb.GetTemplateId("ARE002A"), docId, (int)eB.Common.Enum.ObjectType.Document, objId, (int)eB.Common.Enum.ObjectType.Tag);

        //eb.proxy.commit_trx();
      //}
      //catch (Exception e)
      //{
      //  Console.WriteLine(e.ToString());
      //  eb.proxy.rollback_trx();
      //}
      //finally
      //{
      //  if (eb !=null) eb.proxy.Dispose();
      //}

      Console.WriteLine("Press any key to continue.");
      Console.ReadKey();
    }
  }
}
