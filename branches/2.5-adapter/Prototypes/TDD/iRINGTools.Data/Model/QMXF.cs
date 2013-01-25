using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRINGTools.Data
{
  public class QMXF
  {
    public LazyList<ClassDefinition> ClassDefinitions { get; set; }
    public LazyList<TemplateDefinition> TemplateDefinitions { get; set; }
    public LazyList<TemplateQualification> TemplateQualifications { get; set; }
    public LazyList<License> Licenses { get; set; }

    public string Timestamp { get; set; }
    public string LicenseRef { get; set; }
    public string TargetRepository { get; set; }
    public string SourceRepository { get; set; }

    public QMXF()
    {
    }
  }

  public class TemplateDefinition
  {
    public string RepositoryName { get; set; }
    public LazyList<SuggestedDesignation> SuggestedDesignation { get; set; }
    public Designation Designation { get; set; }
    public LazyList<QMXFName> Name { get; set; }
    public LazyList<Description> Description { get; set; }
    public LazyList<TextualDefinition> TextualDefinition { get; set; }
    public LazyList<QMXFStatus> Status { get; set; }
    public string Identifier { get; set; }
    public LazyList<RoleDefinition> RoleDefinition { get; set; }
    public LazyList<Specialization> Specialization { get; set; }

    public TemplateDefinition()
    {
    }
  }

  public class ClassDefinition
  {
    public string RepositoryName { get; set; }
    public EntityType EntityType { get; set; }
    public LazyList<Classification> Classification { get; set; }
    public LazyList<Specialization> Specialization { get; set; }
    public LazyList<SuggestedDesignation> SuggestedDesignation { get; set; }
    public Designation Designation { get; set; }
    public LazyList<QMXFName> Name { get; set; }
    public LazyList<Description> Description { get; set; }
    public LazyList<TextualDefinition> TextualDefinition { get; set; }
    public LazyList<QMXFStatus> Status { get; set; }
    public string Identifier { get; set; }

    public ClassDefinition()
    {
    }
  }

  public class TemplateQualification
  {
    public string RepositoryName { get; set; }
    public LazyList<SuggestedDesignation> SuggestedDesignation { get; set; }
    public LazyList<Specialization> Specialization { get; set; }
    public LazyList<Designation> Designation { get; set; }
    public LazyList<QMXFName> Name { get; set; }
    public LazyList<Description> Description { get; set; }
    public LazyList<TextualDefinition> TextualDefinition { get; set; }
    public LazyList<QMXFStatus> Status { get; set; }
    public string Identifier { get; set; }
    public string Qualifies { get; set; }
    public LazyList<RoleQualification> RoleQualification { get; set; }

    public TemplateQualification()
    {
    }
  }

  public class License
  {
    public LazyList<LicenseTerms> LicenseTerms { get; set; }

    public License()
    {
    }
  }

  public class LicenseTerms
  {
    public LazyList<LicenseText> LicenseText { get; set; }
    public string Identifier { get; set; }

    public LicenseTerms()
    {
    }
  }

  public class LicenseText
  {
    public string Lang { get; set; }
    public string Value { get; set; }
    public string Formal { get; set; }

    public LicenseText()
    {
      this.Formal = "true";
    }
  }

  public class EntityType
  {
    public string Reference { get; set; }
  }

  public class Classification
  {
    public string Reference { get; set; }
    public string Label { get; set; }
    public string Lang { get; set; }

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

  public class Specialization
  {
    public string Reference { get; set; }
    public string Label { get; set; }
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

  public class SuggestedDesignation
  {
    public string Value { get; set; }
  }

  public class QMXFName
  {
    public string Lang { get; set; }
    public string Value { get; set; }
  }

  public class Designation
  {
    public string Value { get; set; }
  }

  public class Description
  {
    public string Lang { get; set; }
    public string ContentType { get; set; }
    public string ParseType { get; set; }
    public string Value { get; set; }
  }

  public class TextualDefinition
  {
    public string Lang { get; set; }
    public string Value { get; set; }

    public TextualDefinition()
    {
    }
  }

  public class RuleSet
  {
    public string ContentType { get; set; }
    public string DLType { get; set; }
    public string ParseType { get; set; }
    public string Value { get; set; }
  }

  public class PropertyRestriction
  {
    public string Type { get; set; }
    public string Value { get; set; }
    public string ValuesFrom { get; set; }
    public string Cardiniality { get; set; }
  }

  public class RoleDefinition
  {
    public LazyList<PropertyRestriction> Restrictions { get; set; }
    public LazyList<QMXFName> Name { get; set; }
    public LazyList<SuggestedDesignation> SuggestedDesignation { get; set; }
    public Designation Designation { get; set; }
    public Description Description { get; set; }
    public string Identifier { get; set; }
    public string Range { get; set; }
    public string Minimum { get; set; }
    public string Maximum { get; set; }
    public string InverseMinimum { get; set; }
    public string InverseMaximum { get; set; }

    public RoleDefinition()
    {
    }

    public static IComparer<RoleDefinition> SortAscending()
    {
      return (IComparer<RoleDefinition>)new SortAscendingHelper();
    }

    private class SortAscendingHelper : IComparer<RoleDefinition>
    {
      int IComparer<RoleDefinition>.Compare(RoleDefinition rd1, RoleDefinition rd2)
      {
        return string.Compare(rd1.Name[0].Value, rd2.Name[0].Value);
      }
    }
  }

  public class QMXFValue
  {
    public string Text { get; set; }
    public string Lang { get; set; }
    public string Reference { get; set; }
    public string As { get; set; }
  }

  public class RoleQualification
  {
    public RoleQualification()
    {
    }

    public LazyList<QMXFName> Name { get; set; }
    public string Identifier { get; set; }
    public QMXFValue Value { get; set; }
    public LazyList<Description> Description { get; set; }
    public string Qualifies { get; set; }
    public string Range { get; set; }
    public string Minimum { get; set; }
    public string Maximum { get; set; }
    public string InverseMinimum { get; set; }
    public string InverseMaximum { get; set; }

    public static IComparer<RoleQualification> SortAscending()
    {
      return (IComparer<RoleQualification>)new SortAscendingHelper();
    }

    private class SortAscendingHelper : IComparer<RoleQualification>
    {
      int IComparer<RoleQualification>.Compare(RoleQualification rq1, RoleQualification rq2)
      {
        return string.Compare(rq1.Name[0].Value, rq2.Name[0].Value);
      }
    }
  }

  public class QMXFStatus
  {
    public string Authority { get; set; }
    public string Class { get; set; }
    public string From { get; set; }
    public string To { get; set; }

    public QMXFStatus()
    {
      this.Authority = "http://rdl.rdlfacade.org/data#R6569332477";
      this.Class = "http://rdl.rdlfacade.org/data#R3732211754";
    }
  }

  public enum QMXFType
  {
    Qualification,
    Definition
  }
}