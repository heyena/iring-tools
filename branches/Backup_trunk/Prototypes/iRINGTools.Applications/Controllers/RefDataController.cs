using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Configuration;
using System.Collections.Specialized;

using org.iringtools.library;
using org.iringtools.utility;

using iRINGTools.Web.Helpers;
using iRINGTools.Web.Models;
using org.ids_adi.qmxf;
using org.iringtools.mapping;
using VDS.RDF;

namespace org.iringtools.web.controllers
{
    public class RefDataController : BaseController
    {

        private IRefDataRepository _refdataRepository = null;
        private NamespaceMapper _nsMap = new NamespaceMapper();
        private string _adapterServiceURI = String.Empty;
        private string _refDataServiceURI = String.Empty;
        public RefDataController()
            : this(new RefDataRepository())
        { }

        public RefDataController(IRefDataRepository repository)
        {
            _refdataRepository = repository;
            _nsMap.AddNamespace("eg", new Uri("http://example.org/data#"));
            _nsMap.AddNamespace("owl", new Uri("http://www.w3.org/2002/07/owl#"));
            _nsMap.AddNamespace("rdl", new Uri("http://rdl.rdlfacade.org/data#"));
            _nsMap.AddNamespace("tpL", new Uri("http://tpl.rdlfacade.org/data#"));
            _nsMap.AddNamespace("dm", new Uri("http://dm.rdlfacade.org/data#"));
            _nsMap.AddNamespace("p8dm", new Uri("http://standards.tc184-sc4.org/iso/15926/-8/data-model#"));
            _nsMap.AddNamespace("owl2xml", new Uri("http://www.w3.org/2006/12/owl2-xml#"));
            _nsMap.AddNamespace("p8", new Uri("http://standards.tc184-sc4.org/iso/15926/-8/template-model#"));
            _nsMap.AddNamespace("templates", new Uri("http://standards.tc184-sc4.org/iso/15926/-8/templates#"));
        }

