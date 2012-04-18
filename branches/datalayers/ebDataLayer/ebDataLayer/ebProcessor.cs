using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eB.Data;
using org.iringtools.library;
using org.iringtools.adaper.datalayer.eb.config;

namespace org.iringtools.adaper.datalayer.eb
{
  public class ebProcessor
  {
    private Session _session;
    private List<Map> _mappings;

    public ebProcessor(Session session, List<Map> mappings)
    {
      _session = session;
      _mappings = mappings;
    }

    public Status ProcessDocument(int id, string key)
    {
      Status status = new Status();

      try
      {
        Document t = new Document(_session, id);
        t.Retrieve("Header;Attributes");

        status.Messages.Add(SetRelationships(t));
        status.Level = StatusLevel.Success;
      }
      catch (Exception e)
      {
        status.Level = StatusLevel.Error;
        status.Messages.Add(e.ToString());
      }

      return status;
    }

    public Status ProcessTag(int id, string key)
    {
      Status status = new Status();

      try
      {
        StringBuilder messages = new StringBuilder();
        Tag t = new Tag(_session, id);

        t.Retrieve("Header;Attributes");
        t.Code = key;

        if (!SetName(ref t, key))
        {
          messages.Append("Failed to set Name\n");
        }

        if (!SetTitle(ref t, key))
        {
          messages.Append("Failed to set Title\n");
        }

        messages.Append(SetAttributes(ref t));
        SetRelationships(t);
        t.Save();
      }
      catch (Exception e)
      {
        status.Level = StatusLevel.Error;
        status.Messages.Add(e.ToString());
      }

      return status;
    }

    protected string SetAttributes(ref Tag o)
    {
      StringBuilder messages = new StringBuilder();
      try
      {
        var attrs = from k in _mappings
                    where k.Type == org.iringtools.adaper.datalayer.eb.config.PropertyType.Attribute
                    select k;
        foreach (Map m in attrs)
        {
          try
          {
            //if (!String.IsNullOrEmpty(DataSet[m.ColumnName]))
            //{
            //  eB.Data.Attribute ebAtr = o.Attributes.Where(atr => ((atr.Name == m.ColumnName))).Select(atr => atr).FirstOrDefault();

            //  if ((ebAtr.Value == null) || (ebAtr.Value.ToString() != DataSet[m.ColumnName]))
            //  {
            //    this.Session.Writer.ChgCharData(o.Id, ebAtr.AttributeDef.Id, DataSet[m.ColumnName]);
            //  }
            //}
          }
          catch (Exception ex)
          {
            messages.Append(String.Format("Attribute {0} update failed due to error {1}", m.Name, ex.Message));
          }
        }
      }
      catch (Exception ex) { return ex.Message; }
      return messages.ToString();
    }

    protected string SetAttributes(ref Document o)
    {
      StringBuilder messages = new StringBuilder();
      try
      {
        var attrs = from k in _mappings
                    where k.Type == org.iringtools.adaper.datalayer.eb.config.PropertyType.Attribute
                    select k;
        foreach (Map m in attrs)
        {
          try
          {
            //if (!String.IsNullOrEmpty(DataSet[m.ColumnName]))
            //{
            //  eB.Data.Attribute ebAtr = o.Attributes.Where(atr => ((atr.Name == m.ColumnName))).Select(atr => atr).FirstOrDefault();
            //  if (ebAtr.Value.ToString() != DataSet[m.ColumnName])
            //  {
            //    this.Session.Writer.ChgCharData(o.Id, ebAtr.AttributeDef.Id, DataSet[m.ColumnName]);
            //  }
            //}
          }
          catch (Exception ex)
          {
            messages.Append(String.Format("Attribute {0} update failed due to error {1}", m.Name, ex.Message));
          }
        }
      }
      catch (Exception ex) { return ex.Message; }
      return messages.ToString();
    }

