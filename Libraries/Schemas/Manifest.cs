﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: System.Runtime.Serialization.ContractNamespaceAttribute("http://www.iringtools.org/dxfr/manifest", ClrNamespace="org.iringtools.dxfr.manifest")]
[assembly: System.Runtime.Serialization.ContractNamespaceAttribute("http://www.iringtools.org/mapping", ClrNamespace="org.iringtools.mapping")]

namespace org.iringtools.dxfr.manifest
{
    using System.Runtime.Serialization;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Manifest", Namespace="http://www.iringtools.org/dxfr/manifest")]
    public partial class Manifest : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private org.iringtools.dxfr.manifest.Graphs graphsField;
        
        private string versionField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false)]
        public org.iringtools.dxfr.manifest.Graphs graphs
        {
            get
            {
                return this.graphsField;
            }
            set
            {
                this.graphsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false)]
        public string version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.CollectionDataContractAttribute(Name="Graphs", Namespace="http://www.iringtools.org/dxfr/manifest", ItemName="graph")]
    public class Graphs : System.Collections.Generic.List<org.iringtools.dxfr.manifest.Graph>
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Graph", Namespace="http://www.iringtools.org/dxfr/manifest")]
    public partial class Graph : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string nameField;
        
        private org.iringtools.dxfr.manifest.ClassTemplatesList classTemplatesListField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false)]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false, Order=1)]
        public org.iringtools.dxfr.manifest.ClassTemplatesList classTemplatesList
        {
            get
            {
                return this.classTemplatesListField;
            }
            set
            {
                this.classTemplatesListField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.CollectionDataContractAttribute(Name="ClassTemplatesList", Namespace="http://www.iringtools.org/dxfr/manifest", ItemName="classTemplates")]
    public class ClassTemplatesList : System.Collections.Generic.List<org.iringtools.dxfr.manifest.ClassTemplates>
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ClassTemplates", Namespace="http://www.iringtools.org/dxfr/manifest")]
    public partial class ClassTemplates : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private org.iringtools.dxfr.manifest.Class classField;
        
        private org.iringtools.dxfr.manifest.Templates templatesField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false)]
        public org.iringtools.dxfr.manifest.Class @class
        {
            get
            {
                return this.classField;
            }
            set
            {
                this.classField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false)]
        public org.iringtools.dxfr.manifest.Templates templates
        {
            get
            {
                return this.templatesField;
            }
            set
            {
                this.templatesField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Class", Namespace="http://www.iringtools.org/dxfr/manifest")]
    public partial class Class : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string classIdField;
        
        private string nameField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false)]
        public string classId
        {
            get
            {
                return this.classIdField;
            }
            set
            {
                this.classIdField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false)]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.CollectionDataContractAttribute(Name="Templates", Namespace="http://www.iringtools.org/dxfr/manifest", ItemName="template")]
    public class Templates : System.Collections.Generic.List<org.iringtools.dxfr.manifest.Template>
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Template", Namespace="http://www.iringtools.org/dxfr/manifest")]
    public partial class Template : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string templateIdField;
        
        private string nameField;
        
        private org.iringtools.dxfr.manifest.Roles rolesField;
        
        private org.iringtools.mapping.TransferOption transferOptionField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false)]
        public string templateId
        {
            get
            {
                return this.templateIdField;
            }
            set
            {
                this.templateIdField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false, Order=1)]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false, Order=2)]
        public org.iringtools.dxfr.manifest.Roles roles
        {
            get
            {
                return this.rolesField;
            }
            set
            {
                this.rolesField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=3)]
        public org.iringtools.mapping.TransferOption transferOption
        {
            get
            {
                return this.transferOptionField;
            }
            set
            {
                this.transferOptionField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.CollectionDataContractAttribute(Name="Roles", Namespace="http://www.iringtools.org/dxfr/manifest", ItemName="role")]
    public class Roles : System.Collections.Generic.List<org.iringtools.dxfr.manifest.Role>
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Role", Namespace="http://www.iringtools.org/dxfr/manifest")]
    public partial class Role : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private org.iringtools.mapping.RoleType typeField;
        
        private string roleIdField;
        
        private string nameField;
        
        private string dataTypeField;
        
        private string valueField;
        
        private org.iringtools.dxfr.manifest.Class classField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public org.iringtools.mapping.RoleType type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false, Order=1)]
        public string roleId
        {
            get
            {
                return this.roleIdField;
            }
            set
            {
                this.roleIdField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false, Order=2)]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false, Order=3)]
        public string dataType
        {
            get
            {
                return this.dataTypeField;
            }
            set
            {
                this.dataTypeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false, Order=4)]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false, Order=5)]
        public org.iringtools.dxfr.manifest.Class @class
        {
            get
            {
                return this.classField;
            }
            set
            {
                this.classField = value;
            }
        }
    }
}
namespace org.iringtools.mapping
{
    using System.Runtime.Serialization;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="TransferOption", Namespace="http://www.iringtools.org/mapping")]
    public enum TransferOption : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Desired = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Required = 1,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="RoleType", Namespace="http://www.iringtools.org/mapping")]
    public enum RoleType : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Property = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Possessor = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Reference = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        FixedValue = 3,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        DataProperty = 4,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        ObjectProperty = 5,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Mapping", Namespace="http://www.iringtools.org/mapping")]
    public partial class Mapping : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private org.iringtools.mapping.GraphMaps graphMapsField;
        
        private org.iringtools.mapping.ValueListMaps valueListMapsField;
        
        private string versionField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false)]
        public org.iringtools.mapping.GraphMaps graphMaps
        {
            get
            {
                return this.graphMapsField;
            }
            set
            {
                this.graphMapsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false)]
        public org.iringtools.mapping.ValueListMaps valueListMaps
        {
            get
            {
                return this.valueListMapsField;
            }
            set
            {
                this.valueListMapsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false)]
        public string version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.CollectionDataContractAttribute(Name="GraphMaps", Namespace="http://www.iringtools.org/mapping", ItemName="graphMap")]
    public class GraphMaps : System.Collections.Generic.List<org.iringtools.mapping.GraphMap>
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.CollectionDataContractAttribute(Name="ValueListMaps", Namespace="http://www.iringtools.org/mapping", ItemName="valueListMaps")]
    public class ValueListMaps : System.Collections.Generic.List<org.iringtools.mapping.ValueListMap>
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="GraphMap", Namespace="http://www.iringtools.org/mapping")]
    public partial class GraphMap : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string nameField;
        
        private org.iringtools.mapping.ClassTemplateMapList classTemplateMapsField;
        
        private string dataObjectNameField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false)]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false, Order=1)]
        public org.iringtools.mapping.ClassTemplateMapList classTemplateMaps
        {
            get
            {
                return this.classTemplateMapsField;
            }
            set
            {
                this.classTemplateMapsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false, Order=2)]
        public string dataObjectName
        {
            get
            {
                return this.dataObjectNameField;
            }
            set
            {
                this.dataObjectNameField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.CollectionDataContractAttribute(Name="ClassTemplateMapList", Namespace="http://www.iringtools.org/mapping", ItemName="classTemplateMap")]
    public class ClassTemplateMapList : System.Collections.Generic.List<org.iringtools.mapping.ClassTemplateMap>
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ClassTemplateMap", Namespace="http://www.iringtools.org/mapping")]
    public partial class ClassTemplateMap : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private org.iringtools.mapping.ClassMap classMapField;
        
        private org.iringtools.mapping.TemplateMaps templateMapsField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false)]
        public org.iringtools.mapping.ClassMap classMap
        {
            get
            {
                return this.classMapField;
            }
            set
            {
                this.classMapField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false)]
        public org.iringtools.mapping.TemplateMaps templateMaps
        {
            get
            {
                return this.templateMapsField;
            }
            set
            {
                this.templateMapsField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ClassMap", Namespace="http://www.iringtools.org/mapping")]
    public partial class ClassMap : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string nameField;
        
        private string classIdField;
        
        private string identifierDelimiterField;
        
        private org.iringtools.mapping.Identifiers identifiersField;
        
        private string identifierValueField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false)]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false, Order=1)]
        public string classId
        {
            get
            {
                return this.classIdField;
            }
            set
            {
                this.classIdField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=2)]
        public string identifierDelimiter
        {
            get
            {
                return this.identifierDelimiterField;
            }
            set
            {
                this.identifierDelimiterField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false, Order=3)]
        public org.iringtools.mapping.Identifiers identifiers
        {
            get
            {
                return this.identifiersField;
            }
            set
            {
                this.identifiersField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=4)]
        public string identifierValue
        {
            get
            {
                return this.identifierValueField;
            }
            set
            {
                this.identifierValueField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.CollectionDataContractAttribute(Name="TemplateMaps", Namespace="http://www.iringtools.org/mapping", ItemName="templateMap")]
    public class TemplateMaps : System.Collections.Generic.List<org.iringtools.mapping.TemplateMap>
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.CollectionDataContractAttribute(Name="Identifiers", Namespace="http://www.iringtools.org/mapping", ItemName="identifier")]
    public class Identifiers : System.Collections.Generic.List<string>
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="TemplateMap", Namespace="http://www.iringtools.org/mapping")]
    public partial class TemplateMap : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string templateIdField;
        
        private org.iringtools.mapping.TemplateType templateTypeField;
        
        private string nameField;
        
        private org.iringtools.mapping.RoleMaps roleMapsField;
        
        private org.iringtools.mapping.TransferOption transferOptionField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false)]
        public string templateId
        {
            get
            {
                return this.templateIdField;
            }
            set
            {
                this.templateIdField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public org.iringtools.mapping.TemplateType templateType
        {
            get
            {
                return this.templateTypeField;
            }
            set
            {
                this.templateTypeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false, Order=2)]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false, Order=3)]
        public org.iringtools.mapping.RoleMaps roleMaps
        {
            get
            {
                return this.roleMapsField;
            }
            set
            {
                this.roleMapsField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=4)]
        public org.iringtools.mapping.TransferOption transferOption
        {
            get
            {
                return this.transferOptionField;
            }
            set
            {
                this.transferOptionField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="TemplateType", Namespace="http://www.iringtools.org/mapping")]
    public enum TemplateType : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Qualification = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Definition = 1,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.CollectionDataContractAttribute(Name="RoleMaps", Namespace="http://www.iringtools.org/mapping", ItemName="roleMap")]
    public class RoleMaps : System.Collections.Generic.List<org.iringtools.mapping.RoleMap>
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="RoleMap", Namespace="http://www.iringtools.org/mapping")]
    public partial class RoleMap : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private org.iringtools.mapping.RoleType typeField;
        
        private string roleIdField;
        
        private string nameField;
        
        private string dataTypeField;
        
        private string valueField;
        
        private string propertyNameField;
        
        private string valueListNameField;
        
        private org.iringtools.mapping.ClassMap classMapField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public org.iringtools.mapping.RoleType type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false, Order=1)]
        public string roleId
        {
            get
            {
                return this.roleIdField;
            }
            set
            {
                this.roleIdField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false, Order=2)]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=3)]
        public string dataType
        {
            get
            {
                return this.dataTypeField;
            }
            set
            {
                this.dataTypeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=4)]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=5)]
        public string propertyName
        {
            get
            {
                return this.propertyNameField;
            }
            set
            {
                this.propertyNameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=6)]
        public string valueListName
        {
            get
            {
                return this.valueListNameField;
            }
            set
            {
                this.valueListNameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=7)]
        public org.iringtools.mapping.ClassMap classMap
        {
            get
            {
                return this.classMapField;
            }
            set
            {
                this.classMapField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ValueListMap", Namespace="http://www.iringtools.org/mapping")]
    public partial class ValueListMap : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string nameField;
        
        private org.iringtools.mapping.ValueMaps valueMapsField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false)]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false)]
        public org.iringtools.mapping.ValueMaps valueMaps
        {
            get
            {
                return this.valueMapsField;
            }
            set
            {
                this.valueMapsField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.CollectionDataContractAttribute(Name="ValueMaps", Namespace="http://www.iringtools.org/mapping", ItemName="valueMap")]
    public class ValueMaps : System.Collections.Generic.List<org.iringtools.mapping.ValueMap>
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ValueMap", Namespace="http://www.iringtools.org/mapping")]
    public partial class ValueMap : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string internalValueField;
        
        private string uriField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false)]
        public string internalValue
        {
            get
            {
                return this.internalValueField;
            }
            set
            {
                this.internalValueField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, EmitDefaultValue=false)]
        public string uri
        {
            get
            {
                return this.uriField;
            }
            set
            {
                this.uriField = value;
            }
        }
    }
}
