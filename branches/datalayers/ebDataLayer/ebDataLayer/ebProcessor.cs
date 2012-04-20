using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eB.Data;
using org.iringtools.library;
using org.iringtools.adaper.datalayer.eb.config;
using System.Data;
using eB.Common.Enum;

namespace org.iringtools.adaper.datalayer.eb
{
  public class ebProcessor
  {
    private Session _session;
    private List<Map> _mappings;
    private Rules _rules;
    private DataObject _objectDefinition;
    private IDataObject _dataObject;

    public ebProcessor(Session session, List<Map> mappings, Rules rules)
    {
      _session = session;
      _mappings = mappings;
      _rules = rules;
    }

    public Status ProcessTag(DataObject objectDefinition, IDataObject dataObject, int id, string key)
    {
      _objectDefinition = objectDefinition;
      _dataObject = dataObject;

      Status status = new Status() { Level = StatusLevel.Success };

      try
      {
        Tag tag = new Tag(_session, id);

        tag.Retrieve("Header;Attributes");
        tag.Code = key;

        if (!SetName(ref tag, key))
        {
          status.Messages.Add("Failed to set Name\n");
        }

        if (!SetTitle(ref tag, key))
        {
          status.Messages.Add("Failed to set Title\n");
        }

        Append(ref status, SetAttributes(ref tag));
        Append(ref status, SetRelationships(tag));

        tag.Save();
      }
      catch (Exception e)
      {
        status.Level = StatusLevel.Error;
        status.Messages.Add(e.ToString());
      }

      return status;
    }

    public Status ProcessDocument(DataObject objectDefinition, IDataObject dataObject, int id, string key)
    {
      _objectDefinition = objectDefinition;
      _dataObject = dataObject;

      Status status = new Status() { Level = StatusLevel.Success };

      try
      {
        Document doc = new Document(_session, id);
        doc.Retrieve("Header;Attributes");
        
        Append(ref status, SetAttributes(ref doc));
        Append(ref status, SetRelationships(doc));
      }
      catch (Exception e)
      {
        status.Level = StatusLevel.Error;
        status.Messages.Add(e.ToString());
      }

      return status;
    }

    protected bool SetName(ref Tag tag, string key)
    {
      try
      {
        Map map = (from m in _mappings
                   where m.Type == (int)Destination.Name
                   select m).FirstOrDefault();

        if (map != null)
        {
          string name = (string)_dataObject.GetPropertyValue(map.Property);

          if (tag.Name != name)
          {
            tag.Name = name;
          }
        }
        else
        {
          tag.Name = key;
        }
      }
      catch
      {
        return false;
      }

      return true;
    }

    protected bool SetName(ref Document doc, string key)
    {
      try
      {
        Map map = (from m in _mappings
                   where m.Type == (int)Destination.Name
                   select m).FirstOrDefault();

        if (map != null)
        {
          string name = (string)_dataObject.GetPropertyValue(map.Property);

          if (doc.Name != name)
          {
            doc.Name = name;
          }
        }
        else
        {
          doc.Name = key;
        }
      }
      catch
      {
        return false;
      }

      return true;
    }

    protected bool SetTitle(ref Tag tag, string key)
    {
      try
      {
        Map map = (from m in _mappings
                   where m.Type == (int)Destination.Title
                   select m).FirstOrDefault();

        if (map != null)
        {
          string name = (string)_dataObject.GetPropertyValue(map.Property);

          if (tag.Name != name)
          {
            tag.Description = name;
          }
        }
        else
        {
          tag.Description = key;
        }
      }
      catch
      {
        return false;
      }

      return true;
    }

