using System;
using wcf = ModuleLibrary.ReferenceServiceProxy;
using System.Collections.Generic;
using org.ids_adi.qmxf;

namespace ModuleLibrary.Extensions
{
    /// <summary>
    /// XElement extensions for processing the Mapping file.   The individual
    /// routines lend themselves to the recursive nature of the mapping
    /// structure.
    /// </summary>
    public static class QMXFExtensions
    {
      /// <summary>
      /// Gets the classification list.
      /// </summary>s
      /// <param name="dataObjects">The data objects.</param>
      /// <returns></returns>
      public static List<Classification> GetClassificationList(this List<wcf.Classification> dataObjects)
      {
        List<Classification> list = new List<Classification>();
        foreach (var record in dataObjects)
        {
          list.Add(new Classification
          {
            label = record.label,
            reference = record.reference,
          });
        }
        return list;
      }

      /// <summary>
      /// Gets the Description list.
      /// </summary>
      /// <param name="dataObjects">The data objects.</param>
      /// <returns></returns>
      public static List<Description> GetDescriptionList(this List<wcf.Description> dataObjects)
      {
        List<Description> list = new List<Description>();
        foreach (var record in dataObjects)
        {
          list.Add(new Description
          {
            contentType = record.contentType,
            lang = record.lang,
            parseType = record.parseType,
            value = record.value,
          });
        }
        return list;
      }

      /// <summary>
      /// Gets the Name list.
      /// </summary>
      /// <param name="dataObjects">The data objects.</param>
      /// <returns></returns>
      public static List<Name> GetNameList(this List<wcf.Name> dataObjects)
      {
        List<Name> list = new List<Name>();
        foreach (var record in dataObjects)
        {
          list.Add(new Name
          {
            lang = record.lang,
            value = record.value,
          });
        }
        return list;
      }

      /// <summary>
      /// Gets the Specialization list.
      /// </summary>
      /// <param name="dataObjects">The data objects.</param>
      /// <returns></returns>
      public static List<Specialization> GetSpecializationList(this List<wcf.Specialization> dataObjects)
      {
        List<Specialization> list = new List<Specialization>();
        foreach (var record in dataObjects)
        {
          list.Add(new Specialization
          {
            label = record.label,
            reference = record.reference,
          });
        }
        return list;
      }

      /// <summary>
      /// Gets the Status list.
      /// </summary>
      /// <param name="dataObjects">The data objects.</param>
      /// <returns></returns>
      public static List<Status> GetStatusList(this List<wcf.Status> dataObjects)
      {
        List<Status> list = new List<Status>();
        foreach (var record in dataObjects)
        {
          list.Add(new Status
          {
            authority = record.authority,
            Class = record.Class,
            from = record.from,
            to = record.to,
          });
        }
        return list;
      }

      /// <summary>
      /// Gets the SuggestedDesignation list.
      /// </summary>
      /// <param name="dataObjects">The data objects.</param>
      /// <returns></returns>
      public static List<SuggestedDesignation> GetSuggestedDesignationList(this List<wcf.SuggestedDesignation> dataObjects)
      {
        List<SuggestedDesignation> list = new List<SuggestedDesignation>();
        foreach (var record in dataObjects)
        {
          list.Add(new SuggestedDesignation
          {
            value = record.value,
          });
        }
        return list;
      }

      /// <summary>
      /// Gets the TextualDefinition list.
      /// </summary>
      /// <param name="dataObjects">The data objects.</param>
      /// <returns></returns>
      public static List<TextualDefinition> GetTextualDefinitionList(this List<wcf.TextualDefinition> dataObjects)
      {
        List<TextualDefinition> list = new List<TextualDefinition>();
        foreach (var record in dataObjects)
        {
          list.Add(new TextualDefinition
          {
            value = record.value,
          });
        }
        return list;
      }

      /// <summary>
      /// Gets the LicenseTerms list.
      /// </summary>
      /// <param name="dataObjects">The data objects.</param>
      /// <returns></returns>
      public static List<LicenseTerms> GetLicenseTermsList(this List<wcf.LicenseTerms> dataObjects)
      {
        List<LicenseTerms> list = new List<LicenseTerms>();
        foreach (var record in dataObjects)
        {
          list.Add(new LicenseTerms
          {
            identifier = record.identifier,
            licenseText = record.licenseText.GetLicenseTextList(),
          });
        }
        return list;
      }

      /// <summary>
      /// Gets the LicenseText list.
      /// </summary>
      /// <param name="dataObjects">The data objects.</param>
      /// <returns></returns>
      public static List<LicenseText> GetLicenseTextList(this List<wcf.LicenseText> dataObjects)
      {
        List<LicenseText> list = new List<LicenseText>();
        foreach (var record in dataObjects)
        {
          list.Add(new LicenseText
          {
            formal = record.formal,
            lang = record.lang,
            value = record.value,
          });
        }
        return list;
      }

      /// <summary>
      /// Gets the Designation list.
      /// </summary>
      /// <param name="dataObjects">The data objects.</param>
      /// <returns></returns>
      public static List<Designation> GetDesignationList(this List<wcf.Designation> dataObjects)
      {
        List<Designation> list = new List<Designation>();
        foreach (var record in dataObjects)
        {
          list.Add(new Designation
          {
            value = record.value,
          });
        }
        return list;
      }

      /// <summary>
      /// Gets the RoleDefinition list.
      /// </summary>
      /// <param name="dataObjects">The data objects.</param>
      /// <returns></returns>
      public static List<RoleDefinition> GetRoleDefinitionList(this List<wcf.RoleDefinition> dataObjects)
      {
        List<RoleDefinition> list = new List<RoleDefinition>();
        foreach (var record in dataObjects)
        {
          list.Add(
            new RoleDefinition
            {
              identifier = record.identifier,
              inverseMaximum = record.inverseMaximum,
              inverseMinimum = record.inverseMinimum,
              maximum = record.maximum,
              minimum = record.minimum,
              range = record.range,
              designation = new Designation() 
              { 
                value = record.designation.value 
              },
              description = new Description() 
              { 
                contentType = record.description.contentType,
                lang = record.description.lang,
                parseType = record.description.parseType,
                value = record.description.value,
              },
              name = record.name.GetNameList(),
              suggestedDesignation = record.suggestedDesignation.GetSuggestedDesignationList(),
            }
          );
        }
        return list;
      }

      /// <summary>
      /// Gets the RoleQualification list.
      /// </summary>
      /// <param name="dataObjects">The data objects.</param>
      /// <returns></returns>
      public static List<RoleQualification> GetRoleQualificationList(this List<wcf.RoleQualification> dataObjects)
      {
        List<RoleQualification> list = new List<RoleQualification>();
        foreach (var record in dataObjects)
        {
          Value value = null;
          if (record.value != null)
          {
            value = new Value
            {
              As = record.value.As,
              lang = record.value.lang,
              reference = record.value.reference,
              text = record.value.text,
            };
          }

          list.Add(
            new RoleQualification
            {
              inverseMaximum = record.inverseMaximum,
              inverseMinimum = record.inverseMinimum,
              maximum = record.maximum,
              minimum = record.minimum,
              range = record.range,
              qualifies = record.qualifies,
              value = value,
              description = record.description.GetDescriptionList(),
              name = record.name.GetNameList(),
              
            }
          );
        }
        return list;
      } 

    }
}
