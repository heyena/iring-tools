
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace org.iringtools.refdata.federation
{

    [DataContract(Name = "federation", Namespace = "http://www.iringtools.org/refdata/federation")]
    public class Federation
    {

        [DataMember(Name = "idgeneratorlist", Order = 0)]
        public IdGeneratorList IdGeneratorList { get; set;}


        [DataMember(Name = "namespacelist", Order = 1)]
        public NamespaceList NamespaceList { get; set; }

        [DataMember(Name = "repositorylist", Order = 2)]
        public RepositoryList RepositoryList { get; set; }

    }

    [CollectionDataContract(Name = "idgeneratorlist", Namespace = "http://www.iringtools.org/refdata/federation")]
    public class IdGeneratorList : List<IdGenerator>
    {

        [DataMember(Name = "sequenceid")]
        public int SequenceId { get; set; }

        public bool SequenceIdSpecified { get; set; }

    }

    [DataContract(Name = "idgenerator", Namespace = "http://www.iringtools.org/refdata/federation")]
    public class IdGenerator
    {

        [DataMember(Name = "id")]
        public int Id { get; set; }


        [DataMember(Name = "uri")]
        public string Uri { get; set; }


        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }
    }

    [DataContract(Name = "repository", Namespace = "http://www.iringtools.org/refdata/federation")]
    public class Repository
    {
        [DataMember(Name = "id", Order = 0)]
        public string Id { get; set; }

        [DataMember(Name = "description", Order = 1)]
        public string Description { get; set; }

        [DataMember(Name = "isreadonly", Order = 2)]
        public bool IsReadonly { get; set; }

        [DataMember(Name = "name", Order = 3)]
        public string Name { get; set; }

        [DataMember(Name = "repositorytype", Order = 4)]
        public RepositoryType RepositoryType { get; set; }

        [DataMember(Name = "uri", Order = 5)]
        public string Uri { get; set; }

        [DataMember(Name = "updateUri", Order = 6)]
        public string UpdateUri { get; set; }

        [DataMember(Name = "namespaces", Order = 7)]
        public NamespaceList Namespaces { get; set; }
    }

    [DataContract(Name = "repositorytype", Namespace = "http://www.iringtools.org/refdata/federation")]
    public enum RepositoryType
    {

        [EnumMember] Part8,

        [EnumMember] RDSWIP,

        [EnumMember] Camelot,
        [EnumMember] JORD,
    }

    [CollectionDataContract(Name = "namespacelist", Namespace = "http://www.iringtools.org/refdata/federation")]
    public class NamespaceList : List<Namespace>
    {
        [DataMember(Name = "sequenceid")]
        public int SequenceId { get; set; }

        public bool SequenceIdSpecified { get; set; }
    }

    [DataContract(Name = "namespace", Namespace = "http://www.iringtools.org/refdata/federation")]
    public partial class Namespace
    {

        [DataMember(Name = "id", Order = 0)]
        public int Id { get; set; }


        [DataMember(Name = "uri", Order = 1)]
        public string Uri { get; set; }

        [DataMember(Name = "prefix", Order = 2)]
        public string Prefix { get; set; }


        [DataMember(Name = "iswriteable", Order = 2)]
        public bool IsWriteable { get; set; }


        [DataMember(Name = "description", Order = 3)]
        public string Description { get; set; }


        [DataMember(Name = "idgenerator", Order = 4)]
        public int IdGenerator { get; set; }

    }

    [CollectionDataContract(Name = "repositoryList", Namespace = "http://www.iringtools.org/refdata/federation")]
    public class RepositoryList : List<Repository>
    {

        [DataMember(Name = "sequenceid")]
        public int SequenceId { get; set; }

        public bool SequenceIdSpecified { get; set; }
    }
}