    protected Status SetAttributes(ref Tag tag)
    {
      Status status = new Status() { Level = StatusLevel.Success };

      try
      {
        var attrMaps = from m in _mappings
                    where m.Type == (int)Destination.Attribute
                    select m;

        foreach (Map map in attrMaps)
        {
          try
          {
            object value = _dataObject.GetPropertyValue(map.Property);

            if (value != null)
            {
              DataProperty prop = _objectDefinition.dataProperties.Find(x => x.propertyName.ToLower() == map.Property.ToLower());
              eB.Data.Attribute ebAttr = tag.Attributes.Where(attr => ((attr.Name == prop.columnName))).Select(attr => attr).FirstOrDefault();

              if ((ebAttr.Value == null) || (ebAttr.Value.ToString() != value.ToString()))
              {
                _session.Writer.ChgCharData(tag.Id, ebAttr.AttributeDef.Id, value);
              }
            }
          }
          catch (Exception e)
          {
            status.Messages.Add(string.Format("Attribute {0} update failed due to error {1}", map.Property, e.Message));
            status.Level = StatusLevel.Warning;
          }
        }
      }
      catch (Exception e) 
      {
        status.Messages.Add(e.Message);
        status.Level = StatusLevel.Error;
      }

      return status;
    }

    protected Status SetAttributes(ref Document doc)
    {
      Status status = new Status() { Level = StatusLevel.Success };

      try
      {
        var attrMaps = from m in _mappings
                    where m.Type == (int)Destination.Attribute
                    select m;

        foreach (Map map in attrMaps)
        {
          try
          {
            object value = _dataObject.GetPropertyValue(map.Property);

            if (value != null)
            {
              DataProperty prop = _objectDefinition.dataProperties.Find(x => x.propertyName.ToLower() == map.Property.ToLower());
              eB.Data.Attribute ebAttr = doc.Attributes.Where(attr => ((attr.Name == prop.columnName))).Select(attr => attr).FirstOrDefault();

              if (ebAttr.Value.ToString() != (string)value)
              {
                _session.Writer.ChgCharData(doc.Id, ebAttr.AttributeDef.Id, value);
              }
            }
          }
          catch (Exception e)
          {
            status.Messages.Add(string.Format("Attribute {0} update failed due to error {1}", map.Property, e.Message));
            status.Level = StatusLevel.Warning;
          }
        }
      }
      catch (Exception e) 
      { 
        status.Messages.Add(e.Message);
        status.Level = StatusLevel.Error;
      }

      return status;
    }

    protected Status SetRelationships(Tag tag)
    {
      Status status = new Status() { Level = StatusLevel.Success };
      EqlClient eqlClient = new EqlClient(_session);

      try
      {
        foreach (Map m in _mappings.Where(m => m.RuleRefs.Count > 0).Select(m => m))
        {
          List<Rule> rules = GetRules(m.RuleRefs);

          foreach (Rule rule in rules)
          {
            if (RequiredParametersPresent(rule))
            {
              if (SelfchecksPassed(rule))
              {
                int reltemplateId = eqlClient.GetTemplateId(rule.RelationshipTemplate);

                if (reltemplateId > 0)
                {
                  DataTable relatedObjects = eqlClient.GetObjectIds(ParseEQL(rule));

                  foreach (DataRow row in relatedObjects.Rows)
                  {
                    int relatedObjectId = int.Parse(row[0].ToString());

                    if (relatedObjectId > 0)
                    {
                      try
                      {
                        bool add = false;
                        int existingrelId = eqlClient.GetExistingRelationship(reltemplateId, tag.Id, relatedObjectId, ref add);

                        if (existingrelId > 0)
                        {
                          _session.Writer.DelRelationship(existingrelId);
                        }

                        if (add)
                        {
                          _session.Writer.CreateRelFromTemplate(reltemplateId, tag.Id, (int)ObjectType.Tag, relatedObjectId, rule.RelatedObjectType);
                        }
                      }
                      catch (Exception e)
                      {
                        status.Messages.Add(string.Format("Relationship update failed due to error {0}.", e.Message));
                        status.Level = StatusLevel.Warning;
                      }
                    }
                  }
                }
                else
                {
                  status.Messages.Add(string.Format("Relationship template {0} is not defined.", rule.RelationshipTemplate));
                  status.Level = StatusLevel.Warning;
                }
              }
            }
          }
        }
      }
      catch (Exception e) 
      {         
        status.Messages.Add(e.Message);
        status.Level = StatusLevel.Error;
      }

      return status;
    }

