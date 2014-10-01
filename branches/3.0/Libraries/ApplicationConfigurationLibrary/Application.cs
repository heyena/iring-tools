using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using org.iringtools.UserSecurity;

namespace org.iringtools.applicationConfig
{
    /// <summary>
    /// These below defined classes are corresponding to the Application context tables.
    /// </summary>
    
    [CollectionDataContract(Name = "contexts", Namespace = "http://www.iringtools.org/library", ItemName = "context")]
    public class Contexts : List<Context>
    {

    }

    [DataContract(Name = "context", Namespace = "http://www.iringtools.org/library")]
    public class Context
    {
        Context()
        {
            permissions = new Permissions();
        }

        [DataMember(Name = "contextId", Order = 0)]
        public Guid ContextId { get; set; }

        [DataMember(Name = "displayName", Order = 1, EmitDefaultValue = false)]
        public string DisplayName { get; set; }

        [DataMember(Name = "internalName", Order = 2, EmitDefaultValue = false)]
        public string InternalName { get; set; }

        [DataMember(Name = "description", Order = 3, EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(Name = "cacheConnstr", Order = 4, EmitDefaultValue = false)]
        public string CacheConnStr { get; set; }

        [DataMember(Name = "siteId", Order = 5, EmitDefaultValue = false)]
        public int SiteId { get; set; }

        [DataMember(Name = "active", Order = 6, EmitDefaultValue = false)]
        public Byte Active { get; set; }

        [DataMember(Name = "folderId", Order = 7, EmitDefaultValue = false)]
        public Guid FolderId { get; set; }

        [DataMember(Name = "permissions", Order = 8, EmitDefaultValue = false)]
        public Permissions permissions { get; set; }
    }


    [CollectionDataContract(Name = "applications", Namespace = "http://www.iringtools.org/library", ItemName = "application")]
    public class Applications : List<Application>
    {

    }

    [DataContract(Name = "application", Namespace = "http://www.iringtools.org/library")]
    public class Application
    {
        Application()
        {
            permissions = new Permissions();
            applicationSettings = new ApplicationSettings();
        }

        [DataMember(Name = "contextId", Order = 0)]
        public Guid ContextId { get; set; }

        [DataMember(Name = "applicationId", Order = 1, EmitDefaultValue = false)]
        public Guid ApplicationId { get; set; }

        [DataMember(Name = "displayName", Order = 2, EmitDefaultValue = false)]
        public string DisplayName { get; set; }

        [DataMember(Name = "internalName", Order = 3, EmitDefaultValue = false)]
        public string InternalName { get; set; }

        [DataMember(Name = "description", Order = 4, EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(Name = "dxfrUrl", Order = 5, EmitDefaultValue = false)]
        public string DXFRUrl { get; set; }

        [DataMember(Name = "siteId", Order = 6, EmitDefaultValue = false)]
        public int SiteId { get; set; }
        
        [DataMember(Name = "active", Order = 7, EmitDefaultValue = false)]
        public Byte Active { get; set; }

        [DataMember(Name = "assembly", Order = 8, EmitDefaultValue = false)]
        public string Assembly { get; set; }

        [DataMember(Name = "permissions", Order = 9, EmitDefaultValue = false)]
        public Permissions permissions { get; set; }

        [DataMember(Name = "applicationSettings", Order = 10, EmitDefaultValue = false)]
        public ApplicationSettings applicationSettings { get; set; }
    }


    [CollectionDataContract(Name = "graphs", Namespace = "http://www.iringtools.org/library", ItemName = "graph")]
    public class Graphs : List<Graph>
    {

    }

    [DataContract(Name = "graph", Namespace = "http://www.iringtools.org/library")]
    public class Graph
    {
        Graph()
        {
            permissions = new Permissions();
        }

        [DataMember(Name = "applicationId", Order = 0)]
        public Guid ApplicationId { get; set; }

        [DataMember(Name = "graphId", Order = 1, EmitDefaultValue = false)]
        public Guid GraphId { get; set; }

