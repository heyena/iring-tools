// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Text;

namespace org.ids_adi.qmxf
{
  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "qmxf")]
  [XmlRoot(Namespace = "http://ns.ids-adi.org/qxf/model#", ElementName = "qmxf")]
  public class QMXF
  {
    public QMXF()
    {
      this.ClassDefinitions = new List<ClassDefinition>();
      this.TemplateDefinitions = new List<TemplateDefinition>();
      this.TemplateQualifications = new List<TemplateQualification>();
      this.Licenses = new List<License>();
    }

    [DataMember(Name = "class-Definitions", EmitDefaultValue = false, Order = 0)]
    [XmlElement(ElementName = "class-Definitions")]
    public List<ClassDefinition> ClassDefinitions { get; set; }


    [DataMember(Name = "licenses", EmitDefaultValue = false, Order = 1)]
    [XmlElement(ElementName = "licenses")]
    public List<License> Licenses { get; set; }

    [DataMember(Name = "license-Ref", EmitDefaultValue = false, Order = 2)]
    [XmlAttribute(AttributeName = "license-Ref")]
    public string LicenseRef { get; set; }

    [DataMember(Name = "sourceRepository", EmitDefaultValue = false, Order = 3)]
    [XmlAttribute(AttributeName = "sourceRepository")]
    public string SourceRepository { get; set; }   

    [DataMember(Name = "targetRepository", EmitDefaultValue = false, Order = 4)]
    [XmlAttribute(AttributeName = "targetRepository")]
    public string TargetRepository { get; set; }

    [DataMember(Name = "template-Definitions", EmitDefaultValue = false, Order = 5)]
    [XmlElement(ElementName = "template-Definitions")]
    public List<TemplateDefinition> TemplateDefinitions { get; set; }

    [DataMember(Name = "template-Qualifications", EmitDefaultValue = false, Order = 6)]
    [XmlElement(ElementName = "template-Qualifications")]
    public List<TemplateQualification> TemplateQualifications { get; set; }

    [DataMember(Name = "timestamp", EmitDefaultValue = false, Order = 7)]
    [XmlAttribute(AttributeName = "timestamp")]
    public string Timestamp { get; set; }

  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "template-definition")]
  public class TemplateDefinition
  {
    public TemplateDefinition()
    {
      this.Names = new List<QMXFName>();
      this.Descriptions = new List<Description>();
      this.SuggestedDesignations = new List<SuggestedDesignation>();
      this.Statuses = new List<QMXFStatus>();
      this.TextualDefinitions = new List<TextualDefinition>();
      this.RoleDefinitions = new List<RoleDefinition>();
      this.RepositoryName = string.Empty;
      this.specialization = new List<Specialization>();
    }

    
    [DataMember(Name = "descriptions", EmitDefaultValue = false)]
    [XmlElement(ElementName = "descriptions")]
    public List<Description> Descriptions { get; set; }

    [DataMember(Name = "designations", EmitDefaultValue = false)]
    [XmlElement(ElementName = "designations")]
    public Designation Designation { get; set; }

    [DataMember(Name = "id", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "id")]
    public string Identifier { get; set; }

    [DataMember(Name = "names", EmitDefaultValue = false)]
    [XmlElement(ElementName = "namse")]
    public List<QMXFName> Names { get; set; }

    [DataMember(Name = "repository", EmitDefaultValue = false)]
    [XmlElement(ElementName = "repository")]
    public string RepositoryName { get; set; }

    [DataMember(Name = "roleDefinitions", EmitDefaultValue = false)]
    [XmlElement(ElementName = "roleDefinitions")]
    public List<RoleDefinition> RoleDefinitions { get; set; }

    [DataMember(Name = "specializations", EmitDefaultValue = false)]
    [XmlElement(ElementName = "specializations")]
    public List<Specialization> specialization { get; set; }


    [DataMember(Name = "statuses", EmitDefaultValue = false)]
    [XmlElement(ElementName = "statuses")]
    public List<QMXFStatus> Statuses { get; set; }

    [DataMember(Name = "suggestedDesignations", EmitDefaultValue = false)]
    [XmlElement(ElementName = "suggestedDesignations")]
    public List<SuggestedDesignation> SuggestedDesignations { get; set; }

    [DataMember(Name = "textualDefinitions", EmitDefaultValue = false)]
    [XmlElement(ElementName = "textualDefinitions")]
    public List<TextualDefinition> TextualDefinitions { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "classDefinition")]
  public class ClassDefinition
  {
    public ClassDefinition()
    {
      this.Names = new List<QMXFName>();
      this.Descriptions = new List<Description>();
      this.Classifications = new List<Classification>();
      this.Specializations = new List<Specialization>();
      this.Statuses = new List<QMXFStatus>();
      this.SuggestedDesignations = new List<SuggestedDesignation>();
      this.TextualDefinitions = new List<TextualDefinition>();
      this.RepositoryName = string.Empty;
    }

    [DataMember(Name = "classifications", EmitDefaultValue = false, Order = 0)]
    [XmlElement(ElementName = "classifications")]
    public List<Classification> Classifications { get; set; }

    [DataMember(Name = "descriptions", EmitDefaultValue = false, Order = 1)]
    [XmlElement(ElementName = "descriptions")]
    public List<Description> Descriptions { get; set; }


    [DataMember(Name = "designations", EmitDefaultValue = false, Order = 2)]
    [XmlElement(ElementName = "designations")]
    public Designation Designation { get; set; }

    [DataMember(Name = "entityType", EmitDefaultValue = false, Order = 3)]
    [XmlElement(ElementName = "entityType")]
    public EntityType EntityType { get; set; }

    [DataMember(Name = "id", EmitDefaultValue = false, Order = 4)]
    [XmlAttribute(AttributeName = "id")]
    public string Identifier { get; set; }

    [DataMember(Name = "names", EmitDefaultValue = false, Order = 5)]
    [XmlElement(ElementName = "names")]
    public List<QMXFName> Names { get; set; }

    [DataMember(Name = "repository", EmitDefaultValue = false, Order = 6)]
    [XmlElement(ElementName = "repository")]
    public string RepositoryName { get; set; }

    [DataMember(Name = "specializations", EmitDefaultValue = false, Order = 7)]
    [XmlElement(ElementName = "specializations")]
    public List<Specialization> Specializations { get; set; }

    [DataMember(Name = "statuses", EmitDefaultValue = false, Order = 8)]
    [XmlElement(ElementName = "statuses")]
    public List<QMXFStatus> Statuses { get; set; }

    [DataMember(Name = "suggestedDesignations", EmitDefaultValue = false, Order = 9)]
    [XmlElement(ElementName = "suggestedDesignations")]
    public List<SuggestedDesignation> SuggestedDesignations { get; set; }

    [DataMember(Name = "textualDefinitions", EmitDefaultValue = false, Order = 10)]
    [XmlElement(ElementName = "textualDefinitions")]
    public List<TextualDefinition> TextualDefinitions { get; set; }

  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "template-qualification")]
  public class TemplateQualification
  {
    public TemplateQualification()
    {
      this.Names = new List<QMXFName>();
      this.Descriptions = new List<Description>();
      this.SuggestedDesignations = new List<SuggestedDesignation>();
      this.Designations = new List<Designation>();
      this.Statuses = new List<QMXFStatus>();
      this.TextualDefinitions = new List<TextualDefinition>();
      this.RoleQualifications = new List<RoleQualification>();
      this.Specializations = new List<Specialization>();
      this.RepositoryName = string.Empty;
    }

    [DataMember(Name = "repository", EmitDefaultValue = false)]
    [XmlElement(ElementName = "repository")]
    public string RepositoryName { get; set; }

    [DataMember(Name = "suggested-designation", EmitDefaultValue = false)]
    [XmlElement(ElementName = "suggested-designation")]
    public List<SuggestedDesignation> SuggestedDesignations { get; set; }

    [DataMember(Name = "specialization", EmitDefaultValue = false)]
    [XmlElement(ElementName = "specialization")]
    public List<Specialization> Specializations { get; set; }

    [DataMember(Name = "designation", EmitDefaultValue = false)]
    [XmlElement(ElementName = "designation")]
    public List<Designation> Designations { get; set; }

    [DataMember(Name = "name", EmitDefaultValue = false)]
    [XmlElement(ElementName = "name")]
    public List<QMXFName> Names { get; set; }

    [DataMember(Name = "description", EmitDefaultValue = false)]
    [XmlElement(ElementName = "description")]
    public List<Description> Descriptions { get; set; }

    [DataMember(Name = "textual-definition", EmitDefaultValue = false)]
    [XmlElement(ElementName = "textual-definition")]
    public List<TextualDefinition> TextualDefinitions { get; set; }

    [DataMember(Name = "status", EmitDefaultValue = false)]
    [XmlElement(ElementName = "status")]
    public List<QMXFStatus> Statuses { get; set; }

    [DataMember(Name = "id", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "id")]
    public string Identifier { get; set; }

    [DataMember(Name = "qualifies", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "qualifies")]
    public string Qualifies { get; set; }

    [DataMember(Name = "role-qualification", EmitDefaultValue = false)]
    [XmlElement(ElementName = "role-qualification")]
    public List<RoleQualification> RoleQualifications { get; set; }

  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "license")]
  public class License
  {
    public License()
    {
      this.LicenseTerms = new List<LicenseTerms>();
    }

    [DataMember(Name = "license-terms", EmitDefaultValue = false)]
    [XmlElement(ElementName = "license-terms")]
    public List<LicenseTerms> LicenseTerms { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "license-terms")]
  public class LicenseTerms
  {
    public LicenseTerms()
    {
      this.LicenseTexts = new List<LicenseText>();
    }

    [DataMember(Name = "license-text", EmitDefaultValue = false)]
    [XmlElement(ElementName = "license-text")]
    public List<LicenseText> LicenseTexts { get; set; }

    [DataMember(Name = "id", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "id")]
    public string Identifier { get; set; }

  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "license-text")]
  public class LicenseText
  {
    public LicenseText()
    {
      this.Formal = "true";
    }

    [DataMember(Name = "lang", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "lang")]
    public string Lang { get; set; }

    [DataMember(Name = "value", EmitDefaultValue = false)]
    [XmlText]
    public string Value { get; set; }

    [DataMember(Name = "formal", EmitDefaultValue = false)]
    [XmlAttribute]
    public string Formal { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "entity-type")]
  public class EntityType
  {
    [DataMember(Name = "reference", EmitDefaultValue = false)]
    [XmlAttribute]
    public string Reference { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "classification")]
  public class Classification
  {
      [DataMember(Name = "label", EmitDefaultValue = false, Order = 0)]
      [XmlAttribute]
      public string Label { get; set; }

      [DataMember(Name = "lang", EmitDefaultValue = false, Order = 1)]
      [XmlAttribute]
      public string Lang { get; set; }

      [DataMember(Name = "reference", EmitDefaultValue = false, Order = 2)]
      [XmlAttribute]
      public string Reference { get; set; }

      [DataMember(Name = "repository", EmitDefaultValue = false, Order = 3)]
      [XmlAttribute]
      public string RepositoryName { get; set; }


      public static IComparer<Classification> SortAscending()
      {
          return (IComparer<Classification>)new SortAscendingHelper();
      }

      private class SortAscendingHelper : IComparer<Classification>
      {
          int IComparer<Classification>.Compare(Classification cd1, Classification cd2)
          {
              return string.Compare(cd1.Label, cd2.Label);
          }
      }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "specialization")]
  public class Specialization
  {
    [DataMember(Name = "reference", EmitDefaultValue = false)]
    [XmlAttribute]
    public string Reference { get; set; }

    [DataMember(Name = "rdsuri", EmitDefaultValue = false)]
    [XmlAttribute]
    public string RdsUri { get; set; }

    [DataMember(Name = "label", EmitDefaultValue = false)]
    [XmlAttribute]
    public string Label { get; set; }

    [DataMember(Name = "repository", EmitDefaultValue = false)]
    [XmlAttribute]
    public string RepositoryName { get; set; }

    [DataMember(Name = "lang", EmitDefaultValue = false)]
    [XmlAttribute]
    public string Lang { get; set; }

    public static IComparer<Specialization> SortAscending()
    {
      return (IComparer<Specialization>)new SortAscendingHelper();
    }

    private class SortAscendingHelper : IComparer<Specialization>
    {
      int IComparer<Specialization>.Compare(Specialization s1, Specialization s2)
      {
        return string.Compare(s1.Label, s2.Label);
      }
    }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "suggested-designation")]
  public class SuggestedDesignation
  {
    [DataMember(Name = "value", EmitDefaultValue = false)]
    [XmlText]
    public string Value { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "name")]
  public class QMXFName
  {
    //TODO: Ignore for now
    [DataMember(Name = "lang", EmitDefaultValue = false, Order = 0)]
    [XmlAttribute]
    public string Lang { get; set; }

    [DataMember(Name = "value", EmitDefaultValue = false, Order = 1)]
    [XmlText]
    public string Value { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "designation")]
  public class Designation
  {
    [DataMember(Name = "value", EmitDefaultValue = false)]
    [XmlText]
    public string Value { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "description")]
  public class Description
  {

    [DataMember(Name = "contentType", EmitDefaultValue = false, Order = 0)]
    [XmlAttribute]
    public string ContentType { get; set; }

    [DataMember(Name = "lang", EmitDefaultValue = false, Order = 1)]
    [XmlAttribute]
    public string Lang { get; set; }

    [DataMember(Name = "parseType", EmitDefaultValue = false, Order = 2)]
    [XmlAttribute]
    public string ParseType { get; set; }

    [DataMember(Name = "value", EmitDefaultValue = false, Order = 3)]
    [XmlText]
    public string Value { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "textual-definition")]
  public class TextualDefinition
  {
    public TextualDefinition()
    {
      this.Lang = "en-GB";
    }

    [DataMember(Name = "lang", EmitDefaultValue = false)]
    [XmlAttribute]
    public string Lang { get; set; }

    [DataMember(Name = "value", EmitDefaultValue = false)]
    [XmlText]
    public string Value { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "rule-set")]
  public class RuleSet
  {
    [DataMember(Name = "contentType", EmitDefaultValue = false)]
    [XmlAttribute]
    public string ContentType { get; set; }

    [DataMember(Name = "dlType", EmitDefaultValue = false)]
    [XmlAttribute]
    public string DlType { get; set; }

    [DataMember(Name = "parseType", EmitDefaultValue = false)]
    [XmlAttribute]
    public string ParseType { get; set; }

    [DataMember(Name = "value", EmitDefaultValue = false)]
    [XmlText]
    public string Value { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "restriction")]
  public class PropertyRestriction
  {
    [DataMember(Name = "type", EmitDefaultValue = false)]
    [XmlElement(ElementName = "type")]
    public string Type { get; set; }

    [DataMember(Name = "value", EmitDefaultValue = false)]
    [XmlElement(ElementName = "value")]
    public string Value { get; set; }

    [DataMember(Name = "valuesFrom", EmitDefaultValue = false)]
    [XmlElement(ElementName = "valuesFrom")]
    public string ValuesFrom { get; set; }

    [DataMember(Name = "cardinality", EmitDefaultValue = false)]
    [XmlElement(ElementName = "cardinality")]
    public string Cardiniality { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "role-definition")]
  public class RoleDefinition
  {
    public RoleDefinition()
    {
      this.Names = new List<QMXFName>();
      this.SuggestedDesignations = new List<SuggestedDesignation>();
      this.Restrictions = new List<PropertyRestriction>();
      this.Description = new Description();

    }

    [DataMember(Name = "restriction", EmitDefaultValue = false)]
    [XmlElement(ElementName = "restriction")]
    public List<PropertyRestriction> Restrictions { get; set; }

    [DataMember(Name = "name", EmitDefaultValue = false)]
    [XmlElement(ElementName = "name")]
    public List<QMXFName> Names { get; set; }

    [DataMember(Name = "suggested-designation", EmitDefaultValue = false)]
    [XmlElement(ElementName = "suggested-designation")]
    public List<SuggestedDesignation> SuggestedDesignations { get; set; }

    [DataMember(Name = "designation", EmitDefaultValue = false)]
    [XmlElement(ElementName = "designation")]
    public Designation Designation { get; set; }

    [DataMember(Name = "description", EmitDefaultValue = false)]
    [XmlElement(ElementName = "description")]
    public Description Description { get; set; }

    [DataMember(Name = "id", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "id")]
    public string Identifier { get; set; }

    [DataMember(Name = "range", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "range")]
    public string Range { get; set; }

    [DataMember(Name = "minimum", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "minimum")]
    public string Minimum { get; set; }

    [DataMember(Name = "maximum", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "maximum")]
    public string Maximum { get; set; }

    [DataMember(Name = "inverse-minimum", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "inverse-minimum")]
    public string InverseMinimum { get; set; }

    [DataMember(Name = "inverse-maximum", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "inverse-maximum")]
    public string InverseMaximum { get; set; }

    public static IComparer<RoleDefinition> SortAscending()
    {
      return (IComparer<RoleDefinition>)new SortAscendingHelper();
    }

    private class SortAscendingHelper : IComparer<RoleDefinition>
    {
      int IComparer<RoleDefinition>.Compare(RoleDefinition rd1, RoleDefinition rd2)
      {
        return string.Compare(rd1.Names[0].Value, rd2.Names[0].Value);
      }
    }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "value")]
  public class QMXFValue
  {
    [DataMember(Name = "text", EmitDefaultValue = false)]
    [XmlText]
    public string Text { get; set; }

    [DataMember(Name = "lang", EmitDefaultValue = false)]
    [XmlAttribute]
    public string Lang { get; set; }

    [DataMember(Name = "reference", EmitDefaultValue = false)]
    [XmlAttribute]
    public string Reference { get; set; }

    [DataMember(Name = "as", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "as")]
    public string As { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "role-qualification")]
  public class RoleQualification
  {
    public RoleQualification()
    {
      this.Names = new List<QMXFName>();
      this.Descriptions = new List<Description>();

    }

    [DataMember(Name = "name", EmitDefaultValue = false)]
    [XmlElement(ElementName = "name")]
    public List<QMXFName> Names { get; set; }

    [DataMember(Name = "id", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "id")]
    public string Identifier { get; set; }

    [DataMember(Name = "value", EmitDefaultValue = false)]
    [XmlElement(ElementName = "value")]
    public QMXFValue Value { get; set; }

    [DataMember(Name = "description", EmitDefaultValue = false)]
    [XmlElement(ElementName = "description")]
    public List<Description> Descriptions { get; set; }

    [DataMember(Name = "qualifies", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "qualifies")]
    public string Qualifies { get; set; }

    [DataMember(Name = "range", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "range")]
    public string Range { get; set; }

    [DataMember(Name = "minimum", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "minimum")]
    public string Minimum { get; set; }

    [DataMember(Name = "maximum", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "maximum")]
    public string Maximum { get; set; }

    [DataMember(Name = "inverse-minimum", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "inverse-minimum")]
    public string InverseMinimum { get; set; }

    [DataMember(Name = "inverse-maximum", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "inverse-maximum")]
    public string InverseMaximum { get; set; }

    public static IComparer<RoleQualification> SortAscending()
    {
      return (IComparer<RoleQualification>)new SortAscendingHelper();
    }

    private class SortAscendingHelper : IComparer<RoleQualification>
    {
      int IComparer<RoleQualification>.Compare(RoleQualification rq1, RoleQualification rq2)
      {
        return string.Compare(rq1.Names[0].Value, rq2.Names[0].Value);
      }
    }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "status")]
  public class QMXFStatus
  {
    public QMXFStatus()
    {
      this.Authority = "http://rdl.rdlfacade.org/data#R6569332477";
      this.Class = "http://rdl.rdlfacade.org/data#R3732211754";
    }

    [DataMember(Name = "authority", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "authority")]
    public string Authority { get; set; }

    [DataMember(Name = "class", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "class")]
    public string Class { get; set; }

    [DataMember(Name = "from", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "from")]
    public string From { get; set; }

    [DataMember(Name = "to", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "to")]
    public string To { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "type")]
  public enum QMXFType
  {
    [EnumMember(Value = "qualification")]
    [XmlElement(ElementName = "qualification")]
    Qualification,

    [EnumMember(Value = "definition")]
    [XmlElement(ElementName = "definition")]
    Definition
  }
}
