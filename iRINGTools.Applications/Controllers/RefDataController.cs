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

        IRefDataRepository _refdataRepository = null;
        NamespaceMapper _nsMap = new NamespaceMapper();
        string _adapterServiceURI = String.Empty;
        string _refDataServiceURI = String.Empty;

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

            string id = form["id"];
            string searchtype = form["type"];
            string query = form["query"];
            Int32.TryParse(form["limit"], out limit);
            Int32.TryParse(form["start"], out start);

            List<JsonTreeNode> nodes = null;

            if (!string.IsNullOrEmpty(query))
            {
                switch (searchtype)
                {
                    case "SearchNode":
                        nodes = GetClasses(query, start, limit);
                        break;
                    case "ClassificationsNode":
                        nodes = GetClasses(id);
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
                        nodes = GetRoleClass(id);
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

        private List<JsonTreeNode> GetClasses(string query, int start, int limit)
        {
            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

            RefDataEntities dataEntities = _refdataRepository.Search(query, start, limit);

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
                    //  children = (prefix.Equals("rdl")) ? GetDefaultChildren(label) : null,
                    record = entity
                };

                nodes.Add(node);
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
                    var label = entity.name[0].value;
                    JsonTreeNode clasifNode = new JsonTreeNode
                    {
                        id = ("Classifications" + label).GetHashCode().ToString(),
                        children = new List<JsonTreeNode>(),
                        leaf = false,
                        text = "Classifications",
                        type = "ClassificationsNode",
                        identifier = null,
                        expanded = false
                    };
                    nodes.Add(clasifNode);
                    JsonTreeNode supersNode = new JsonTreeNode
                    {
                        id = ("Superclasses" + label).GetHashCode().ToString(),
                        children = new List<JsonTreeNode>(),
                        leaf = false,
                        text = "Superclasses",
                        type = "SuperclassesNode",
                        expanded = false
                    };
                    nodes.Add(supersNode);
                    JsonTreeNode subsNode = new JsonTreeNode
                    {
                        id = ("Subclasses" + label).GetHashCode().ToString(),
                        //children = new List<JsonTreeNode>(),
                        leaf = false,
                        text = "Subclasses",
                        type = "SubclassesNode",
                        expanded = false
                    };
                    nodes.Add(subsNode);
                    JsonTreeNode tempsNode = new JsonTreeNode
                    {
                        id = ("Templates" + label).GetHashCode().ToString(),
                      //  children = new List<JsonTreeNode>(),
                        leaf = false,
                        text = "Templates",
                        type = "ClassTemplatesNode",
                        expanded = false
                    };
                    nodes.Add(tempsNode);
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
     
                }
            }
            return nodes;
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
                        nodeType = "async",
                        type = "SubclassNode",
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
                        id = (entity.Label + entity.Repository).GetHashCode().ToString(),
                        identifier = entity.Uri.Split('#')[1],
                        text = entity.Label,
                        expanded = false,
                        leaf = false,
                        children = null,
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
                Entity entity = _refdataRepository.GetClassLabel(id);
                if (entity != null && entity.Uri != null)
                {
                    JsonTreeNode classNode = new JsonTreeNode
                    {
                        identifier = entity.Uri.Split('#')[1],
                        leaf = false,
                        children = null,
                        id = ("ClassMap" + entity.Label).GetHashCode().ToString(),
                        record = entity,
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
                            JsonTreeNode entityNode = new JsonTreeNode
                            {
                                id = ("Roles" + role.name[0].value).GetHashCode().ToString(),
                                type = "RoleNode",
                                text = role.name[0].value,
                                icon = "Content/img/role.png",
                                children = null,
                                leaf = false,
                                identifier = role.identifier,
                                record = role
                            };
                            nodes.Add(entityNode);
                        }
                    }
                }
            }

            return nodes;
        }
    }
}