    protected string SetRelationships(Tag o)
    {
      StringBuilder messages = new StringBuilder();
      try
      {
        foreach (Map m in _mappings.Where(m => m.Rules.Count > 0).Select(m => m))
        {
          //foreach (Rule r in m.Rules)
          //{
          //  if (r.RequiredParametersPresent(this.DataSet))
          //  {
          //    if (r.SelfchecksPassed(this.DataSet))
          //    {
          //      int reltemplateId = data.GetTemplateId(r.RelationshipTemplate);
          //      if (reltemplateId > 0)
          //      {
          //        DataTable relatedObjects = data.GetObjectIds(r.ParseEQL(this.DataSet));
          //        foreach (DataRow dr in relatedObjects.Rows)
          //        {
          //          int relatedObjectId = int.Parse(dr[0].ToString());
          //          if (relatedObjectId > 0)
          //          {
          //            try
          //            {
          //              bool add = false;
          //              int existingrelId = data.GetExistingRelationship(reltemplateId, o.Id, relatedObjectId, ref add);
          //              if (existingrelId > 0)
          //              {
          //                Session.Writer.DelRelationship(existingrelId);
          //              }
          //              if (add)
          //              {
          //                Session.Writer.CreateRelFromTemplate(reltemplateId, o.Id, (int)ObjectType.Tag, relatedObjectId, (int)r.RelatedObjectType);
          //              }
          //            }
          //            catch (Exception ex)
          //            {
          //              messages.Append(String.Format("Relationship failed due to error {0}.", ex.Message));
          //            }
          //          }
          //        }
          //      }
          //      else
          //      {
          //        messages.Append(String.Format("Relationship template {0} is not defined.", r.RelationshipTemplate));
          //      }
          //    }
          //  }
          //}
        }


      }
      catch (Exception ex) { return ex.Message; }
      return messages.ToString();
    }

    protected string SetRelationships(Document o)
    {
      StringBuilder messages = new StringBuilder();
      try
      {
        foreach (Map m in _mappings.Where(m => m.Rules.Count > 0).Select(m => m))
        {
          //foreach (Rule r in m.Rules)
          //{
          //  if (r.RequiredParametersPresent(this.DataSet))
          //  {
          //    if (r.SelfchecksPassed(this.DataSet))
          //    {
          //      int reltemplateId = data.GetTemplateId(r.RelationshipTemplate);
          //      if (reltemplateId > 0)
          //      {
          //        DataTable relatedObjects = data.GetObjectIds(r.ParseEQL(this.DataSet));
          //        foreach (DataRow dr in relatedObjects.Rows)
          //        {
          //          int relatedObjectId = int.Parse(dr[0].ToString());
          //          if (relatedObjectId > 0)
          //          {
          //            try
          //            {
          //              bool add = false;
          //              int existingrelId = data.GetExistingRelationship(reltemplateId, o.Id, relatedObjectId, ref add);
          //              if (existingrelId > 0)
          //              {
          //                Session.Writer.DelRelationship(existingrelId);
          //              }
          //              if (add)
          //              {
          //                Session.Writer.CreateRelFromTemplate(reltemplateId, o.Id, (int)ObjectType.Document, relatedObjectId, (int)r.RelatedObjectType);
          //              }
          //            }
          //            catch (Exception ex)
          //            {
          //              messages.Append(String.Format("Relationship failed due to error {0}.", ex.Message));
          //            }
          //          }
          //        }
          //      }
          //      else
          //      {
          //        messages.Append(String.Format("Relationship template {0} is not defined.", r.RelationshipTemplate));
          //      }
          //    }
          //  }
          //}
        }


      }
      catch (Exception ex) { return ex.Message; }
      return messages.ToString();
    }

    protected bool SetName(ref Tag o, string key)
    {
      try
      {
        Map n = (from k in _mappings
                     where k.Type == org.iringtools.adaper.datalayer.eb.config.PropertyType.Name
                     select k).FirstOrDefault();
        //if (n != null)
        //{
        //  string name = DataSet[n.ColumnName];
        //  if (o.Name != name)
        //  {
        //    o.Name = name;
        //  }
        //}
        //else
        //{
        //  o.Name = key;
        //}
      }
      catch { return false; }
      return true;
    }

    protected bool SetName(ref Document o, string key)
    {
      try
      {
        Map n = (from k in _mappings
                     where k.Type == org.iringtools.adaper.datalayer.eb.config.PropertyType.Name
                     select k).FirstOrDefault();
        //if (n != null)
        //{
        //  string name = DataSet[n.ColumnName];
        //  if (o.Name != name)
        //  {
        //    o.Name = name;
        //  }
        //}
        //else
        //{
        //  o.Name = key;
        //}
      }
      catch { return false; }
      return true;
    }

    protected bool SetTitle(ref Tag o, string key)
    {
      try
      {
        Map n = (from k in _mappings
                     where k.Type == org.iringtools.adaper.datalayer.eb.config.PropertyType.Title
                     select k).FirstOrDefault();
        //if (n != null)
        //{
        //  string name = DataSet[n.ColumnName];
        //  if (o.Name != name)
        //  {
        //    o.Description = name;
        //  }
        //}
        //else
        //{
        //  o.Description = key;
        //}
      }
      catch { return false; }
      return true;
    }
  }
}