        [DataMember(Name = "graphName", Order = 2, EmitDefaultValue = false)]
        public string GraphName { get; set; }

        [DataMember(Name = "graphObject", Order = 3, EmitDefaultValue = false)]
        public byte[] graph { get; set; }

        [DataMember(Name = "siteId", Order = 4, EmitDefaultValue = false)]
        public int SiteId { get; set; }

        [DataMember(Name = "active", Order = 5, EmitDefaultValue = false)]
        public Byte Active { get; set; }

        [DataMember(Name = "permissions", Order = 6, EmitDefaultValue = false)]
        public Permissions permissions { get; set; }
    }


    [CollectionDataContract(Name = "folders", Namespace = "http://www.iringtools.org/library", ItemName = "folder")]
    public class Folders : List<Folder>
    {

    }

    [DataContract(Name = "folder", Namespace = "http://www.iringtools.org/library")]
    public class Folder
    {
        Folder()
        {
            permissions = new Permissions();
        }

        [DataMember(Name = "folderId", Order = 0)]
        public Guid FolderId { get; set; }

        [DataMember(Name = "parentFolderId", Order = 1, EmitDefaultValue = false)]
        public Guid ParentFolderId { get; set; }

        [DataMember(Name = "folderName", Order = 2, EmitDefaultValue = false)]
        public string FolderName { get; set; }

        [DataMember(Name = "siteId", Order = 3, EmitDefaultValue = false)]
        public int SiteId { get; set; }

        [DataMember(Name = "active", Order = 4, EmitDefaultValue = false)]
        public Byte Active { get; set; }

        [DataMember(Name = "permissions", Order = 5, EmitDefaultValue = false)]
        public Permissions permissions { get; set; }
    }


    [CollectionDataContract(Name = "exchanges", Namespace = "http://www.iringtools.org/library", ItemName = "exchange")]
    public class Exchanges : List<Exchange>
    {

    }

    [DataContract(Name = "exchange", Namespace = "http://www.iringtools.org/library")]
    public class Exchange
    {
        Exchange()
        {
            permissions = new Permissions();
        }

        [DataMember(Name = "exchangeId", Order = 0)]
        public Guid ExchangeId { get; set; }

        [DataMember(Name = "commodityId", Order = 1)]
        public Guid CommodityId { get; set; }

        [DataMember(Name = "sourceGraphId", Order = 2, EmitDefaultValue = false)]
        public Guid SourceGraphId { get; set; }

        [DataMember(Name = "destinationGraphId", Order = 3, EmitDefaultValue = false)]
        public Guid DestinationGraphId { get; set; }

