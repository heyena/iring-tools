using System;
using wcf = ModuleLibrary.AdapterServiceProxy;
using System.Collections.Generic;
using org.ids_adi.iring;
using org.ids_adi.qmxf;

#if SILVERLIGHT
#else
  using System.Xml.Linq;
  using System.Linq;
#endif

namespace ModuleLibrary.Extensions
{
    /// <summary>
    /// XElement extensions for processing the Mapping file.   The individual
    /// routines lend themselves to the recursive nature of the mapping
    /// structure.
    /// </summary>
    public static class MappingExtensions
    {
        /// <summary>
        /// Gets the data object list.
        /// </summary>
        /// <param name="dataObjects">The data objects.</param>
        /// <returns></returns>
        public static List<DataObjectMap> GetDataObjectList(this wcf.DataObjectMap[] dataObjects)
        {
            List<DataObjectMap> list = new List<DataObjectMap>();
            foreach (var record in dataObjects)
            {
                list.Add(new DataObjectMap
                {
                    name = record.name,
                    inFilter = record.inFilter,
                    outFilter = record.outFilter
                });
            }
            return list;
        }

        public static wcf.DataObjectMap[] GetDataObjectList(this List<DataObjectMap> dataObjects)
        {
            if (dataObjects == null)
                return null;

            wcf.DataObjectMap[] list = new wcf.DataObjectMap[dataObjects.Count];
            for (int i = 0; i < dataObjects.Count; i++)
            {
                list[i] = new wcf.DataObjectMap
                {
                    name = dataObjects[i].name,
                    inFilter = dataObjects[i].inFilter,
                    outFilter = dataObjects[i].outFilter
                };
            }
            return list;
        }

        /// <summary>
        /// Gets the template map list.
        /// </summary>
        /// <param name="dataObjects">The data objects.</param>
        /// <returns></returns>
        public static List<TemplateMap> GetTemplateMapList(this wcf.TemplateMap[] dataObjects)
        {
            List<TemplateMap> list = new List<TemplateMap>();
            foreach (var record in dataObjects)
            {
                list.Add(new TemplateMap
                {
                    classRole = record.classRole,
                    name = record.name,
                    type = record.type.GetTemplateType(),
                    roleMaps = record.RoleMaps.GetRoleMapList(),
                    templateId = record.templateId
                });
            }
            return list;
        }


        public static wcf.TemplateMap[] GetTemplateMapList(this List<TemplateMap> dataObjects)
        {
            if (dataObjects == null)
                return null;

            wcf.TemplateMap[] list = new wcf.TemplateMap[dataObjects.Count];
            for (int i = 0; i < dataObjects.Count; i++)
            {
                list[i] = new wcf.TemplateMap
                {
                    classRole = dataObjects[i].classRole,
                    name = dataObjects[i].name,
                    type = dataObjects[i].type.GetTemplateType(),
                    RoleMaps = dataObjects[i].roleMaps.GetRoleMapList(),
                    templateId = dataObjects[i].templateId
                };
            }
            return list;
        }


        /// <summary>
        /// Gets the data object list.
        /// </summary>
        /// <param name="dataObjects">The data objects.</param>
        /// <returns></returns>
        public static List<RoleMap> GetRoleMapList(this wcf.RoleMap[] dataObjects)
        {
            List<RoleMap> list = new List<RoleMap>();
            foreach (var record in dataObjects)
            {
                list.Add(new RoleMap
                {
                    name = record.name,
                    propertyName = record.propertyName,
                    roleId = record.roleId,
                    valueList = record.valueList,
                    dataType = record.dataType,
                    classMap = GetPopulatedClassMap(record.ClassMap)
                });
            }
            return list;
        }

        public static wcf.RoleMap[] GetRoleMapList(this List<RoleMap> dataObjects)
        {

            wcf.RoleMap[] list = new wcf.RoleMap[dataObjects.Count];

            for (int i = 0; i < dataObjects.Count; i++)
            {
                list[i] = new wcf.RoleMap
                {
                    name = dataObjects[i].name,
                    propertyName = dataObjects[i].propertyName,
                    roleId = dataObjects[i].roleId,
                    valueList = dataObjects[i].valueList,
                    dataType = dataObjects[i].dataType,
                    ClassMap = GetPopulatedClassMap(dataObjects[i].classMap)
                };
            }
            return list;
        }


        public static ClassMap GetPopulatedClassMap(wcf.ClassMap classMap)
        {
            if (classMap == null)
                return null;

            return new ClassMap
            {
                classId = classMap.classId,
                identifier = classMap.identifier,
                name = classMap.name,
                templateMaps = classMap.TemplateMaps.GetTemplateMapList()
            };
        }

        public static wcf.ClassMap GetPopulatedClassMap(ClassMap classMap)
        {
            if (classMap == null)
                return null;

            return new wcf.ClassMap
            {
                classId = classMap.classId,
                identifier = classMap.identifier,
                name = classMap.name,
                TemplateMaps = classMap.templateMaps.GetTemplateMapList()
            };
        }

