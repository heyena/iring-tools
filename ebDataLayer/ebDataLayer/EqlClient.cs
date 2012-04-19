using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eB.Data;
using System.Data;
using System.Xml.Linq;
using log4net;

namespace org.iringtools.adaper.datalayer.eb
{
  public class EqlClient
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(EqlClient));
    private Session _session;

    public EqlClient(Session session)
    {
      _session = session;
    }

    public string GetDocumentTemplate(int docId)
    {
      String templateName = string.Empty;

      try
      {
        string eql = String.Format("START WITH Template SELECT Name WHERE Instances.Object.Id = {0} AND Instances.Object.Type = 3", docId);
        eB.Data.Search s = new Search(_session, eql);
        templateName = s.RetrieveScalar<String>("Name");
      }
      catch (Exception e) 
      { 
        _logger.Error(e); 
        throw e; 
      }

      return templateName;
    }

    public int GetTemplateId(string templateName)
    {
      int templateId = 0;

      try
      {
        string eql = String.Format("START WITH Template SELECT Id WHERE Name = '{0}'", templateName);
        eB.Data.Search s = new Search(this._session, eql);
        templateId = s.RetrieveScalar<int>("Id");
      }
      catch (Exception e) 
      { 
        _logger.Error(e); 
        throw e; 
      }

      return templateId;
    }

    public int GetObjectId(string code, string revision, int type)
    {
      int objectId = 0;

      try
      {
        string eql = string.Empty;

        if (String.IsNullOrEmpty(revision))
        {
          eql = String.Format("START WITH Object SELECT Id WHERE Code = '{0}' AND Type = {1}", code, type);
        }
        else
        {
          eql = String.Format("START WITH Object SELECT Id WHERE Code = '{0}' AND Type = {1} AND Revision = '{2}'", code, type, revision);
        }

        eB.Data.Search s = new Search(this._session, eql);
        objectId = s.RetrieveScalar<int>("Id");
      }
      catch (Exception e) 
      { 
        _logger.Error(e); 
        throw e; 
      }

      return objectId;
    }

    public int GetObjectId(string eql)
    {
      int objectId = 0;

      try
      {
        eB.Data.Search s = new Search(this._session, eql);
        objectId = s.RetrieveScalar<int>("Id");
      }
      catch (Exception e) 
      { 
        _logger.Error(e); 
        throw e; 
      }

      return objectId;
    }

    public DataTable GetObjectIds(string eql)
    {
      DataTable dt = new DataTable();

      try
      {
        eB.Data.Search s = new Search(this._session, eql);
        dt = s.Retrieve<DataTable>();
      }
      catch (Exception e)
      {
        _logger.Error(e);
        throw e;
      }

      return dt;
    }

    public int GetExistingRelationship(int relationshipTemplateId, int leftObjectId, int rightObjectId, ref bool add)
    {
      try
      {
        string sql = String.Format(@"select s.rel_id,r.num_right,s.right_object_id from templates t 
                            inner join relationship_types r on t.class_id = r.class_id 
                            inner join relationships s on r.rel_type_id  = s.rel_type_id 
                            where t.template_id = {0} and s.left_object_id = {1}", relationshipTemplateId, leftObjectId);

        XDocument dataset = XDocument.Parse(this._session.ProtoProxy.Query(this._session.ReaderSessionString, sql));
        int rows = dataset.Element("records").Elements("record").Count();

        if (rows == 1)
        {
          if (int.Parse(dataset.Element("records").Element("record").Element("num_right").Value) == 1)
          {
            if (int.Parse(dataset.Element("records").Element("record").Element("right_object_id").Value) == rightObjectId)
            {
              add = false;
              return 0;
            }
            else
            {
              add = true;
              return int.Parse(dataset.Element("records").Element("record").Element("rel_id").Value);
            }
          }
          else
          {
            if (int.Parse(dataset.Element("records").Element("record").Element("right_object_id").Value) == rightObjectId)
            {
              add = false;
              return 0;
            }
            else
            {
              add = true;
              return 0;
            }
          }
        }
        else
        {
          XElement element = dataset.Element("records").Elements("record").Where(s => s.Element("right_object_id").Value == rightObjectId.ToString()).Select(s => s).FirstOrDefault();
          if (element != null)
          {
            add = false;
            return 0;
          }
          else
          {
            add = true;
            return 0;
          }
        }

      }
      catch (Exception e) 
      { 
        _logger.Error(e); 
        throw e; 
      }
    }
  }
}