        public JsonResult Index(FormCollection form)
        {

            int start = 0;
            int limit = 100;
            string id = form["id"];
            string query = form["query"];

            Int32.TryParse(form["limit"], out limit);
            Int32.TryParse(form["start"], out start);

            JsonContainer<List<Entity>> container = new JsonContainer<List<Entity>>();

            if (!string.IsNullOrEmpty(query))
            {
                RefDataEntities dataEntities = _refdataRepository.Search(query, start, limit);

                container.items = dataEntities.Entities.Values.ToList<Entity>();
                container.total = dataEntities.Total;
                container.success = true;
            }

            return Json(container, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNode(FormCollection form)
        {
            List<JsonTreeNode> nodes = null;
            int start = 0;
            int limit = 100;
            string roleClassId = string.Empty;
            string id = form["id"];
            string searchtype = form["type"];
            string query = form["query"];
            if (string.IsNullOrEmpty(query)) return Json(nodes, JsonRequestBehavior.AllowGet);
            string range = form["range"];
            string isreset = form["reset"];

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
                        nodes = GetClasses(id);
                        break;
                    case "MembersNode":
                        // nodes = GetClassMembers(id);
                        break;
                    case "SuperclassesNode":
                        nodes = GetSuperClasses(id);
                        break;
                    case "SubclassesNode":
                        nodes = GetSubClasses(id);
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
                        nodes = GetClasses(id);
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
            List<JsonTreeNode> nodes = new List<JsonTreeNode>();
            if (!string.IsNullOrEmpty(id))
            {
                Entities dataEntities = _refdataRepository.GetClassMembers(id);
                foreach (Entity entity in dataEntities)
                {
                  TreeNode node = new TreeNode
                    {
                        type = "MemberNode",
                        icon = "Content/img/class.png",
                        identifier = entity.Uri.Split('#')[1],
                        id = Guid.NewGuid().ToString(),
                        text = entity.Label,
                        leaf = false,
                        children = GetDefaultChildren(entity.Label),
                        record = entity
                    };
                    node.children.Add(node);
                }
            }
            return nodes;
        }

        private List<JsonTreeNode> GetDefaultChildren(string label)
        {
            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

            JsonTreeNode memberNode = new JsonTreeNode
            {
                id = Guid.NewGuid().ToString(),
                
                iconCls = "folder",
                leaf = false,
                text = "Members",
                type = "MembersNode",
                identifier = null,
            };
            nodes.Add(memberNode);
            JsonTreeNode clasifNode = new JsonTreeNode
            {
                icon = "Content/img/folder.png",
                id = Guid.NewGuid().ToString(),
                
                leaf = false,
                text = "Member Of",
                type = "ClassificationsNode",
                //  expanded = false
            };
            nodes.Add(clasifNode);
            JsonTreeNode supersNode = new JsonTreeNode
            {
                icon = "Content/img/folder.png",
                id = Guid.NewGuid().ToString(),
                
                leaf = false,
                text = "Superclasses",
                type = "SuperclassesNode",
                //    expanded = false
            };
            nodes.Add(supersNode);
            JsonTreeNode subsNode = new JsonTreeNode
            {
                icon = "Content/img/folder.png",
                id = Guid.NewGuid().ToString(),
                
                leaf = false,
                text = "Subclasses",
                type = "SubclassesNode",
                //   expanded = false
            };
            nodes.Add(subsNode);
            JsonTreeNode tempsNode = new JsonTreeNode
            {
                id = Guid.NewGuid().ToString(),
                icon = "Content/img/folder.png",
                
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
            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

            RefDataEntities dataEntities = new RefDataEntities();
            if (isReset)
            {
                dataEntities = _refdataRepository.SearchReset(query);
            }
            else
            {
                dataEntities = _refdataRepository.Search(query, start, limit);
            }

            foreach (Entity entity in dataEntities.Entities.Values.ToList<Entity>())
            {
                string label = entity.Label + '[' + entity.Repository + ']';
                string prefix = _nsMap.GetPrefix(new Uri(entity.Uri.Substring(0, entity.Uri.LastIndexOf("#") + 1)));
                JsonTreeNode node = new JsonTreeNode
                {
                    type = (prefix.Equals("rdl")) ? "ClassNode" : "TemplateNode",
                    icon = (prefix.Equals("rdl")) ? "Content/img/class.png" : "Content/img/template.png",
                    identifier = entity.Uri.Split('#')[1],
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
            List<JsonTreeNode> nodes = new List<JsonTreeNode>();
            if (classId != string.Empty)
            {
                QMXF dataEntities = _refdataRepository.GetClasses(classId);

                foreach (var entity in dataEntities.classDefinitions)
                {
                    string label = entity.name[0].value.ToString() + '[' + entity.repositoryName + ']';
                    string prefix = _nsMap.GetPrefix(new Uri(entity.identifier.Substring(0, entity.identifier.LastIndexOf("#") + 1)));
                    JsonTreeNode node = new JsonTreeNode
                    {
                        type = (prefix.Equals("rdl")) ? "ClassNode" : "TemplateNode",
                        icon = (prefix.Equals("rdl")) ? "Content/img/class.png" : "Content/img/template.png",
                        identifier = entity.identifier.Split('#')[1],
                        id = Guid.NewGuid().ToString(),
                        text = label,
                        //     expanded = false,
                        leaf = false,
                        //  children = (prefix.Equals("rdl")) ? GetDefaultChildren(label) : null,
                        record = entity
                    };

                    nodes.Add(node);
                }
            }
            return nodes;
        }

        private List<JsonTreeNode> GetClasses(string classId)
        {
            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

            if (!string.IsNullOrEmpty(classId))
            {
                QMXF dataEntities = _refdataRepository.GetClasses(classId);
                foreach (var entity in dataEntities.classDefinitions)
                {
                    #region Default Nodes------------------
                    var label = entity.name[0].value;

                    TreeNode memberNode = new TreeNode
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

                    TreeNode clasifNode = new TreeNode
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

                    TreeNode supersNode = new TreeNode
                    {
                        id = Guid.NewGuid().ToString(),
                        children = new List<JsonTreeNode>(),
                        iconCls = "folder",
                        leaf = false,
                        text = "Superclasses",
                        type = "SuperclassesNode",
                        //    expanded = false
                    };

                    TreeNode subsNode = new TreeNode
                    {
                        id = Guid.NewGuid().ToString(),
                        
                        iconCls = "folder",
                        leaf = false,
                        text = "Subclasses",
                        type = "SubclassesNode",
                        //    expanded = false
                    };

                    TreeNode tempsNode = new TreeNode
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
                    string reference = string.Empty;
                    if (entity.entityType != null)
                    {
                        reference = Convert.ToString(entity.entityType.reference);
                    }
                    Dictionary<string, string> properties = new Dictionary<string, string>()
                          {
                            {"Description", Convert.ToString(entity.description[0].value)},
                            {"Entity Type", reference},
                            {"Identifiers", Convert.ToString(entity.identifier)},
                            {"Name", Convert.ToString(entity.name[0].value)},
                            {"Repository", Convert.ToString(entity.repositoryName)},
                            {"Status Authority", Convert.ToString(entity.status[0].authority)},
                            {"Status Class", Convert.ToString(entity.status[0].Class)},
                            {"Status From", Convert.ToString(entity.status[0].from)},
                          };
                    #endregion
                    // nodes.Add(memberNode);


                    #region Fill Data in Classification node--------
                    foreach (var classification in entity.classification)
                    {

                        JsonTreeNode leafNode = new JsonTreeNode
                        {
                            type = "ClassNode",
                            icon = "Content/img/class.png",
                            leaf = false,
                            identifier = classification.reference.Split('#')[1],
                            id = Guid.NewGuid().ToString(),
                            text = classification.label,
                            //       expanded = false,
                            
                            record = classification
                        };

                        clasifNode.children.Add(leafNode);
                    }
                    clasifNode.text = clasifNode.text + " (" + clasifNode.children.Count() + ")";
                    if (clasifNode.children.Count() == 0)
                    {
                        clasifNode.leaf = true;
                        clasifNode.icon = "Content/img/folder.png";
                    }
                    #endregion

                    JsonTreeNode membersNodes = GetClassMembers(classId, memberNode);
                    memberNode.record = properties;
                    nodes.Add(membersNodes);
                    nodes.Add(clasifNode); // Add Classification node.
                    nodes.Add(supersNode); // Add SuperClassNode.
                    #region Fill Data in SuperClass node--------
                    foreach (var specialization in entity.specialization)
                    {

                        JsonTreeNode leafNode = new JsonTreeNode
                        {
                            type = "ClassNode",
                            icon = "Content/img/class.png",
                            leaf = false,
                            identifier = specialization.reference.Split('#')[1],
                            id = Guid.NewGuid().ToString(),
                            text = specialization.label,
                            //      expanded = false,
                            
                            record = specialization
                        };

                        supersNode.children.Add(leafNode);
                    }
                    supersNode.text = supersNode.text + " (" + supersNode.children.Count() + ")";
                    if (supersNode.children.Count() == 0)
                    {
                        supersNode.leaf = true;
                        supersNode.icon = "Content/img/folder.png";
                    }
                    #endregion

                    //Get Sub Classes
                    //JsonTreeNode subClassNodes = GetSubClasses(classId, subsNode);
                    string subNodeCount = GetSubClassesCount(classId);
                    subsNode.text = subsNode.text + "(" + subNodeCount + ")";

                    if (subNodeCount == "0")
                    {
                        subsNode.children = null;
                        subsNode.leaf = true;
                        subsNode.icon = "Content/img/folder.png";
                    }
                    nodes.Add(subsNode);

                    //Get Templates
                    // JsonTreeNode templateNodes = GetTemplates(classId, tempsNode);
                    string templateNodesCount = GetTemplatesCount(classId);
                    tempsNode.text = tempsNode.text + "(" + templateNodesCount + ")";
                    if (templateNodesCount == "0")
                    {
                        tempsNode.children = null;
                        tempsNode.leaf = true;
                        tempsNode.icon = "Content/img/folder.png";
                    }

                    nodes.Add(tempsNode);
                }
            }

            return nodes;
        }

        private JsonTreeNode GetSubClasses(string classId, TreeNode subsNode)
        {
            if (!string.IsNullOrEmpty(classId))
            {
                Entities dataEntities = _refdataRepository.GetSubClasses(classId);
                foreach (var entity in dataEntities)
                {
                    JsonTreeNode node = new JsonTreeNode
                    {
                        type = "ClassNode",
                        icon = "Content/img/class.png",
                        identifier = entity.Uri.Split('#')[1],
                        id = Guid.NewGuid().ToString(),
                        text = entity.Label,
                        //   expanded = false,
                        leaf = false,
                        
                        record = entity
                    };

                    subsNode.children.Add(node);
                }
                subsNode.text = subsNode.text + " (" + subsNode.children.Count() + ")";
            }

            return subsNode;
        }

        private string GetSubClassesCount(string classId)
        {
            string count = string.Empty;
            if (!string.IsNullOrEmpty(classId))
            {
                Entities dataEntities = _refdataRepository.GetSubClassesCount(classId);
                foreach (var entity in dataEntities)
                {
                    count = entity.Label;
                }
            }
            return count;
        }

        private List<JsonTreeNode> GetSubClasses(string classId)
        {
            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

            if (!string.IsNullOrEmpty(classId))
            {
                Entities dataEntities = _refdataRepository.GetSubClasses(classId);
                foreach (var entity in dataEntities)
                {
                    JsonTreeNode node = new JsonTreeNode
                    {
                        type = "ClassNode",
                        icon = "Content/img/class.png",
                        identifier = entity.Uri.Split('#')[1],
                        id = Guid.NewGuid().ToString(),
                        text = entity.Label,
                        //    expanded = false,
                        leaf = false,
                        //  children = GetDefaultChildren(entity.Label),
                        record = entity
                    };

                    nodes.Add(node);
                }
            }

            return nodes;
        }

        private List<JsonTreeNode> GetSuperClasses(string classId)
        {
            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

            if (!string.IsNullOrEmpty(classId))
            {
                Entities dataEntities = _refdataRepository.GetSuperClasses(classId);
                foreach (var entity in dataEntities)
                {
                    JsonTreeNode node = new JsonTreeNode
                    {
                        type = "ClassNode",
                        icon = "Content/img/class.png",
                        identifier = entity.Uri.Split('#')[1],
                        id = Guid.NewGuid().ToString(),
                        text = entity.Label,
                        //   expanded = false,
                        leaf = false,
                        //  children = GetDefaultChildren(entity.Label),
                        record = entity
                    };
                    nodes.Add(node);
                }
            }

            return nodes;
        }

        private JsonTreeNode GetClassMembers(string classId, TreeNode tempsNode)
        {
            if (!string.IsNullOrEmpty(classId))
            {
                Entities dataEntities = _refdataRepository.GetClassMembers(classId);
                foreach (Entity entity in dataEntities)
                {
                    JsonTreeNode node = new JsonTreeNode
                    {
                        type = "ClassNode",
                        icon = "Content/img/class.png",
                        identifier = entity.Uri.Split('#')[1],
                        id = Guid.NewGuid().ToString(),
                        text = entity.Label,
                        //    expanded = false,
                        leaf = false,
                        //   
                        record = entity
                    };
                    tempsNode.children.Add(node);
                }
                tempsNode.text = tempsNode.text + " (" + tempsNode.children.Count() + ")";
                if (tempsNode.children.Count() == 0)
                    tempsNode.leaf = true;
            }
            return tempsNode;
        }

        private JsonTreeNode GetTemplates(string classId, TreeNode tempsNode)
        {

            if (!string.IsNullOrEmpty(classId))
            {
                Entities dataEntities = _refdataRepository.GetClassTemplates(classId);
                foreach (var entity in dataEntities)
                {
                    JsonTreeNode node = new JsonTreeNode
                    {
                        type = "TemplateNode",
                        icon = "Content/img/template.png",
                        id = Guid.NewGuid().ToString(),
                        identifier = entity.Uri.Split('#')[1],
                        text = entity.Label,
                        //    expanded = false,
                        leaf = false,
                        
                        record = entity
                    };

                    tempsNode.children.Add(node);
                }
                tempsNode.text = tempsNode.text + " (" + tempsNode.children.Count() + ")";
            }

            return tempsNode;
        }

        private string GetTemplatesCount(string classId)
        {
            string count = string.Empty;
            if (!string.IsNullOrEmpty(classId))
            {
                Entities dataEntities = _refdataRepository.GetClassTemplatesCount(classId);
                foreach (var entity in dataEntities)
                {
                    count = entity.Label;
                }
            }
            return count;
        }

        private List<JsonTreeNode> GetTemplates(string classId)
        {
            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

            if (!string.IsNullOrEmpty(classId))
            {
                Entities dataEntities = _refdataRepository.GetClassTemplates(classId);
                foreach (var entity in dataEntities)
                {
                    JsonTreeNode node = new JsonTreeNode
                    {
                        nodeType = "async",
                        type = "TemplateNode",
                        icon = "Content/img/template.png",
                        id = Guid.NewGuid().ToString(),
                        identifier = entity.Uri.Split('#')[1],
                        text = entity.Label,
                        //    expanded = false,
                        leaf = false,
                        
                        record = entity
                    };

                    nodes.Add(node);
                }
            }

            return nodes;
        }

        private List<JsonTreeNode> GetRoleClass(string id)
        {
            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

            if (!string.IsNullOrEmpty(id))
            {
                QMXF entity = _refdataRepository.GetClasses(id);
                if (entity != null && entity.classDefinitions.Count > 0)
                {
                    JsonTreeNode classNode = new JsonTreeNode
                    {
                        identifier = entity.classDefinitions[0].identifier.Split('#')[1],
                        leaf = false,
                        
                        text = entity.classDefinitions[0].name[0].value,
                        id = Guid.NewGuid().ToString(),
                        record = entity.classDefinitions[0],
                        type = "ClassNode",
                        icon = "Content/img/class.png"
                    };
                    nodes.Add(classNode);
                }
            }
            return nodes;
        }

        private List<JsonTreeNode> GetRoles(string Id)
        {
            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

            if (!string.IsNullOrEmpty(Id))
            {
                QMXF dataEntities = _refdataRepository.GetTemplate(Id);

                if (dataEntities.templateDefinitions.Count > 0)
                {
                    foreach (var entity in dataEntities.templateDefinitions)
                    {
                        foreach (var role in entity.roleDefinition)
                        {
                            JsonTreeNode entityNode = new JsonTreeNode
                            {
                                id = Guid.NewGuid().ToString(),
                                type = "RoleNode",
                                icon = "Content/img/role.png",
                                
                                leaf = false,
                                text = role.name[0].value,
                                identifier = role.identifier,
                                record = role
                            };
                            nodes.Add(entityNode);
                        }
                    }
                }
                else if (dataEntities.templateQualifications.Count > 0)
                {
                    foreach (var entity in dataEntities.templateQualifications)
                    {
                        foreach (var role in entity.roleQualification)
                        {
                            string roleId = string.Empty;
                            if (role.range != null)
                            {
                                roleId = role.range.Split('#')[1];
                            }
                            TreeNode entityNode = new TreeNode
                            {
                                id = Guid.NewGuid().ToString(),
                                type = "RoleNode",
                                text = role.name[0].value,
                                icon = "Content/img/role.png",
                                children = GetTemplateRoleClasses(roleId),
                                leaf = false,
                                identifier = role.identifier,
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

