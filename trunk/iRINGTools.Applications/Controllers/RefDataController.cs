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

namespace iRINGTools.Web.Controllers
{
    public class RefDataController : Controller
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
            int start = 0;
            int limit = 100;
            string roleClassId = string.Empty;
            string id = form["id"];
            string searchtype = form["type"];
            string query = form["query"];
            string range = form["range"];
            string isreset = form["reset"];

            Int32.TryParse(form["limit"], out limit);
            Int32.TryParse(form["start"], out start);

            List<JsonTreeNode> nodes = null;
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
                    case "SuperclassesNode":
                        nodes = GetSuperClasses(id);
                        break;
                    case "SubclassesNode":
                        // nodes = GetSubClasses(id);
                        break;
                    case "ClassTemplatesNode":
                        // nodes = GetTemplates(id);
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

        private List<JsonTreeNode> GetDefaultChildren(string label)
        {
            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

            JsonTreeNode clasifNode = new JsonTreeNode
            {
                id = ("Classifications" + label).GetHashCode().ToString(),
                children = null,
                leaf = false,
                text = "Classifications",
                type = "ClassificationsNode",
                expanded = false
            };
            nodes.Add(clasifNode);
            JsonTreeNode supersNode = new JsonTreeNode
            {
                id = ("Superclasses" + label).GetHashCode().ToString(),
                children = null,
                leaf = false,
                text = "Superclasses",
                type = "SuperclassesNode",
                expanded = false
            };
            nodes.Add(supersNode);
            JsonTreeNode subsNode = new JsonTreeNode
            {
                id = ("Subclasses" + label).GetHashCode().ToString(),
                children = null,
                leaf = false,
                text = "Subclasses",
                type = "SubclassesNode",
                expanded = false
            };
            nodes.Add(subsNode);
            JsonTreeNode tempsNode = new JsonTreeNode
            {
                id = ("Templates" + label).GetHashCode().ToString(),
                children = null,
                leaf = false,
                text = "Templates",
                type = "ClassTemplatesNode",
                expanded = false
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
                    nodeType = "async",
                    type = (prefix.Equals("rdl")) ? "ClassNode" : "TemplateNode",
                    icon = (prefix.Equals("rdl")) ? "Content/img/class.png" : "Content/img/template.png",
                    identifier = entity.Uri.Split('#')[1],
                    id = (entity.Label + entity.Repository).GetHashCode().ToString(),
                    text = label,
                    expanded = false,
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
                        nodeType = "async",
                        type = (prefix.Equals("rdl")) ? "ClassNode" : "TemplateNode",
                        icon = (prefix.Equals("rdl")) ? "Content/img/class.png" : "Content/img/template.png",
                        identifier = entity.identifier.Split('#')[1],
                        id = (label + entity.repositoryName).GetHashCode().ToString(),
                        text = label,
                        expanded = false,
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
                    JsonTreeNode clasifNode = new JsonTreeNode
                    {
                        id = ("Classifications" + label).GetHashCode().ToString(),
                        children = new List<JsonTreeNode>(),
                        iconCls = "folder",
                        leaf = false,
                        text = "Classifications",
                        type = "ClassificationsNode",
                        identifier = null,
                        expanded = false
                    };

                    JsonTreeNode supersNode = new JsonTreeNode
                    {
                        id = ("Superclasses" + label).GetHashCode().ToString(),
                        children = new List<JsonTreeNode>(),
                        iconCls = "folder",
                        leaf = false,
                        text = "Superclasses",
                        type = "SuperclassesNode",
                        expanded = false
                    };

                    JsonTreeNode subsNode = new JsonTreeNode
                    {
                        id = ("Subclasses" + label).GetHashCode().ToString(),
                        children = new List<JsonTreeNode>(),
                        iconCls = "folder",
                        leaf = false,
                        text = "Subclasses",
                        type = "SubclassesNode",
                        expanded = false
                    };

                    JsonTreeNode tempsNode = new JsonTreeNode
                    {
                        id = ("Templates" + label).GetHashCode().ToString(),
                        children = new List<JsonTreeNode>(),
                        iconCls = "folder",
                        leaf = false,
                        text = "Templates",
                        type = "ClassTemplatesNode",
                        expanded = false
                    };
                    #endregion

                    #region Add Hidden node for Properties------------
                    string reference = string.Empty;
                    if (entity.entityType!= null){
                        reference=Convert.ToString(entity.entityType.reference);
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
                    JsonTreeNode hiddenNode = new JsonTreeNode
                    {
                        hidden = true,
                        record = properties
                    };
                    nodes.Add(hiddenNode);
                    #endregion

                    nodes.Add(clasifNode); // Add Classification node.
                    nodes.Add(supersNode); // Add SuperClassNode.

                    #region Fill Data in Classification node--------
                    foreach (var classification in entity.classification)
                    {

                        JsonTreeNode leafNode = new JsonTreeNode
                        {
                            type = "ClassNode",
                            icon = "Content/img/class.png",
                            leaf = false,
                            identifier = classification.reference.Split('#')[1],
                            id = (classification.label),
                            text = classification.label,
                            expanded = false,
                            children = null,
                            record = classification
                        };

                        clasifNode.children.Add(leafNode);
                    }
                    clasifNode.text = clasifNode.text + "(" + clasifNode.children.Count() + ")";
                    if (clasifNode.children.Count() == 0)
                    {
                        clasifNode.leaf = true;
                        clasifNode.icon = "Content/img/folder.png";
                    }
                    #endregion

                    #region Fill Data in SuperClass node--------
                    foreach (var specialization in entity.specialization)
                    {

                        JsonTreeNode leafNode = new JsonTreeNode
                        {
                            type = "ClassNode",
                            icon = "Content/img/class.png",
                            leaf = false,
                            identifier = specialization.reference.Split('#')[1],
                            id = (specialization.label),
                            text = specialization.label,
                            expanded = false,
                            children = null,
                            record = specialization
                        };

                        supersNode.children.Add(leafNode);
                    }
                    supersNode.text = supersNode.text + "(" + supersNode.children.Count() + ")";
                    if (supersNode.children.Count() == 0)
                    {
                        supersNode.leaf = true;
                        supersNode.icon = "Content/img/folder.png";
                    }
                    #endregion

                    //Get Sub Classes
                    JsonTreeNode subClassNodes = GetSubClasses(classId, subsNode);

                    if (subClassNodes.children.Count() == 0)
                    {
                        subClassNodes.leaf = true;
                        subClassNodes.icon = "Content/img/folder.png";
                    }
                    nodes.Add(subClassNodes);

                    ////Get Templates
                    //JsonTreeNode templateNodes = GetTemplates(classId, tempsNode);
                    //if (templateNodes.children.Count() == 0)
                    //{
                    //    templateNodes.leaf = true;
                    //    templateNodes.icon = "Content/img/folder.png";
                    //}

                    //nodes.Add(templateNodes);
                }
            }

            return nodes;
        }