    protected Status SetRelationships(Document doc)
    {
      Status status = new Status() { Level = StatusLevel.Success };
      EqlClient eqlClient = new EqlClient(_session);

      try
      {
        foreach (Map map in _mappings.Where(m => m.RuleRefs.Count > 0).Select(m => m))
        {
          List<Rule> rules = GetRules(map.RuleRefs);

          foreach (Rule rule in rules)
          {
            if (RequiredParametersPresent(rule))
            {
              if (SelfchecksPassed(rule))
              {
                int reltemplateId = eqlClient.GetTemplateId(rule.RelationshipTemplate);
                if (reltemplateId > 0)
                {
                  DataTable relatedObjects = eqlClient.GetObjectIds(ParseEQL(rule));
                  foreach (DataRow dr in relatedObjects.Rows)
                  {
                    int relatedObjectId = int.Parse(dr[0].ToString());
                    if (relatedObjectId > 0)
                    {
                      try
                      {
                        bool add = false;
                        int existingrelId = eqlClient.GetExistingRelationship(reltemplateId, doc.Id, relatedObjectId, ref add);
                        if (existingrelId > 0)
                        {
                          _session.Writer.DelRelationship(existingrelId);
                        }
                        if (add)
                        {
                          _session.Writer.CreateRelFromTemplate(reltemplateId, doc.Id, (int)ObjectType.Document, relatedObjectId, (int)rule.RelatedObjectType);
                        }
                      }
                      catch (Exception e)
                      {
                        status.Messages.Add(string.Format("Relationship update failed due to error {0}.", e.Message));
                        status.Level = StatusLevel.Warning;
                      }
                    }
                  }
                }
                else
                {
                  status.Messages.Add(string.Format("Relationship template {0} is not defined.", rule.RelationshipTemplate));
                  status.Level = StatusLevel.Warning;
                }
              }
            }
          }
        }
      }
      catch (Exception e) 
      { 
        status.Messages.Add(e.Message);
        status.Level = StatusLevel.Error;
      }

      return status;
    }

    private bool RequiredParametersPresent(Rule rule)
    {
      foreach (var p in rule.Parameters)
      {
        if (_dataObject.GetPropertyValue(p.Value) == null)
        {
          return false;
        }
      }

      return true;
    }

    private Rules GetRules(List<RuleRef> ruleRefs)
    {
      Rules rules = new Rules();

      ruleRefs.Sort(new RuleRefComparer());

      foreach (RuleRef ruleRef in ruleRefs)
      {
        Rule rule = _rules.Find(x => x.Id == ruleRef.Value);
        if (rule != null) rules.Add(rule);
      }

      return rules;
    }

    private string GetValue(Parameter param, string value)
    {
      try
      {
        return value.Split(new string[] { param.Seperator }, StringSplitOptions.RemoveEmptyEntries)[param.Position];
      }
      catch 
      { 
        return value; 
      }
    }

    private string ParseEQL(Rule rule)
    {
      if ((rule.Parameters == null) || (rule.Parameters.Count() == 0))
      {
        return rule.Eql;
      }

      string[] parameters = new string[rule.Parameters.Count];
      for (int i = 0; i < rule.Parameters.Count; i++)
      {
        parameters[i] = GetValue(rule.Parameters[i], (string)_dataObject.GetPropertyValue(rule.Parameters[i].Value));
      }

      return string.Format(rule.Eql, parameters);
    }

    private bool SelfchecksPassed(Rule rule)
    {
      foreach (var check in rule.SelfChecks)
      {
        if (!Passed(check))
        {
          return false;
        }
      }

      return true;
    }

    private bool Passed(SelfCheck check)
    {
      try
      {
        string value = (string)_dataObject.GetPropertyValue(check.Column);

        switch (check.Operator.ToLower())
        {
          case "=": return value == check.Value;
          case "<>": return value != check.Value;
          case "contains": return value.Contains(check.Value);
          case "startswith": return value.StartsWith(check.Value);
          case "endswith": return value.EndsWith(check.Value);
          default: return false;
        }
      }
      catch 
      { 
        return false; 
      }
    }

    private void Append(ref Status status, Status newStatus)
    {
      if (status.Level < newStatus.Level)
      {
        status.Level = newStatus.Level;
      }

      status.Messages.AddRange(newStatus.Messages);
    }
  }
}
