using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.adapter.datalayer;
using System.Reflection;

namespace ebTest
{
  class Program
  {
    static void Main(string[] args)
    {
      try
      {
        string baseDir = typeof(ebDataLayer).Assembly.Location.Replace(@"ebTest\bin\Debug\ebLibrary.dll", "");

        AdapterSettings settings = new AdapterSettings();
        settings["AppDataPath"] = baseDir + @"ebLibrary\App_Data\";
        settings["ProjectName"] = "MPower";
        settings["ApplicationName"] = "Pilot";

        // MPower.Pilot settings
        settings["ebServer"] = "chist95004.amers.ibechtel.com";
        settings["ebDataSource"] = "ebInsight";
        settings["ebUserName"] = "admin";
        settings["ebPassword"] = "wZf4K0P6bBju40XrrV66vg==";
        settings["ebClassCodes"] = "ELECT, AE, MECH, LINE, VALVE";
        settings["ebMetadataQuery.1"] = @"
            select d.char_name, d.char_data_type, d.char_length, 0 as readonly from class_objects a 
            inner join class_attributes c on c.class_id = a.class_id
            inner join characteristics d on c.char_id = d.char_id
            where a.code = '{0}'
            union select 'Class.Code', 'String', 255, 1
            union select 'Id', 'Int32', 4, 1
            union select 'Code', 'String', 100, 1
            union select 'Middle', 'String', 100, 1
            union select 'Revision', 'String', 100, 1
            union select 'DateEffective', 'DateTime', 0, 1
            union select 'Name', 'String', 255, 1
            union select 'ChangeControlled', 'String', 1, 1
            union select 'ApprovalStatus', 'String', 1, 1
            union select 'Remark', 'String', 255, 1
            union select 'Synopsis', 'String', 255, 1
            union select 'DateObsolete', 'DateTime', 0, 1
            union select 'Class.Id', 'Int32', 8, 1";
        settings["ebMetadataQuery.17"] = @"
            select d.char_name, d.char_data_type, d.char_length, 0 as readonly from class_objects a 
            inner join class_attributes c on c.class_id = a.class_id
            inner join characteristics d on c.char_id = d.char_id
            where a.code = '{0}'
            union select 'Class.Code', 'String', 255, 1
            union select 'Id', 'Int32', 4, 1
            union select 'Class.Id', 'Int32', 4, 1
            union select 'PrimaryPhysicalItem.Id', 'Int32', 4, 1
            union select 'Code', 'String', 100, 1
            union select 'Revision', 'String', 100, 1
            union select 'Name', 'String', 255, 1
            union select 'Description', 'String', 4000, 1
            union select 'ApprovalStatus', 'String', 1, 1
            union select 'ChangeControlled', 'String', 1, 1
            union select 'OperationalStatus', 'String', 1, 1
            union select 'Quantity', 'Int32', 8, 1";

        ebDataLayer dataLayer = new ebDataLayer(settings);
        DataDictionary ebDictionary = dataLayer.GetDictionary();

        //IList<IDataObject> objects = dataLayer.Get("T0046", null, 0, 0);
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