        [DataMember(Name = "name", Order = 4, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(Name = "description", Order = 5, EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(Name = "poolSize", Order = 6, EmitDefaultValue = false)]
        public int PoolSize { get; set; }

        [DataMember(Name = "xtypeAdd", Order = 7, EmitDefaultValue = false)]
        public string XTypeAdd { get; set; }

        [DataMember(Name = "xtypeChange", Order = 8, EmitDefaultValue = false)]
        public string XTypeChange { get; set; }

        [DataMember(Name = "xtypeSync", Order = 9, EmitDefaultValue = false)]
        public string XTypeSync { get; set; }

        [DataMember(Name = "xtypeDelete", Order = 10, EmitDefaultValue = false)]
        public string XTypeDelete { get; set; }

        [DataMember(Name = "xtypeSetNull", Order = 11, EmitDefaultValue = false)]
        public string XTypeSetNull { get; set; }

        [DataMember(Name = "siteId", Order = 12, EmitDefaultValue = false)]
        public int SiteId { get; set; }

        [DataMember(Name = "active", Order = 13, EmitDefaultValue = false)]
        public Byte Active { get; set; }

        [DataMember(Name = "permissions", Order = 14, EmitDefaultValue = false)]
        public Permissions permissions { get; set; }
    }


    /// <summary>
    /// These below defined classes are corresponding to the Application access tables.
    /// </summary>
    
    [CollectionDataContract(Name = "resourceGroups", Namespace = "http://www.iringtools.org/library", ItemName = "resourceGroup")]
    public class ResourceGroups : List<ResourceGroup>
    {

    }

    [DataContract(Name = "resourceGroup", Namespace = "http://www.iringtools.org/library")]
    public class ResourceGroup
    {
        [DataMember(Name = "resourceId", Order = 0)]
        public Guid ResourceId { get; set; }

        [DataMember(Name = "groupId", Order = 1, EmitDefaultValue = false)]
        public int GroupId { get; set; }

        [DataMember(Name = "siteId", Order = 2, EmitDefaultValue = false)]
        public int SiteId { get; set; }

        [DataMember(Name = "active", Order = 3, EmitDefaultValue = false)]
        public Byte Active { get; set; }
    }


    [CollectionDataContract(Name = "commodities", Namespace = "http://www.iringtools.org/library", ItemName = "commodity")]
    public class Commodities : List<Commodity>
    {

    }

    [DataContract(Name = "commodity", Namespace = "http://www.iringtools.org/library")]
    public class Commodity
    {
        Commodity()
        {
            permissions = new Permissions();
            groups = new Groups();
        }

        [DataMember(Name = "contextId", Order = 0)]
        public Guid ContextId { get; set; }

        [DataMember(Name = "commodityId", Order = 1, EmitDefaultValue = false)]
        public Guid CommodityId { get; set; }

        [DataMember(Name = "commodityName", Order = 2, EmitDefaultValue = false)]
        public string CommodityName { get; set; }

        [DataMember(Name = "siteId", Order = 3, EmitDefaultValue = false)]
        public int SiteId { get; set; }

        [DataMember(Name = "active", Order = 4, EmitDefaultValue = false)]
        public Byte Active { get; set; }

        [DataMember(Name = "permissions", Order = 5, EmitDefaultValue = false)]
        public Permissions permissions { get; set; }

        [DataMember(Name = "groups", Order = 6, EmitDefaultValue = false)]
        public Groups groups { get; set; }
    }

    [CollectionDataContract(Name = "applicationSettings", Namespace = "http://www.iringtools.org/library", ItemName = "applicationSetting")]
    public class ApplicationSettings : List<ApplicationSetting>
    {

    }

    [DataContract(Name = "applicationSetting", Namespace = "http://www.iringtools.org/library")]
    public class ApplicationSetting
    {
        [DataMember(Name = "name", Order = 0, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(Name = "value", Order = 1, EmitDefaultValue = false)]
        public string Value { get; set; }
    }


    [CollectionDataContract(Name = "dataFilters", Namespace = "http://www.iringtools.org/library", ItemName = "dataFilter")]
    public class DataFilters : List<DataFilter>
    {

    }

    [DataContract(Name = "dataFilter", Namespace = "http://www.iringtools.org/library")]
    public class DataFilter
    {
        DataFilter()
        {
            expressions = new Expressions();
            orderExpressions = new OrderExpressions();
        }
        [DataMember(Name = "dataFilterId", Order = 0)]
        public Guid DataFilterId { get; set; }

        [DataMember(Name = "resourceId", Order = 1)]
        public Guid ResourceId { get; set; }

        [DataMember(Name = "dataFilterTypeId", Order = 2, EmitDefaultValue = false)]
        public int DataFilterTypeId { get; set; }

        [DataMember(Name = "siteId", Order = 3, EmitDefaultValue = false)]
        public int SiteId { get; set; }

        [DataMember(Name = "active", Order = 4, EmitDefaultValue = false)]
        public Byte Active { get; set; }

        [DataMember(Name = "expressions", Order = 5, EmitDefaultValue = false)]
        public Expressions expressions { get; set; }

        [DataMember(Name = "orderExpressions", Order = 6, EmitDefaultValue = false)]
        public OrderExpressions orderExpressions { get; set; }

        
    }

    [CollectionDataContract(Name = "expressions", Namespace = "http://www.iringtools.org/library", ItemName = "expression")]
    public class Expressions : List<Expression>
    {

    }

    [DataContract(Name = "expression", Namespace = "http://www.iringtools.org/library")]
    public class Expression
    {
        [DataMember(Name = "dfOrder", Order = 0, EmitDefaultValue = false)]
        public int DFOrder { get; set; }

        [DataMember(Name = "openCount", Order = 1, EmitDefaultValue = false)]
        public int OpenCount { get; set; }

        [DataMember(Name = "logicalOperator", Order = 2, EmitDefaultValue = false)]
        public string LogicalOperator { get; set; }

        [DataMember(Name = "propertyName", Order = 4, EmitDefaultValue = false)]
        public string PropertyName { get; set; }

        [DataMember(Name = "relationalOperator", Order = 5, EmitDefaultValue = false)]
        public string RelationalOperator { get; set; }

        [DataMember(Name = "values", Order = 6, EmitDefaultValue = false)]
        public values values { get; set; }

        [DataMember(Name = "closeCount", Order = 7, EmitDefaultValue = false)]
        public int CloseCount { get; set; }

    }

    [CollectionDataContract(Name = "values", Namespace = "http://www.iringtools.org/library", ItemName = "value")]
    public class values : List<string>
    {

    }

    [CollectionDataContract(Name = "orderExpressions", Namespace = "http://www.iringtools.org/library", ItemName = "orderExpression")]
    public class OrderExpressions : List<OrderExpression>
    {

    }

    [DataContract(Name = "orderExpression", Namespace = "http://www.iringtools.org/library")]
    public class OrderExpression
    {
        [DataMember(Name = "dfOrder", Order = 0, EmitDefaultValue = false)]
        public int DFOrder { get; set; }

        [DataMember(Name = "propertyName", Order = 1, EmitDefaultValue = false)]
        public string PropertyName { get; set; }

        [DataMember(Name = "sort", Order = 2, EmitDefaultValue = false)]
        public string Sort { get; set; }

    }

    [CollectionDataContract(Name = "valueMaps", Namespace = "http://www.iringtools.org/library", ItemName = "valueMap")]
    public class ValueMaps : List<ValueMap>
    {

    }

    [DataContract(Name = "valueMap", Namespace = "http://www.iringtools.org/library")]
    public class ValueMap
    {
        [DataMember(Name = "label", Order = 0, EmitDefaultValue = false)]
        public string Label { get; set; }

        [DataMember(Name = "internalValue", Order = 1, EmitDefaultValue = false)]
        public string InternalValue { get; set; }

        [DataMember(Name = "uri", Order = 2, EmitDefaultValue = false)]
        public string Uri { get; set; }

    }

    [CollectionDataContract(Name = "valueListMaps", Namespace = "http://www.iringtools.org/library", ItemName = "valueListMap")]
    public class ValueListMaps : List<ValueListMap>
    {

    }

    [DataContract(Name = "valueListMap", Namespace = "http://www.iringtools.org/library")]
    public class ValueListMap
    {
        ValueListMap()
        {
            valueMaps = new ValueMaps();
        }

        [DataMember(Name = "name", Order = 0, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(Name = "valueMaps", Order = 1, EmitDefaultValue = false)]
        public ValueMaps valueMaps { get; set; }

    }

    [DataContract(Name = "manifest", Namespace = "http://www.iringtools.org/library")]
    public class Manifest
    {
        public Manifest()
        {
            graphs = new org.iringtools.dxfr.manifest.Graphs();
            valueListMaps = new ValueListMaps();
        }

        [DataMember(Name = "graphs", Order = 0, EmitDefaultValue = false)]
        public org.iringtools.dxfr.manifest.Graphs graphs { get; set; }

        [DataMember(Name = "valueListMaps", Order = 1, EmitDefaultValue = false)]
        public ValueListMaps valueListMaps { get; set; }
    }
}