        /// <summary>
        /// Gets the type of the template.
        /// </summary>
        /// <param name="templateType">Type of the template.</param>
        /// <returns></returns>
        public static TemplateType GetTemplateType(this wcf.TemplateType templateType)
        {
            return (TemplateType)Enum.Parse(typeof(TemplateType), templateType.ToString(), true);
        }


        public static wcf.TemplateType GetTemplateType(this TemplateType templateType)
        {
            return (wcf.TemplateType)Enum.Parse(typeof(wcf.TemplateType), templateType.ToString(), true);
        }



#if SILVERLIGHT
#else

    /// <summary>
    /// Gets the class map from an XElement node.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns></returns>
    public static ClassMap GetClassMap(this XElement element)
    {
      // By default we'll have a populated classMapData object
      ClassMap classMapData =
        new ClassMap
        {
          classId = "",
          identifier = "",
          name = "",
          templateMaps = new List<TemplateMap>()
        };

      // If this element does not have ClassMap
      // records then we don't want to reference
      // the record to get attributes (will be null)
      var record = element.Element("ClassMap");

      // Ensure we only attempt to pull attributes on
      // valid records
      if (record != null)
      {
        classMapData.classId = record.GetAttrib("classId");
        classMapData.identifier = record.GetAttrib("identifier");
        classMapData.name = record.GetAttrib("name");
        classMapData.templateMaps = GetTemplateList(record);
      }
      return classMapData;
    }

    /// <summary>
    /// Gets the template list from an XElement node.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns></returns>
    public static List<TemplateMap> GetTemplateList(this XElement element)
    {
      var list = from record in element.Elements("TemplateMaps").Elements("TemplateMap")
                 select new TemplateMap
                 {
                   classRole = record.GetAttrib("classRole"),
                   roleMaps = GetRoleMapList(record),
                   name = record.GetAttrib("name"),
                   templateId = record.GetAttrib("templateId"),
                   type = (TemplateType)Enum.Parse(typeof(TemplateType), record.GetAttrib("type"))
                 };
      return list.ToList<TemplateMap>();
    }

    /// <summary>
    /// Gets the role map list from an XElement node.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns></returns>
    public static List<RoleMap> GetRoleMapList(this XElement element)
    {
      var list = from record in element.Elements("RoleMaps").Elements("RoleMap")
                 select new RoleMap
                 {
                   classMap = GetClassMap(record),
                   dataType = record.GetAttrib("dataType"),
                   name = record.GetAttrib("name"),
                   propertyName = record.GetAttrib("propertyName"),
                   roleId = record.GetAttrib("roleId"),
                   valueList = record.GetAttrib("valueList")
                 };

      return list.ToList<RoleMap>();
    }

    /// <summary>
    /// Gets the graph list from an XElement node.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns></returns>
    public static List<GraphMap> GetGraphList(this XElement element)
    {
      var list = from record in element.Elements("GraphMap")
                 select new GraphMap
                 {
                   classId = record.GetAttrib("classId"),
                   dataObjectMaps = record.GetDataObjectList(),
                   identifier = record.GetAttrib("identifier"),
                   name = record.GetAttrib("name"),
                   templateMaps = record.GetTemplateList()
                 };
      return list.ToList<GraphMap>();
    }

    /// <summary>
    /// Gets the value map list from XElement node.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns></returns>
    public static List<ValueMap> GetValueMapList(this XElement element)
    {
      var list = from record in element.Elements("ValueMaps").Elements("ValueMap")
                 select new ValueMap
                 {
                   internalValue = record.GetAttrib("internalValue"),
                   modelURI = record.GetAttrib("modelURI"),
                   valueList = record.GetAttrib("valueList")
                 };
      return list.ToList<ValueMap>();
    }

    /// <summary>
    /// Gets the data object list from XElement node.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns></returns>
    public static List<DataObjectMap> GetDataObjectList(this XElement element)
    {
      var list = from record in element.Elements("DataObjects").Elements("DataObject")
                 select new DataObjectMap
                 {
                   name = record.GetAttrib("name"),
                   inFilter = record.GetAttrib("inFilter"),
                   outFilter = record.GetAttrib("outFilter")
                 };

      return list.ToList<DataObjectMap>();
    }

    /// <summary>
    /// Gets the attrib for the selected node.
    /// </summary>
    /// <param name="xelement">The xelement.</param>
    /// <param name="attribName">Name of the attrib.</param>
    /// <returns></returns>
    public static string GetAttrib(this XElement xelement, string attribName)
    {
      object result = xelement.Attribute(attribName);
      if (result == null)
        return "";

      if (xelement.Attribute(attribName).Value == null)
        return "";

      return xelement.Attribute(attribName).Value;
    }
#endif
    }
}