        private JsonTreeNode GetSubClasses(string classId, JsonTreeNode subsNode)
        {
            if (!string.IsNullOrEmpty(classId))
            {
                Entities dataEntities = _refdataRepository.GetSubClasses(classId);
                foreach (var entity in dataEntities)
                {
                    JsonTreeNode node = new JsonTreeNode
                    {
                        nodeType = "async",
                        type = "ClassNode",
                        icon = "Content/img/class.png",
                        identifier = entity.Uri.Split('#')[1],
                        id = (entity.Label + entity.Repository).GetHashCode().ToString(),
                        text = entity.Label,
                        expanded = false,
                        leaf = false,
                        children = null,
                        record = entity
                    };

                    subsNode.children.Add(node);
                }
                subsNode.text = subsNode.text + "(" + subsNode.children.Count() + ")";
            }

            return subsNode;
        }
        //private List<JsonTreeNode> GetSubClasses(string classId)
        //{
        //    List<JsonTreeNode> nodes = new List<JsonTreeNode>();

        //    if (!string.IsNullOrEmpty(classId))
        //    {
        //        Entities dataEntities = _refdataRepository.GetSubClasses(classId);
        //        foreach (var entity in dataEntities)
        //        {
        //            JsonTreeNode node = new JsonTreeNode
        //            {
        //                nodeType = "async",
        //                type = "SubclassNode",
        //                icon = "Content/img/class.png",
        //                identifier = entity.Uri.Split('#')[1],
        //                id = (entity.Label + entity.Repository).GetHashCode().ToString(),
        //                text = entity.Label,
        //                expanded = false,
        //                leaf = false,
        //                children = GetDefaultChildren(entity.Label),
        //                record = entity
        //            };

        //            nodes.Add(node);
        //        }
        //    }

        //    return nodes;
        //}

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
                        nodeType = "async",
                        type = "SuperClassNode",
                        icon = "Content/img/class.png",
                        identifier = entity.Uri.Split('#')[1],
                        id = (entity.Label + entity.Repository).GetHashCode().ToString(),
                        text = entity.Label,
                        expanded = false,
                        leaf = false,
                        children = GetDefaultChildren(entity.Label),
                        record = entity
                    };
                    nodes.Add(node);
                }
            }

            return nodes;
        }

        private JsonTreeNode GetTemplates(string classId, JsonTreeNode tempsNode)
        {

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
                        id = (entity.Label + entity.Repository).GetHashCode().ToString(),
                        identifier = entity.Uri.Split('#')[1],
                        text = entity.Label,
                        expanded = false,
                        leaf = false,
                        children = null,
                        record = entity
                    };

                    tempsNode.children.Add(node);
                }
                tempsNode.text = tempsNode.text + "(" + tempsNode.children.Count() + ")";
            }

            return tempsNode;
        }

        //private List<JsonTreeNode> GetTemplates(string classId)
        //{
        //    List<JsonTreeNode> nodes = new List<JsonTreeNode>();

        //    if (!string.IsNullOrEmpty(classId))
        //    {
        //        Entities dataEntities = _refdataRepository.GetClassTemplates(classId);
        //        foreach (var entity in dataEntities)
        //        {
        //            JsonTreeNode node = new JsonTreeNode
        //            {
        //                nodeType = "async",
        //                type = "TemplateNode",
        //                icon = "Content/img/template.png",
        //                id = (entity.Label + entity.Repository).GetHashCode().ToString(),
        //                identifier = entity.Uri.Split('#')[1],
        //                text = entity.Label,
        //                expanded = false,
        //                leaf = false,
        //                children = null,
        //                record = entity
        //            };

        //            nodes.Add(node);
        //        }
        //    }

        //    return nodes;
        //}
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
                        children = null,
                        text = entity.classDefinitions[0].name[0].value,
                        id = ("ClassMap" + entity.classDefinitions[0].name[0].value).GetHashCode().ToString(),
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
                                id = ("Roles" + role.name[0].value).GetHashCode().ToString(),
                                type = "RoleNode",
                                icon = "Content/img/role.png",
                                children = null,
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
                            JsonTreeNode entityNode = new JsonTreeNode
                            {
                                id = ("Roles" + role.name[0].value).GetHashCode().ToString(),
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

