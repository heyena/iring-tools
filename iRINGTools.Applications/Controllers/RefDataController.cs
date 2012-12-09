using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using org.iringtools.library;

using iRINGTools.Web.Helpers;
using iRINGTools.Web.Models;
using org.ids_adi.qmxf;
using VDS.RDF;
using org.iringtools.refdata;
using org.iringtools.refdata.federation;
using org.iringtools.refdata.response;
using org.iringtools.web.Models;

namespace org.iringtools.web.controllers
{
    public class RefDataController : BaseController
    {
        private IRefDataRepository _refdataRepository = null;
        private NamespaceMapper _nsMap = null;
        private List<Namespace> _namespaces = null;
        private Federation _federaton = null;
        private string _adapterServiceURI = String.Empty;
        private string _refDataServiceURI = String.Empty;
        
        public RefDataController()
            : this(new RefDataRepository())
        { }

        public RefDataController(IRefDataRepository repository)
        {
            _nsMap = new NamespaceMapper();
            _refdataRepository = repository;
            _federaton = _refdataRepository.GetFederation();
            _namespaces = _federaton.NamespaceList;
        }

        public JsonResult Index(FormCollection form)
        {
            var start = 0;
            var limit = 100;
            var id = form["id"];
            var query = form["query"];

            Int32.TryParse(form["limit"], out limit);
            Int32.TryParse(form["start"], out start);

            var container = new JsonContainer<List<Entity>>();

            if (!string.IsNullOrEmpty(query))
            {
                var dataEntities = _refdataRepository.Search(query, start, limit);

                container.items = dataEntities.Entities;
                container.total = dataEntities.Total;
                container.success = true;
            }

            return Json(container, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNode(FormCollection form)
        {
            List<JsonTreeNode> nodes = null;
            var start = 0;
            var limit = 100;
            var roleClassId = string.Empty;
            var id = form["id"];
            var searchtype = form["type"];
            var repositoryName = form["repositoryName"];
            var query = form["query"];
            if (string.IsNullOrEmpty(query)) return Json(nodes, JsonRequestBehavior.AllowGet);
            var range = form["range"];
            var isreset = form["reset"];

            Int32.TryParse(form["limit"], out limit);
            Int32.TryParse(form["start"], out start);

            if (!string.IsNullOrEmpty(range))
            {
                roleClassId = range.Substring(range.LastIndexOf("#") + 1);
            }

            if (!string.IsNullOrEmpty(query))
            {
                switch (searchtype)
                {
                    case "SearchNode":
                        nodes = GetClasses(query, start, limit, Convert.ToBoolean(isreset));
                        break;
                    case "ClassificationsNode":
                        nodes = GetClasses(id, repositoryName);
                        break;
                    case "MembersNode":
                        // nodes = GetClassMembers(id);
                        break;
                    case "SuperclassesNode":
                        nodes = GetSuperClasses(id, repositoryName);
                        break;
                    case "SubclassesNode":
                        nodes = GetSubClasses(id, repositoryName);
                        break;
                    case "ClassTemplatesNode":
                        nodes = GetTemplates(id);
                        break;
                    case "TemplateNode":
                        nodes = GetRoles(id);
                        break;
                    case "RoleNode":
                        if (string.IsNullOrEmpty(roleClassId)) break;
                        nodes = GetRoleClass(roleClassId);
                        break;
                    case "ClassNode":
                        nodes = GetClasses(id, repositoryName);
                        break;
                    default:
                        nodes = new List<JsonTreeNode>();
                        break;
                }
            }

            return Json(nodes, JsonRequestBehavior.AllowGet);
        }

        private List<JsonTreeNode> GetClassMembers(string id)
        {
            Repository repository = null;
            var nodes = new List<JsonTreeNode>();
            if (!string.IsNullOrEmpty(id))
            {
                var dataEntities = _refdataRepository.GetClassMembers(id, repository);

                foreach (var node in dataEntities.Select(entity => new TreeNode
                    {
                        type = "MemberNode",
                        iconCls = "treeClass",
                        identifier = entity.Uri.Split('#')[1],
                        id = Guid.NewGuid().ToString(),
                        text = entity.Label,
                        leaf = false,
                        children = GetDefaultChildren(entity.Label),
                        record = entity
                    }))
                {
                    node.children.Add(node);
                }
            }
            return nodes;
        }

        private List<JsonTreeNode> GetDefaultChildren(string label)
        {
            var nodes = new List<JsonTreeNode>();

            var memberNode = new JsonTreeNode
            {
                id = Guid.NewGuid().ToString(),

                iconCls = "folder",
                leaf = false,
                text = "Members",
                type = "MembersNode",
                identifier = null,
            };
            nodes.Add(memberNode);
            var clasifNode = new JsonTreeNode
            {
                iconCls = "folder",
                id = Guid.NewGuid().ToString(),

                leaf = false,
                text = "Member Of",
                type = "ClassificationsNode",
                //  expanded = false
            };
            nodes.Add(clasifNode);
            var supersNode = new JsonTreeNode
            {
                iconCls = "folder",
                id = Guid.NewGuid().ToString(),

                leaf = false,
                text = "Superclasses",
                type = "SuperclassesNode",
                //    expanded = false
            };
            nodes.Add(supersNode);
            var subsNode = new JsonTreeNode
            {
                iconCls = "folder",
                id = Guid.NewGuid().ToString(),

                leaf = false,
                text = "Subclasses",
                type = "SubclassesNode",
                //   expanded = false
            };
            nodes.Add(subsNode);
            var tempsNode = new JsonTreeNode
            {
                id = Guid.NewGuid().ToString(),
                iconCls = "folder",

                leaf = false,
                text = "Templates",
                type = "ClassTemplatesNode",
                //   expanded = false
            };
            nodes.Add(tempsNode);

            return nodes;
        }

        private List<JsonTreeNode> GetClasses(string query, int start, int limit, bool isReset)
        {
            var nodes = new List<JsonTreeNode>();
            var prefix = string.Empty;
            var ident = string.Empty;
            //var dataEntities = new org.iringtools.refdata.response.Response();
            var dataEntities = isReset ? _refdataRepository.SearchReset(query) : _refdataRepository.Search(query, start, limit);

            foreach (var entity in dataEntities.Entities)
            {
                var label = entity.Label + '[' + entity.Repository + ']';

                if (entity.Uri.Contains("#"))
                {
                    prefix = _namespaces.Find(n => n.Uri == entity.Uri.Substring(0, entity.Uri.IndexOf("#") + 1)).Prefix;
                    ident = entity.Uri.Split('#')[1];
                }
                else
                {
                    prefix = _namespaces.Find(n => n.Uri == entity.Uri.Substring(0, entity.Uri.LastIndexOf("/", System.StringComparison.Ordinal) + 1)).Prefix;
                    ident = entity.Uri.Substring(entity.Uri.LastIndexOf("/", System.StringComparison.Ordinal) + 1);
                }

                var node = new JsonTreeNode
                {
                    type = (prefix.Equals("rdl")) ? "ClassNode" : "TemplateNode",
                    iconCls = (prefix.Equals("rdl")) ? "treeClass" : "treeTemplate",
                    identifier = ident,
                    id = Guid.NewGuid().ToString(),
                    text = label,
                    //     expanded = false,
                    leaf = false,
                    // children = new List<JsonTreeNode>(),
                    record = entity
                };

                nodes.Add(node);
            }

            return nodes;
        }

        private List<JsonTreeNode> GetTemplateRoleClasses(string classId)
        {
            var nodes = new List<JsonTreeNode>();
            var prefix = string.Empty;
            var ident = string.Empty;
            var ns = string.Empty;

            if (classId != string.Empty)
            {
                var dataEntities = _refdataRepository.GetClasses(classId, null);

                foreach (var entity in dataEntities.ClassDefinitions)
                {
                    if (entity.Identifier.Contains("#"))
                    {
                        prefix = _namespaces.Find(n => n.Uri == entity.Identifier.Substring(0, entity.Identifier.IndexOf("#") + 1)).Prefix;
                        ident = entity.Identifier.Split('#')[1];
                        ns = _namespaces.Find(n => n.Uri == entity.Identifier.Substring(0, entity.Identifier.LastIndexOf("#") + 1)).Uri;
                    }
                    else
                    {
                        prefix = _namespaces.Find(n => n.Uri == entity.Identifier.Substring(0, entity.Identifier.LastIndexOf("/") + 1)).Prefix;
                        ident = entity.Identifier.Substring(entity.Identifier.LastIndexOf("/") + 1);
                        ns = _namespaces.Find(n => n.Uri == entity.Identifier.Substring(0, entity.Identifier.LastIndexOf("/") + 1)).Uri;
                    }

                    var label = entity.Name[0].Value.ToString() + '[' + entity.RepositoryName + ']';

                    var node = new JsonRefDataNode
                    {
                        type = (prefix.Contains("rdl")) ? "ClassNode" : "TemplateNode",
                        iconCls = (prefix.Contains("rdl")) ? "treeClass" : "treeTemplate",
                        identifier = ident,
                        id = Guid.NewGuid().ToString(),
                        text = label,
                        //     expanded = false,
                        leaf = false,
                        //  children = (prefix.Equals("rdl")) ? GetDefaultChildren(label) : null,
                        record = entity,
                        Namespace = ns
                    };

                    nodes.Add(node);
                }
            }
            return nodes;
        }

        private List<JsonTreeNode> GetClasses(string classId, string repositoryName)
        {
            var nodes = new List<JsonTreeNode>();
            QMXF dataEntities = null;
            Repository repository = null;

            if (!string.IsNullOrEmpty(classId))
            {
                if (!string.IsNullOrEmpty(repositoryName))
                    repository = _refdataRepository.GetFederation().RepositoryList.Find(r => r.Name == repositoryName);

                dataEntities = _refdataRepository.GetClasses(classId, repository);

                foreach (var entity in dataEntities.ClassDefinitions)
                {
                    #region Default Nodes------------------
                    var label = entity.Name[0].Value;

                    var memberNode = new TreeNode
                    {
                        id = Guid.NewGuid().ToString(),
                        children = new List<JsonTreeNode>(),
                        iconCls = "folder",
                        leaf = false,
                        text = "Members",
                        type = "MembersNode",
                        identifier = null,
                        //    expanded = false
                    };

                    var clasifNode = new TreeNode
                    {
                        id = Guid.NewGuid().ToString(),
                        children = new List<JsonTreeNode>(),
                        iconCls = "folder",
                        leaf = false,
                        text = "Member Of",
                        type = "ClassificationsNode",
                        identifier = null,
                        //  expanded = false
                    };

                    var supersNode = new TreeNode
                    {
                        id = Guid.NewGuid().ToString(),
                        children = new List<JsonTreeNode>(),
                        iconCls = "folder",
                        leaf = false,
                        text = "Superclasses",
                        type = "SuperclassesNode",
                        //    expanded = false
                    };

                    var subsNode = new TreeNode
                    {
                        id = Guid.NewGuid().ToString(),

                        iconCls = "folder",
                        leaf = false,
                        text = "Subclasses",
                        type = "SubclassesNode",
                        //    expanded = false
                    };

                    var tempsNode = new TreeNode
                    {
                        id = Guid.NewGuid().ToString(),

                        iconCls = "folder",
                        leaf = false,
                        text = "Templates",
                        type = "ClassTemplatesNode",
                        //   expanded = false
                    };
                    #endregion

                    #region Add Hidden node for Properties------------
                    var reference = string.Empty;
                    if (entity.EntityType != null)
                    {
                        reference = Convert.ToString(entity.EntityType.Reference);
                    }
                    var properties = new Dictionary<string, string>()
                          {
                            {"Description", Convert.ToString(entity.Description[0].Value)},
                            {"Entity Type", reference},
                            {"Identifiers", Convert.ToString(entity.Identifier)},
                            {"Name", Convert.ToString(entity.Name[0].Value)},
                            {"Repository", Convert.ToString(entity.RepositoryName)},
                            {"Status Authority", Convert.ToString(entity.Status[0].Authority)},
                            {"Status Class", Convert.ToString(entity.Status[0].Class)},
                            {"Status From", Convert.ToString(entity.Status[0].From)},
                          };
                    #endregion
                    // nodes.Add(memberNode);


                    #region Fill Data in Classification node--------
                    foreach (var leafNode in entity.Classification.Select(classification => new JsonTreeNode
                        {
                            type = "ClassNode",
                            iconCls = "treeClass",
                            leaf = false,
                            identifier = classification.Reference.Split('#')[1],
                            id = Guid.NewGuid().ToString(),
                            text = classification.Label,
                            //       expanded = false,

                            record = classification
                        }))
                    {
                        clasifNode.children.Add(leafNode);
                    }
                    clasifNode.text = clasifNode.text + " (" + clasifNode.children.Count() + ")";
                    if (!clasifNode.children.Any())
                    {
                        clasifNode.leaf = true;
                        clasifNode.icon = "Content/img/folder.png";
                    }
                    #endregion

                    var membersNodes = GetClassMembers(classId, memberNode, repositoryName);
                    memberNode.record = properties;
                    nodes.Add(membersNodes);
                    nodes.Add(clasifNode); // Add Classification node.
                    nodes.Add(supersNode); // Add SuperClassNode.
                    #region Fill Data in SuperClass node--------
                    foreach (var leafNode in entity.Specialization.Select(specialization => new JsonTreeNode
                        {
                            type = "ClassNode",
                            iconCls = "treeClass",
                            leaf = false,
                            identifier = specialization.Reference.Split('#')[1],
                            id = Guid.NewGuid().ToString(),
                            text = specialization.Label,
                            //      expanded = false,

                            record = specialization
                        }))
                    {
                        supersNode.children.Add(leafNode);
                    }
                    supersNode.text = supersNode.text + " (" + supersNode.children.Count() + ")";
                    if (!supersNode.children.Any())
                    {
                        supersNode.leaf = true;
                        supersNode.iconCls = "folder";
                    }
                    #endregion

                    //Get Sub Classes
                    //JsonTreeNode subClassNodes = GetSubClasses(classId, subsNode);
                    var subNodeCount = GetSubClassesCount(classId);
                    subsNode.text = subsNode.text + "(" + subNodeCount + ")";

                    if (subNodeCount == "0")
                    {
                        subsNode.children = null;
                        subsNode.leaf = true;
                        subsNode.iconCls = "folder";
                    }
                    nodes.Add(subsNode);

                    var templateNodesCount = GetTemplatesCount(classId);
                    tempsNode.text = tempsNode.text + "(" + templateNodesCount + ")";
                    if (templateNodesCount == "0")
                    {
                        tempsNode.children = null;
                        tempsNode.leaf = true;
                        tempsNode.iconCls = "folder";
                    }

                    nodes.Add(tempsNode);
                }
            }

            return nodes;
        }

        private string GetSubClassesCount(string classId)
        {
            var count = string.Empty;
            if (!string.IsNullOrEmpty(classId))
            {
                var dataEntities = _refdataRepository.GetSubClassesCount(classId);
                foreach (var entity in dataEntities)
                {
                    count = entity.Label;
                }
            }
            return count;
        }

        private List<JsonTreeNode> GetSubClasses(string classId, string repositoryName)
        {
            var nodes = new List<JsonTreeNode>();
            Repository repository = null;

            if (!string.IsNullOrEmpty(classId))
            {
                if (!string.IsNullOrEmpty(repositoryName))
                    repository = _refdataRepository.GetFederation().RepositoryList.Find(r => r.Name == repositoryName);

                var dataEntities = _refdataRepository.GetSubClasses(classId, repository);
                nodes.AddRange(dataEntities.Select(entity => new JsonTreeNode
                    {
                        type = "ClassNode",
                        iconCls = "treeClass",
                        identifier = entity.Uri.Split('#')[1],
                        id = Guid.NewGuid().ToString(),
                        text = entity.Label,
                        leaf = false,
                        record = entity
                    }));
            }

            return nodes;
        }

        private List<JsonTreeNode> GetSuperClasses(string classId, string repositoryName)
        {
            var nodes = new List<JsonTreeNode>();
            Repository repository = null;

            if (!string.IsNullOrEmpty(classId))
            {
                if (!string.IsNullOrEmpty(repositoryName))
                    repository = _refdataRepository.GetFederation().RepositoryList.Find(r => r.Name == repositoryName);

                var dataEntities = _refdataRepository.GetSuperClasses(classId, repository);
                nodes.AddRange(dataEntities.Select(entity => new JsonTreeNode
                    {
                        type = "ClassNode",
                        iconCls = "treeClass",
                        identifier = entity.Uri.Split('#')[1],
                        id = Guid.NewGuid().ToString(),
                        text = entity.Label,
                        leaf = false,
                        record = entity
                    }));
            }

            return nodes;
        }

        private JsonTreeNode GetClassMembers(string classId, TreeNode tempsNode, string repositoryName)
        {
            Repository repository = null;

            if (!string.IsNullOrEmpty(classId))
            {
                if (!string.IsNullOrEmpty(repositoryName))
                    repository = _refdataRepository.GetFederation().RepositoryList.Find(r => r.Name == repositoryName);

                var dataEntities = _refdataRepository.GetClassMembers(classId, repository);
                foreach (var node in dataEntities.Select(entity => new JsonTreeNode
                    {
                        type = "ClassNode",
                        iconCls = "treeClass",
                        identifier = entity.Uri.Split('#')[1],
                        id = Guid.NewGuid().ToString(),
                        text = entity.Label,
                        leaf = false,
                        record = entity
                    }))
                {
                    tempsNode.children.Add(node);
                }
                tempsNode.text = tempsNode.text + " (" + tempsNode.children.Count() + ")";
                if (!tempsNode.children.Any())
                    tempsNode.leaf = true;
            }
            return tempsNode;
        }

        private JsonTreeNode GetTemplates(string classId, TreeNode tempsNode)
        {

            if (!string.IsNullOrEmpty(classId))
            {
                var dataEntities = _refdataRepository.GetClassTemplates(classId);
                foreach (var node in dataEntities.Select(entity => new JsonTreeNode
                    {
                        type = "TemplateNode",
                        iconCls = "treeTemplate",
                        id = Guid.NewGuid().ToString(),
                        identifier = entity.Uri.Split('#')[1],
                        text = entity.Label,
                        leaf = false,
                        record = entity
                    }))
                {
                    tempsNode.children.Add(node);
                }
                tempsNode.text = tempsNode.text + " (" + tempsNode.children.Count() + ")";
            }

            return tempsNode;
        }

        private string GetTemplatesCount(string classId)
        {
            var count = string.Empty;
            if (!string.IsNullOrEmpty(classId))
            {
                var dataEntities = _refdataRepository.GetClassTemplatesCount(classId);
                foreach (var entity in dataEntities)
                {
                    count = entity.Label;
                }
            }
            return count;
        }

        private List<JsonTreeNode> GetTemplates(string classId)
        {
            var nodes = new List<JsonTreeNode>();

            if (!string.IsNullOrEmpty(classId))
            {
                var dataEntities = _refdataRepository.GetClassTemplates(classId);
                nodes.AddRange(dataEntities.Select(entity => new JsonTreeNode
                    {
                        nodeType = "async",
                        type = "TemplateNode",
                        iconCls = "treeTemplate",
                        id = Guid.NewGuid().ToString(),
                        identifier = entity.Uri.Split('#')[1],
                        text = entity.Label,
                        leaf = false,
                        record = entity
                    }));
            }

            return nodes;
        }

        private List<JsonTreeNode> GetRoleClass(string id)
        {
            var nodes = new List<JsonTreeNode>();

            if (!string.IsNullOrEmpty(id))
            {
                var entity = _refdataRepository.GetClasses(id, null);
                if (entity != null && entity.ClassDefinitions.Count > 0)
                {
                    var classNode = new JsonTreeNode
                    {
                        identifier = entity.ClassDefinitions[0].Identifier.Split('#')[1],
                        leaf = false,

                        text = entity.ClassDefinitions[0].Name[0].Value,
                        id = Guid.NewGuid().ToString(),
                        record = entity.ClassDefinitions[0],
                        type = "ClassNode",
                        iconCls = "treeClass"
                    };
                    nodes.Add(classNode);
                }
            }
            return nodes;
        }

        private List<JsonTreeNode> GetRoles(string Id)
        {
            var nodes = new List<JsonTreeNode>();

            if (!string.IsNullOrEmpty(Id))
            {
                var dataEntities = _refdataRepository.GetTemplate(Id);

                if (dataEntities.TemplateDefinitions.Count > 0)
                {
                    nodes.AddRange(from entity in dataEntities.TemplateDefinitions
                                   from role in entity.RoleDefinition
                                   select new JsonTreeNode
                                       {
                                           id = Guid.NewGuid().ToString(),
                                           type = "RoleNode",
                                           iconCls = "treeRole",
                                           leaf = false,
                                           text = role.Name[0].Value,
                                           identifier = role.Identifier,
                                           record = role
                                       });
                }
                else if (dataEntities.TemplateQualifications.Count > 0)
                {
                    foreach (var entity in dataEntities.TemplateQualifications)
                    {
                        foreach (var role in entity.RoleQualification)
                        {
                            var roleId = string.Empty;
                            if (role.Range != null)
                            {
                                roleId = role.Range.Split('#')[1];
                            }
                            var entityNode = new TreeNode
                            {
                                id = Guid.NewGuid().ToString(),
                                type = "RoleNode",
                                text = role.Name[0].Value,
                                iconCls = "treeRole",
                                children = GetTemplateRoleClasses(roleId),
                                leaf = false,
                                identifier = role.Identifier,
                                record = role
                            };
                            if (entityNode.children.Count() == 0)
                            {
                                entityNode.leaf = true;
                            }
                            nodes.Add(entityNode);
                        }
                    }
                }
            }

            return nodes;
        }
    }
}

