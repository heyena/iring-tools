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
  public class QMXF
  {
    public QMXF()
    {
      this.ClassDefinitions = new List<ClassDefinition>();
      this.TemplateDefinitions = new List<TemplateDefinition>();
      this.TemplateQualifications = new List<TemplateQualification>();
      this.Licenses = new List<License>();
    }

    [DataMember(Name = "class-definition", EmitDefaultValue = false)]
    public List<ClassDefinition> ClassDefinitions { get; set; }

    [DataMember(Name = "template-definition", EmitDefaultValue = false)]
    public List<TemplateDefinition> TemplateDefinitions { get; set; }

    [DataMember(Name = "template-qualification", EmitDefaultValue = false)]
    public List<TemplateQualification> TemplateQualifications { get; set; }

    [DataMember(Name = "license", EmitDefaultValue = false)]
    public List<License> Licenses { get; set; }

    [DataMember(Name = "timestamp", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "timestamp")]
    public string timestamp { get; set; }

    [DataMember(Name = "license-ref", EmitDefaultValue = false)]
    public string LicenseRef { get; set; }

    [DataMember(Name = "targetRepository", EmitDefaultValue = false)]
    public string TargetRepository { get; set; }

    [DataMember(Name = "sourceRepository", EmitDefaultValue = false)]
    public string SourceRepository { get; set; }

  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "template-definition")]
  public class TemplateDefinition
  {
    public TemplateDefinition()
    {
      this.Name = new List<QMXFName>();
      this.Description = new List<Description>();
      this.SuggestedDesignation = new List<SuggestedDesignation>();
      this.Status = new List<QMXFStatus>();
      this.TextualDefinition = new List<TextualDefinition>();
      this.RoleDefinition = new List<RoleDefinition>();
      this.RepositoryName = string.Empty;
      this.Specialization = new List<Specialization>();
    }

    [DataMember(Name = "repository", EmitDefaultValue = false)]
    public string RepositoryName { get; set; }

    [DataMember(Name = "suggested-designation", EmitDefaultValue = false)]
    public List<SuggestedDesignation> SuggestedDesignation { get; set; }

    [DataMember(Name = "designation", EmitDefaultValue = false)]
    public Designation Designation { get; set; }

    [DataMember(Name = "name", EmitDefaultValue = false)]
    public List<QMXFName> Name { get; set; }

    [DataMember(Name = "description", EmitDefaultValue = false)]
    public List<Description> Description { get; set; }

    [DataMember(Name = "textual-definition", EmitDefaultValue = false)]
    public List<TextualDefinition> TextualDefinition { get; set; }

    [DataMember(Name = "status", EmitDefaultValue = false)]
    public List<QMXFStatus> Status { get; set; }

    [DataMember(Name = "id", EmitDefaultValue = false)]
    public string Identifier { get; set; }

    [DataMember(Name = "role-definition", EmitDefaultValue = false)]
    public List<RoleDefinition> RoleDefinition { get; set; }

    [DataMember(Name = "specialization", EmitDefaultValue = false)]
    public List<Specialization> Specialization { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "class-definition")]
  public class ClassDefinition
  {
    public ClassDefinition()
    {
      this.Name = new List<QMXFName>();
      this.Description = new List<Description>();
      this.Classification = new List<Classification>();
      this.Specialization = new List<Specialization>();
      this.Status = new List<QMXFStatus>();
      this.SuggestedDesignation = new List<SuggestedDesignation>();
      this.TextualDefinition = new List<TextualDefinition>();
      this.RepositoryName = string.Empty;
    }

    [DataMember(Name = "repository", EmitDefaultValue = false)]
    public string RepositoryName { get; set; }

    [DataMember(Name = "entity-type", EmitDefaultValue = false)]
    public EntityType EntityType { get; set; }

    [DataMember(Name = "classification", EmitDefaultValue = false)]
    public List<Classification> Classification { get; set; }

    [DataMember(Name = "specialization", EmitDefaultValue = false)]
    public List<Specialization> Specialization { get; set; }

    [DataMember(Name = "suggested-designation", EmitDefaultValue = false)]
    public List<SuggestedDesignation> SuggestedDesignation { get; set; }

    [DataMember(Name = "designation", EmitDefaultValue = false)]
    public Designation Designation { get; set; }

    [DataMember(Name = "name", EmitDefaultValue = false)]
    public List<QMXFName> Name { get; set; }

    [DataMember(Name = "description", EmitDefaultValue = false)]
    public List<Description> Description { get; set; }

    [DataMember(Name = "textual-definition", EmitDefaultValue = false)]
    public List<TextualDefinition> TextualDefinition { get; set; }

    [DataMember(Name = "status", EmitDefaultValue = false)]
    public List<QMXFStatus> Status { get; set; }

    [DataMember(Name = "id", EmitDefaultValue = false)]
    public string Identifier { get; set; }

  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "template-qualification")]
  public class TemplateQualification
  {
    public TemplateQualification()
    {
      this.Name = new List<QMXFName>();
      this.Description = new List<Description>();
      this.SuggestedDesignation = new List<SuggestedDesignation>();
      this.Designation = new List<Designation>();
      this.Status = new List<QMXFStatus>();
      this.TextualDefinition = new List<TextualDefinition>();
      this.RoleQualification = new List<RoleQualification>();
      this.Specialization = new List<Specialization>();
      this.RepositoryName = string.Empty;
    }

    [DataMember(Name = "repository", EmitDefaultValue = false)]
    public string RepositoryName { get; set; }

    [DataMember(Name = "suggested-designation", EmitDefaultValue = false)]
    public List<SuggestedDesignation> SuggestedDesignation { get; set; }

    [DataMember(Name = "specialization", EmitDefaultValue = false)]
    public List<Specialization> Specialization { get; set; }

    [DataMember(Name = "designation", EmitDefaultValue = false)]
    [XmlElement(ElementName = "designation")]
    public List<Designation> Designation { get; set; }

    [DataMember(Name = "name", EmitDefaultValue = false)]
    public List<QMXFName> Name { get; set; }

    [DataMember(Name = "description", EmitDefaultValue = false)]
    public List<Description> Description { get; set; }

    [DataMember(Name = "textual-definition", EmitDefaultValue = false)]
    public List<TextualDefinition> TextualDefinition { get; set; }

    [DataMember(Name = "status", EmitDefaultValue = false)]
    public List<QMXFStatus> Status { get; set; }

    [DataMember(Name = "id", EmitDefaultValue = false)]
    public string Identifier { get; set; }

    [DataMember(Name = "qualifies", EmitDefaultValue = false)]
    public string qualifies { get; set; }

    [DataMember(Name = "role-qualification", EmitDefaultValue = false)]
    public List<RoleQualification> RoleQualification { get; set; }

  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "license")]
  public class License
  {
    public License()
    {
      this.LicenseTerms = new List<LicenseTerms>();
    }

    [DataMember(Name = "license-terms", EmitDefaultValue = false)]
    public List<LicenseTerms> LicenseTerms { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "license-terms")]
  public class LicenseTerms
  {
    public LicenseTerms()
    {
      this.LicenseText = new List<LicenseText>();
    }

    [DataMember(Name = "license-text", EmitDefaultValue = false)]
    public List<LicenseText> LicenseText { get; set; }

    [DataMember(Name = "id", EmitDefaultValue = false)]
    public string Identifier { get; set; }

  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "license-text")]
  public class LicenseText
  {
    public LicenseText()
    {
      this.Formal = "true";
    }

    [DataMember(Name = "lang",EmitDefaultValue = false)]
    public string Lang { get; set; }

    [DataMember(Name = "value",EmitDefaultValue = false)]
    public string Value { get; set; }

    [DataMember(Name = "formal", EmitDefaultValue = false)]
    public string Formal { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "entity-type")]
  public class EntityType
  {
    [DataMember(Name = "reference", EmitDefaultValue = false)]
    public string Reference { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "classification")]
  public class Classification
  {
    [DataMember(Name = "reference", EmitDefaultValue = false)]
    public string Reference { get; set; }

    [DataMember(Name = "label", EmitDefaultValue = false)]
    public string Label { get; set; }

    [DataMember(Name = "lang", EmitDefaultValue = false)]
    public string Lang { get; set; }

    public static IComparer<Classification> SortAscending()
    {
      return (IComparer<Classification>)new SortAscendingHelper();
    }

    private class SortAscendingHelper : IComparer<Classification>
    {
      int IComparer<Classification>.Compare(Classification cd1, Classification cd2)
      {
        return System.String.CompareOrdinal(cd1.Label, cd2.Label);
      }
    }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "specialization")]
  public class Specialization
  {
    [DataMember(Name = "reference", EmitDefaultValue = false)]
    public string Reference { get; set; }

    [DataMember(Name = "rdsuri", EmitDefaultValue = false)]
    public string Rdsuri { get; set; }

    [DataMember(Name = "label", EmitDefaultValue = false)]
    public string Label { get; set; }

    [DataMember(Name = "lang", EmitDefaultValue = false)]
    public string Lang { get; set; }

    public static IComparer<Specialization> SortAscending()
    {
      return (IComparer<Specialization>)new SortAscendingHelper();
    }

    private class SortAscendingHelper : IComparer<Specialization>
    {
      int IComparer<Specialization>.Compare(Specialization s1, Specialization s2)
      {
        return System.String.CompareOrdinal(s1.Label, s2.Label);
      }
    }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "suggested-designation")]
  public class SuggestedDesignation
  {
    [DataMember(Name = "value", EmitDefaultValue = false)]
    public string Value { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "name")]
  public class QMXFName
  {
    [DataMember(Name = "lang", EmitDefaultValue = false)]
    public string Lang { get; set; }

    [DataMember(Name = "value", EmitDefaultValue = false)]
    public string Value { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "designation")]
  public class Designation
  {
    [DataMember(Name = "value", EmitDefaultValue = false)]
    public string Value { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "description")]
  public class Description
  {
    [DataMember(Name = "lang", EmitDefaultValue = false)]
    public string Lang { get; set; }

    [DataMember(Name = "contentType", EmitDefaultValue = false)]
    public string ContentType { get; set; }

    [DataMember(Name = "parseType", EmitDefaultValue = false)]
    public string ParseType { get; set; }

    [DataMember(Name = "value", EmitDefaultValue = false)]
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
    public string Lang { get; set; }

    [DataMember(Name = "value", EmitDefaultValue = false)]
    public string Value { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "rule-set")]
  public class RuleSet
  {
    [DataMember(Name = "contentType", EmitDefaultValue = false)]
    public string ContentType { get; set; }

    [DataMember(Name = "dlType", EmitDefaultValue = false)]
    public string DlType { get; set; }

    [DataMember(Name = "parseType", EmitDefaultValue = false)]
    public string ParseType { get; set; }

    [DataMember(Name = "value", EmitDefaultValue = false)]
    public string Value { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "restriction")]
  public class PropertyRestriction
  {
    [DataMember(Name = "type", EmitDefaultValue = false)]
    public string Type { get; set; }

    [DataMember(Name = "value", EmitDefaultValue = false)]
    public string Value { get; set; }

    [DataMember(Name = "valuesFrom", EmitDefaultValue = false)]
    public string ValuesFrom { get; set; }

    [DataMember(Name = "cardinality", EmitDefaultValue = false)]
    public string Cardiniality { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "role-definition")]
  public class RoleDefinition
  {
    public RoleDefinition()
    {
      this.Name = new List<QMXFName>();
      this.SuggestedDesignation = new List<SuggestedDesignation>();
      this.Restrictions = new List<PropertyRestriction>();
      this.Description = new Description();
    }

    [DataMember(Name = "restriction", EmitDefaultValue = false)]
    public List<PropertyRestriction> Restrictions { get; set; }

    [DataMember(Name = "name", EmitDefaultValue = false)]
    public List<QMXFName> Name { get; set; }

    [DataMember(Name = "suggested-designation", EmitDefaultValue = false)]
    public List<SuggestedDesignation> SuggestedDesignation { get; set; }

    [DataMember(Name = "designation", EmitDefaultValue = false)]
    public Designation Designation { get; set; }

    [DataMember(Name = "description", EmitDefaultValue = false)]
    public Description Description { get; set; }

    [DataMember(Name = "id", EmitDefaultValue = false)]
    public string Identifier { get; set; }

    [DataMember(Name = "range", EmitDefaultValue = false)]
    public string Range { get; set; }

    [DataMember(Name = "minimum", EmitDefaultValue = false)]
    public string Minimum { get; set; }

    [DataMember(Name = "maximum", EmitDefaultValue = false)]
    public string Maximum { get; set; }

    [DataMember(Name = "inverse-minimum", EmitDefaultValue = false)]
    public string InverseMinimum { get; set; }

    [DataMember(Name = "inverse-maximum", EmitDefaultValue = false)]
    public string InverseMaximum { get; set; }

    public static IComparer<RoleDefinition> SortAscending()
    {
      return (IComparer<RoleDefinition>)new SortAscendingHelper();
    }

    private class SortAscendingHelper : IComparer<RoleDefinition>
    {
      int IComparer<RoleDefinition>.Compare(RoleDefinition rd1, RoleDefinition rd2)
      {
        return System.String.CompareOrdinal(rd1.Name[0].Value, rd2.Name[0].Value);
      }
    }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "value")]
  public class QMXFValue
  {
    [DataMember(Name = "text", EmitDefaultValue = false)]
    public string Text { get; set; }

    [DataMember(Name = "lang", EmitDefaultValue = false)]
    public string Lang { get; set; }

    [DataMember(Name = "reference", EmitDefaultValue = false)]
    public string Reference { get; set; }

    [DataMember(Name = "as", EmitDefaultValue = false)]
    public string As { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "role-qualification")]
  public class RoleQualification
  {
    public RoleQualification()
    {
      this.Name = new List<QMXFName>();
      this.Description = new List<Description>();
    }

    [DataMember(Name = "name", EmitDefaultValue = false)]
    public List<QMXFName> Name { get; set; }

    [DataMember(Name = "id", EmitDefaultValue = false)]
    public string Identifier { get; set; }

    [DataMember(Name = "value", EmitDefaultValue = false)]
    public QMXFValue Value { get; set; }

    [DataMember(Name = "description", EmitDefaultValue = false)]
    public List<Description> Description { get; set; }

    [DataMember(Name = "qualifies", EmitDefaultValue = false)]
    public string Qualifies { get; set; }

    [DataMember(Name = "range", EmitDefaultValue = false)]
    [XmlAttribute(AttributeName = "range")]
    public string Range { get; set; }

    [DataMember(Name = "minimum", EmitDefaultValue = false)]
    public string Minimum { get; set; }

    [DataMember(Name = "maximum", EmitDefaultValue = false)]
    public string Maximum { get; set; }

    [DataMember(Name = "inverse-minimum", EmitDefaultValue = false)]
    public string InverseMinimum { get; set; }

    [DataMember(Name = "inverse-maximum", EmitDefaultValue = false)]
    public string InverseMaximum { get; set; }

    public static IComparer<RoleQualification> SortAscending()
    {
      return (IComparer<RoleQualification>)new SortAscendingHelper();
    }

    private class SortAscendingHelper : IComparer<RoleQualification>
    {
      int IComparer<RoleQualification>.Compare(RoleQualification rq1, RoleQualification rq2)
      {
        return System.String.CompareOrdinal(rq1.Name[0].Value, rq2.Name[0].Value);
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
    public string Authority { get; set; }

    [DataMember(Name = "class", EmitDefaultValue = false)]
    public string Class { get; set; }

    [DataMember(Name = "from", EmitDefaultValue = false)]
    public string From { get; set; }

    [DataMember(Name = "to", EmitDefaultValue = false)]
    public string To { get; set; }
  }

  [DataContract(Namespace = "http://ns.ids-adi.org/qxf/model#", Name = "type")]
  public enum QMXFType
  {
    [EnumMember(Value = "qualification")]
    Qualification,

    [EnumMember(Value = "definition")]
    Definition
  }
}
