using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace org.iringtools.adapter.dataLayer
{
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  public partial class TUsing
  {

    private TDocumentation documentationField;

    private System.Xml.Linq.XElement[] anyField;

    private string namespaceField;

    private string aliasField;

    private System.Xml.XmlAttribute[] anyAttrField;

    /// <remarks/>
    public TDocumentation Documentation
    {
      get
      {
        return this.documentationField;
      }
      set
      {
        this.documentationField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute()]
    public System.Xml.Linq.XElement[] Any
    {
      get
      {
        return this.anyField;
      }
      set
      {
        this.anyField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Namespace
    {
      get
      {
        return this.namespaceField;
      }
      set
      {
        this.namespaceField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Alias
    {
      get
      {
        return this.aliasField;
      }
      set
      {
        this.aliasField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyAttributeAttribute()]
    public System.Xml.XmlAttribute[] AnyAttr
    {
      get
      {
        return this.anyAttrField;
      }
      set
      {
        this.anyAttrField = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  public partial class TDocumentation
  {

    private System.Xml.Linq.XElement summaryField;

    private System.Xml.Linq.XElement longDescriptionField;

    private System.Xml.XmlAttribute[] anyAttrField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute(Name = "Summary", Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
    public System.Xml.Linq.XElement Summary
    {
      get
      {
        return this.summaryField;
      }
      set
      {
        this.summaryField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute(Name = "LongDescription", Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
    public System.Xml.Linq.XElement LongDescription
    {
      get
      {
        return this.longDescriptionField;
      }
      set
      {
        this.longDescriptionField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyAttributeAttribute()]
    public System.Xml.XmlAttribute[] AnyAttr
    {
      get
      {
        return this.anyAttrField;
      }
      set
      {
        this.anyAttrField = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  public partial class TParameter
  {

    private TDocumentation documentationField;

    private System.Xml.Linq.XElement[] anyField;

    private string nameField;

    private string typeField;

    private TParameterMode modeField;

    private bool modeFieldSpecified;

    private string maxLengthField;

    private string precisionField;

    private string scaleField;

    private System.Xml.XmlAttribute[] anyAttrField;

    /// <remarks/>
    public TDocumentation Documentation
    {
      get
      {
        return this.documentationField;
      }
      set
      {
        this.documentationField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute()]
    public System.Xml.Linq.XElement[] Any
    {
      get
      {
        return this.anyField;
      }
      set
      {
        this.anyField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Name
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

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Type
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

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public TParameterMode Mode
    {
      get
      {
        return this.modeField;
      }
      set
      {
        this.modeField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool ModeSpecified
    {
      get
      {
        return this.modeFieldSpecified;
      }
      set
      {
        this.modeFieldSpecified = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string MaxLength
    {
      get
      {
        return this.maxLengthField;
      }
      set
      {
        this.maxLengthField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
    public string Precision
    {
      get
      {
        return this.precisionField;
      }
      set
      {
        this.precisionField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
    public string Scale
    {
      get
      {
        return this.scaleField;
      }
      set
      {
        this.scaleField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyAttributeAttribute()]
    public System.Xml.XmlAttribute[] AnyAttr
    {
      get
      {
        return this.anyAttrField;
      }
      set
      {
        this.anyAttrField = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  public enum TParameterMode
  {

    /// <remarks/>
    In,

    /// <remarks/>
    Out,

    /// <remarks/>
    InOut,
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  public partial class TFunction
  {

    private TDocumentation documentationField;

    private string commandTextField;

    private TParameter[] parameterField;

    private System.Xml.Linq.XElement[] anyField;

    private string nameField;

    private string returnTypeField;

    private bool aggregateField;

    private bool aggregateFieldSpecified;

    private bool builtInField;

    private bool builtInFieldSpecified;

    private string storeFunctionNameField;

    private bool niladicFunctionField;

    private bool niladicFunctionFieldSpecified;

    private bool isComposableField;

    private TParameterTypeSemantics parameterTypeSemanticsField;

    private string schemaField;

    private string schema1Field;

    private string name1Field;

    private System.Xml.XmlAttribute[] anyAttrField;

    public TFunction()
    {
      this.isComposableField = true;
      this.parameterTypeSemanticsField = TParameterTypeSemantics.AllowImplicitConversion;
    }

    /// <remarks/>
    public TDocumentation Documentation
    {
      get
      {
        return this.documentationField;
      }
      set
      {
        this.documentationField = value;
      }
    }

    /// <remarks/>
    public string CommandText
    {
      get
      {
        return this.commandTextField;
      }
      set
      {
        this.commandTextField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Parameter")]
    public TParameter[] Parameter
    {
      get
      {
        return this.parameterField;
      }
      set
      {
        this.parameterField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute()]
    public System.Xml.Linq.XElement[] Any
    {
      get
      {
        return this.anyField;
      }
      set
      {
        this.anyField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Name
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

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ReturnType
    {
      get
      {
        return this.returnTypeField;
      }
      set
      {
        this.returnTypeField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool Aggregate
    {
      get
      {
        return this.aggregateField;
      }
      set
      {
        this.aggregateField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool AggregateSpecified
    {
      get
      {
        return this.aggregateFieldSpecified;
      }
      set
      {
        this.aggregateFieldSpecified = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool BuiltIn
    {
      get
      {
        return this.builtInField;
      }
      set
      {
        this.builtInField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool BuiltInSpecified
    {
      get
      {
        return this.builtInFieldSpecified;
      }
      set
      {
        this.builtInFieldSpecified = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string StoreFunctionName
    {
      get
      {
        return this.storeFunctionNameField;
      }
      set
      {
        this.storeFunctionNameField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool NiladicFunction
    {
      get
      {
        return this.niladicFunctionField;
      }
      set
      {
        this.niladicFunctionField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool NiladicFunctionSpecified
    {
      get
      {
        return this.niladicFunctionFieldSpecified;
      }
      set
      {
        this.niladicFunctionFieldSpecified = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(true)]
    public bool IsComposable
    {
      get
      {
        return this.isComposableField;
      }
      set
      {
        this.isComposableField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(TParameterTypeSemantics.AllowImplicitConversion)]
    public TParameterTypeSemantics ParameterTypeSemantics
    {
      get
      {
        return this.parameterTypeSemanticsField;
      }
      set
      {
        this.parameterTypeSemanticsField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Schema
    {
      get
      {
        return this.schemaField;
      }
      set
      {
        this.schemaField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute("Schema", Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator")]
    public string Schema1
    {
      get
      {
        return this.schema1Field;
      }
      set
      {
        this.schema1Field = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute("Name", Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator")]
    public string Name1
    {
      get
      {
        return this.name1Field;
      }
      set
      {
        this.name1Field = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyAttributeAttribute()]
    public System.Xml.XmlAttribute[] AnyAttr
    {
      get
      {
        return this.anyAttrField;
      }
      set
      {
        this.anyAttrField = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  public enum TParameterTypeSemantics
  {

    /// <remarks/>
    ExactMatchOnly,

    /// <remarks/>
    AllowImplicitPromotion,

    /// <remarks/>
    AllowImplicitConversion,
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  public partial class TNavigationProperty
  {

    private TDocumentation documentationField;

    private System.Xml.Linq.XElement[] anyField;

    private string nameField;

    private string relationshipField;

    private string toRoleField;

    private string fromRoleField;

    private System.Xml.XmlAttribute[] anyAttrField;

    /// <remarks/>
    public TDocumentation Documentation
    {
      get
      {
        return this.documentationField;
      }
      set
      {
        this.documentationField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute()]
    public System.Xml.Linq.XElement[] Any
    {
      get
      {
        return this.anyField;
      }
      set
      {
        this.anyField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Name
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

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Relationship
    {
      get
      {
        return this.relationshipField;
      }
      set
      {
        this.relationshipField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ToRole
    {
      get
      {
        return this.toRoleField;
      }
      set
      {
        this.toRoleField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string FromRole
    {
      get
      {
        return this.fromRoleField;
      }
      set
      {
        this.fromRoleField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyAttributeAttribute()]
    public System.Xml.XmlAttribute[] AnyAttr
    {
      get
      {
        return this.anyAttrField;
      }
      set
      {
        this.anyAttrField = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  public partial class TEntityProperty
  {

    private TDocumentation documentationField;

    private System.Xml.Linq.XElement[] anyField;

    private string nameField;

    private string typeField;

    private bool nullableField;

    private string defaultValueField;

    private string maxLengthField;

    private bool fixedLengthField;

    private bool fixedLengthFieldSpecified;

    private string precisionField;

    private string scaleField;

    private bool unicodeField;

    private bool unicodeFieldSpecified;

    private string collationField;

    private TStoreGeneratedPattern storeGeneratedPatternField;

    private bool storeGeneratedPatternFieldSpecified;

    private System.Xml.XmlAttribute[] anyAttrField;

    public TEntityProperty()
    {
      this.nullableField = true;
    }

    /// <remarks/>
    public TDocumentation Documentation
    {
      get
      {
        return this.documentationField;
      }
      set
      {
        this.documentationField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute()]
    public System.Xml.Linq.XElement[] Any
    {
      get
      {
        return this.anyField;
      }
      set
      {
        this.anyField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Name
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

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Type
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

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    [System.ComponentModel.DefaultValueAttribute(true)]
    public bool Nullable
    {
      get
      {
        return this.nullableField;
      }
      set
      {
        this.nullableField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string DefaultValue
    {
      get
      {
        return this.defaultValueField;
      }
      set
      {
        this.defaultValueField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string MaxLength
    {
      get
      {
        return this.maxLengthField;
      }
      set
      {
        this.maxLengthField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool FixedLength
    {
      get
      {
        return this.fixedLengthField;
      }
      set
      {
        this.fixedLengthField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool FixedLengthSpecified
    {
      get
      {
        return this.fixedLengthFieldSpecified;
      }
      set
      {
        this.fixedLengthFieldSpecified = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
    public string Precision
    {
      get
      {
        return this.precisionField;
      }
      set
      {
        this.precisionField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
    public string Scale
    {
      get
      {
        return this.scaleField;
      }
      set
      {
        this.scaleField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool Unicode
    {
      get
      {
        return this.unicodeField;
      }
      set
      {
        this.unicodeField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool UnicodeSpecified
    {
      get
      {
        return this.unicodeFieldSpecified;
      }
      set
      {
        this.unicodeFieldSpecified = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Collation
    {
      get
      {
        return this.collationField;
      }
      set
      {
        this.collationField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public TStoreGeneratedPattern StoreGeneratedPattern
    {
      get
      {
        return this.storeGeneratedPatternField;
      }
      set
      {
        this.storeGeneratedPatternField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool StoreGeneratedPatternSpecified
    {
      get
      {
        return this.storeGeneratedPatternFieldSpecified;
      }
      set
      {
        this.storeGeneratedPatternFieldSpecified = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyAttributeAttribute()]
    public System.Xml.XmlAttribute[] AnyAttr
    {
      get
      {
        return this.anyAttrField;
      }
      set
      {
        this.anyAttrField = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  public enum TStoreGeneratedPattern
  {

    /// <remarks/>
    None,

    /// <remarks/>
    Identity,

    /// <remarks/>
    Computed,
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  public partial class TEntityKeyElement
  {

    private TPropertyRef[] propertyRefField;

    private System.Xml.Linq.XElement[] anyField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("PropertyRef")]
    public TPropertyRef[] PropertyRef
    {
      get
      {
        return this.propertyRefField;
      }
      set
      {
        this.propertyRefField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute()]
    public System.Xml.Linq.XElement[] Any
    {
      get
      {
        return this.anyField;
      }
      set
      {
        this.anyField = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  public partial class TPropertyRef : IEquatable<TPropertyRef>
  {

    private TDocumentation documentationField;

    private System.Xml.Linq.XElement[] anyField;

    private string nameField;

    private System.Xml.XmlAttribute[] anyAttrField;

    /// <remarks/>
    public TDocumentation Documentation
    {
      get
      {
        return this.documentationField;
      }
      set
      {
        this.documentationField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute()]
    public System.Xml.Linq.XElement[] Any
    {
      get
      {
        return this.anyField;
      }
      set
      {
        this.anyField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Name
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

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyAttributeAttribute()]
    public System.Xml.XmlAttribute[] AnyAttr
    {
      get
      {
        return this.anyAttrField;
      }
      set
      {
        this.anyAttrField = value;
      }
    }

    public bool Equals(TPropertyRef other)
    {
      if (Name == other.Name)
        return true;
      else if (other.Name == "*")
        return true;
      else
        return false;
    } 
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  public partial class TEntityType
  {

    private TDocumentation documentationField;

    private TEntityKeyElement keyField;

    private object[] itemsField;

    private System.Xml.Linq.XElement[] anyField;

    private string nameField;

    private System.Xml.XmlAttribute[] anyAttrField;

    /// <remarks/>
    public TDocumentation Documentation
    {
      get
      {
        return this.documentationField;
      }
      set
      {
        this.documentationField = value;
      }
    }

    /// <remarks/>
    public TEntityKeyElement Key
    {
      get
      {
        return this.keyField;
      }
      set
      {
        this.keyField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("NavigationProperty", typeof(TNavigationProperty))]
    [System.Xml.Serialization.XmlElementAttribute("Property", typeof(TEntityProperty))]
    public object[] Items
    {
      get
      {
        return this.itemsField;
      }
      set
      {
        this.itemsField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute()]
    public System.Xml.Linq.XElement[] Any
    {
      get
      {
        return this.anyField;
      }
      set
      {
        this.anyField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Name
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

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyAttributeAttribute()]
    public System.Xml.XmlAttribute[] AnyAttr
    {
      get
      {
        return this.anyAttrField;
      }
      set
      {
        this.anyAttrField = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  public partial class TReferentialConstraintRoleElement
  {

    private TDocumentation documentationField;

    private TPropertyRef[] propertyRefField;

    private System.Xml.Linq.XElement[] anyField;

    private string roleField;

    private System.Xml.XmlAttribute[] anyAttrField;

    /// <remarks/>
    public TDocumentation Documentation
    {
      get
      {
        return this.documentationField;
      }
      set
      {
        this.documentationField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("PropertyRef")]
    public TPropertyRef[] PropertyRef
    {
      get
      {
        return this.propertyRefField;
      }
      set
      {
        this.propertyRefField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute()]
    public System.Xml.Linq.XElement[] Any
    {
      get
      {
        return this.anyField;
      }
      set
      {
        this.anyField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Role
    {
      get
      {
        return this.roleField;
      }
      set
      {
        this.roleField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyAttributeAttribute()]
    public System.Xml.XmlAttribute[] AnyAttr
    {
      get
      {
        return this.anyAttrField;
      }
      set
      {
        this.anyAttrField = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  public partial class TConstraint
  {

    private TDocumentation documentationField;

    private TReferentialConstraintRoleElement principalField;

    private TReferentialConstraintRoleElement dependentField;

    private System.Xml.Linq.XElement[] anyField;

    private System.Xml.XmlAttribute[] anyAttrField;

    /// <remarks/>
    public TDocumentation Documentation
    {
      get
      {
        return this.documentationField;
      }
      set
      {
        this.documentationField = value;
      }
    }

    /// <remarks/>
    public TReferentialConstraintRoleElement Principal
    {
      get
      {
        return this.principalField;
      }
      set
      {
        this.principalField = value;
      }
    }

    /// <remarks/>
    public TReferentialConstraintRoleElement Dependent
    {
      get
      {
        return this.dependentField;
      }
      set
      {
        this.dependentField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute()]
    public System.Xml.Linq.XElement[] Any
    {
      get
      {
        return this.anyField;
      }
      set
      {
        this.anyField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyAttributeAttribute()]
    public System.Xml.XmlAttribute[] AnyAttr
    {
      get
      {
        return this.anyAttrField;
      }
      set
      {
        this.anyAttrField = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  public partial class TOnAction
  {

    private TDocumentation documentationField;

    private System.Xml.Linq.XElement[] anyField;

    private TAction actionField;

    private System.Xml.XmlAttribute[] anyAttrField;

    /// <remarks/>
    public TDocumentation Documentation
    {
      get
      {
        return this.documentationField;
      }
      set
      {
        this.documentationField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute()]
    public System.Xml.Linq.XElement[] Any
    {
      get
      {
        return this.anyField;
      }
      set
      {
        this.anyField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public TAction Action
    {
      get
      {
        return this.actionField;
      }
      set
      {
        this.actionField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyAttributeAttribute()]
    public System.Xml.XmlAttribute[] AnyAttr
    {
      get
      {
        return this.anyAttrField;
      }
      set
      {
        this.anyAttrField = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  public enum TAction
  {

    /// <remarks/>
    Cascade,

    /// <remarks/>
    Restrict,

    /// <remarks/>
    None,
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  public partial class TAssociationEnd
  {

    private TDocumentation documentationField;

    private TOnAction[] onDeleteField;

    private System.Xml.Linq.XElement[] anyField;

    private string typeField;

    private string roleField;

    private TMultiplicity multiplicityField;

    private System.Xml.XmlAttribute[] anyAttrField;

    /// <remarks/>
    public TDocumentation Documentation
    {
      get
      {
        return this.documentationField;
      }
      set
      {
        this.documentationField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("OnDelete")]
    public TOnAction[] OnDelete
    {
      get
      {
        return this.onDeleteField;
      }
      set
      {
        this.onDeleteField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute()]
    public System.Xml.Linq.XElement[] Any
    {
      get
      {
        return this.anyField;
      }
      set
      {
        this.anyField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Type
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

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Role
    {
      get
      {
        return this.roleField;
      }
      set
      {
        this.roleField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public TMultiplicity Multiplicity
    {
      get
      {
        return this.multiplicityField;
      }
      set
      {
        this.multiplicityField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyAttributeAttribute()]
    public System.Xml.XmlAttribute[] AnyAttr
    {
      get
      {
        return this.anyAttrField;
      }
      set
      {
        this.anyAttrField = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  public enum TMultiplicity
  {

    /// <remarks/>
    [System.Xml.Serialization.XmlEnumAttribute("0..1")]
    Item01,

    /// <remarks/>
    [System.Xml.Serialization.XmlEnumAttribute("1")]
    Item1,

    /// <remarks/>
    [System.Xml.Serialization.XmlEnumAttribute("*")]
    Item,
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  public partial class TAssociation
  {

    private TDocumentation documentationField;

    private TAssociationEnd[] endField;

    private TConstraint referentialConstraintField;

    private System.Xml.Linq.XElement[] anyField;

    private string nameField;

    private System.Xml.XmlAttribute[] anyAttrField;

    /// <remarks/>
    public TDocumentation Documentation
    {
      get
      {
        return this.documentationField;
      }
      set
      {
        this.documentationField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("End")]
    public TAssociationEnd[] End
    {
      get
      {
        return this.endField;
      }
      set
      {
        this.endField = value;
      }
    }

    /// <remarks/>
    public TConstraint ReferentialConstraint
    {
      get
      {
        return this.referentialConstraintField;
      }
      set
      {
        this.referentialConstraintField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute()]
    public System.Xml.Linq.XElement[] Any
    {
      get
      {
        return this.anyField;
      }
      set
      {
        this.anyField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Name
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

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyAttributeAttribute()]
    public System.Xml.XmlAttribute[] AnyAttr
    {
      get
      {
        return this.anyAttrField;
      }
      set
      {
        this.anyAttrField = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  [System.Xml.Serialization.XmlRootAttribute("Schema", Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl", IsNullable = false)]
  public partial class TSchema
  {

    private TAssociation[] associationField;

    private TEntityType[] entityTypeField;

    private TEntityContainer[] entityContainerField;

    private TFunction[] functionField;

    private System.Xml.Linq.XElement[] anyField;

    private string namespaceField;

    private string aliasField;

    private string providerField;

    private string providerManifestTokenField;

    private System.Xml.XmlAttribute[] anyAttrField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Association")]
    public TAssociation[] Association
    {
      get
      {
        return this.associationField;
      }
      set
      {
        this.associationField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("EntityType")]
    public TEntityType[] EntityType
    {
      get
      {
        return this.entityTypeField;
      }
      set
      {
        this.entityTypeField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("EntityContainer")]
    public TEntityContainer[] EntityContainer
    {
      get
      {
        return this.entityContainerField;
      }
      set
      {
        this.entityContainerField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Function")]
    public TFunction[] Function
    {
      get
      {
        return this.functionField;
      }
      set
      {
        this.functionField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute()]
    public System.Xml.Linq.XElement[] Any
    {
      get
      {
        return this.anyField;
      }
      set
      {
        this.anyField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Namespace
    {
      get
      {
        return this.namespaceField;
      }
      set
      {
        this.namespaceField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Alias
    {
      get
      {
        return this.aliasField;
      }
      set
      {
        this.aliasField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Provider
    {
      get
      {
        return this.providerField;
      }
      set
      {
        this.providerField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ProviderManifestToken
    {
      get
      {
        return this.providerManifestTokenField;
      }
      set
      {
        this.providerManifestTokenField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyAttributeAttribute()]
    public System.Xml.XmlAttribute[] AnyAttr
    {
      get
      {
        return this.anyAttrField;
      }
      set
      {
        this.anyAttrField = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl", IsNullable = false)]
  public partial class TEntityContainer
  {

    private TDocumentation documentationField;

    private object[] itemsField;

    private string nameField;

    /// <remarks/>
    public TDocumentation Documentation
    {
      get
      {
        return this.documentationField;
      }
      set
      {
        this.documentationField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("AssociationSet", typeof(EntityContainerAssociationSet))]
    [System.Xml.Serialization.XmlElementAttribute("EntitySet", typeof(EntityContainerEntitySet))]
    public object[] Items
    {
      get
      {
        return this.itemsField;
      }
      set
      {
        this.itemsField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Name
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
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  public partial class EntityContainerAssociationSet
  {

    private TDocumentation documentationField;

    private EntityContainerAssociationSetEnd[] endField;

    private System.Xml.Linq.XElement[] anyField;

    private string nameField;

    private string associationField;

    private System.Xml.XmlAttribute[] anyAttrField;

    /// <remarks/>
    public TDocumentation Documentation
    {
      get
      {
        return this.documentationField;
      }
      set
      {
        this.documentationField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("End")]
    public EntityContainerAssociationSetEnd[] End
    {
      get
      {
        return this.endField;
      }
      set
      {
        this.endField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute()]
    public System.Xml.Linq.XElement[] Any
    {
      get
      {
        return this.anyField;
      }
      set
      {
        this.anyField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Name
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

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Association
    {
      get
      {
        return this.associationField;
      }
      set
      {
        this.associationField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyAttributeAttribute()]
    public System.Xml.XmlAttribute[] AnyAttr
    {
      get
      {
        return this.anyAttrField;
      }
      set
      {
        this.anyAttrField = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  public partial class EntityContainerAssociationSetEnd
  {

    private TDocumentation documentationField;

    private System.Xml.Linq.XElement[] anyField;

    private string roleField;

    private string entitySetField;

    /// <remarks/>
    public TDocumentation Documentation
    {
      get
      {
        return this.documentationField;
      }
      set
      {
        this.documentationField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute()]
    public System.Xml.Linq.XElement[] Any
    {
      get
      {
        return this.anyField;
      }
      set
      {
        this.anyField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Role
    {
      get
      {
        return this.roleField;
      }
      set
      {
        this.roleField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string EntitySet
    {
      get
      {
        return this.entitySetField;
      }
      set
      {
        this.entitySetField = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl")]
  public partial class EntityContainerEntitySet
  {

    private TDocumentation documentationField;

    private string definingQueryField;

    private System.Xml.Linq.XElement[] anyField;

    private string nameField;

    private string entityTypeField;

    private string schemaField;

    private string tableField;

    private TSourceType typeField;

    private bool typeFieldSpecified;

    private string schema1Field;

    private string name1Field;

    private System.Xml.XmlAttribute[] anyAttrField;

    /// <remarks/>
    public TDocumentation Documentation
    {
      get
      {
        return this.documentationField;
      }
      set
      {
        this.documentationField = value;
      }
    }

    /// <remarks/>
    public string DefiningQuery
    {
      get
      {
        return this.definingQueryField;
      }
      set
      {
        this.definingQueryField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyElementAttribute()]
    public System.Xml.Linq.XElement[] Any
    {
      get
      {
        return this.anyField;
      }
      set
      {
        this.anyField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Name
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

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string EntityType
    {
      get
      {
        return this.entityTypeField;
      }
      set
      {
        this.entityTypeField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Schema
    {
      get
      {
        return this.schemaField;
      }
      set
      {
        this.schemaField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Table
    {
      get
      {
        return this.tableField;
      }
      set
      {
        this.tableField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator")]
    public TSourceType Type
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

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool TypeSpecified
    {
      get
      {
        return this.typeFieldSpecified;
      }
      set
      {
        this.typeFieldSpecified = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute("Schema", Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator")]
    public string Schema1
    {
      get
      {
        return this.schema1Field;
      }
      set
      {
        this.schema1Field = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute("Name", Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator")]
    public string Name1
    {
      get
      {
        return this.name1Field;
      }
      set
      {
        this.name1Field = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAnyAttributeAttribute()]
    public System.Xml.XmlAttribute[] AnyAttr
    {
      get
      {
        return this.anyAttrField;
      }
      set
      {
        this.anyAttrField = value;
      }
    }
  }
  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator")]
  public enum TSourceType
  {

    /// <remarks/>
    Tables,

    /// <remarks/>
    Views,
  }

}
