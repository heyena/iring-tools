using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OntologyService.Interface.Types;

namespace OntologyService.Interface.Entities
{
    /// <summary>
    /// Returns information about the specified entity
    /// </summary>
    public class EntitySearchInfo
    {
        Regex regCase = new Regex("([A-Z]+[a-z]+)");

        public bool IsTemplate { get; set; }
        public bool IsProperty { get; set; }
        public bool IsClassification { get; set; }
        public bool IsRole { get; set; }
        public bool IsClass { get; set; }
        public bool IsSearch { get; set; }
        public bool IsSandbox { get; set; }
        public bool IsExact { get; set; }
        public bool IsContains { get; set; }
        public bool IsSubclass { get; set; }
        public bool IsSuperclass { get; set; }

        public EntityType Entity { get; set; }
        public QueryType Query { get; set; }
        public SearchType Search { get; set; }

        /// <summary>
        /// Returns the formatted query template
        /// </summary>
        /// <value>The query template.</value>
        public string QueryTemplate { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySearchInfo"/> class.
        /// Sets flags and query template string based on naming convention
        /// - PascalCase is used to generate paths,e.g. HelloWorld becomes hello/world
        /// - Underscores delimit parameter, e.g., Hello_World becomes hello/{0}/world
        /// </summary>
        /// <param name="queryType">Type of the query.</param>
        public EntitySearchInfo(QueryType queryType)
        {
            Query = queryType;

            switch (queryType)
            {
                case QueryType.TemplatesAdd:
                case QueryType.TemplatesPost:
                    QueryTemplate = "/templates";
                    break;

                default:
                    List<string> preList =
                        queryType.ToString().Split('_').ToList<string>();

                    List<string> paraList = new List<string>();

                    // Split PascalCase and insert forward slash
                    foreach (string para in preList)
                    {
                        string addSlash = regCase.Replace(para,
                            m => (m.Value.Length > 3 ? m.Value : m.Value.ToLower()) + " ")
                            .ToLower();

                        // If no PascalCase string is found we want to ensure
                        // a slash isn't appended.
                        if (addSlash.Trim().Split(' ').Count<string>() == 1)
                            paraList.Add(para.ToLower());
                        else
                            paraList.Add(addSlash.Replace(" ", "/"));
                    }

                    // Build QueryTemplate
                    switch (paraList.Count)
                    {
                        case 1:
                            QueryTemplate = string.Format("/{0}/{{0}}",
                                paraList[0]).ToLower().Replace("//", "/");
                            break;
                        case 2:
                            QueryTemplate = string.Format("/{0}/{{0}}/{1}",
                                paraList[0], paraList[1]);
                            break;
                        default:
                            break;
                    }
                    break;
            }

            // Set flags as applicable
            IsClass = QueryTemplate.Contains("/classes");
            IsTemplate = QueryTemplate.Contains("/templates");
            IsRole = QueryTemplate.Contains("/roles");
            IsProperty = QueryTemplate.Contains("/properties");
            IsSearch = QueryTemplate.Contains("/search")
                    || QueryTemplate.Contains("/esearch");
            IsContains = QueryTemplate.Contains("/search");
            IsExact = QueryTemplate.Contains("/esearch");
            IsSandbox = QueryTemplate.Contains("/sandbox");
            IsSubclass = QueryTemplate.Contains("/subclasses");
            IsSuperclass = QueryTemplate.Contains("/superclasses");
            IsClassification = QueryTemplate.Contains("/classification");
        }

    }
}
