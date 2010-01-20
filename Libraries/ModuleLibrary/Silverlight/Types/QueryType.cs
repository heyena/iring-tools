
namespace org.iringtools.ontologyservice.presentation.types
{
    public enum QueryType
    {
        /// <summary>
        /// GetClassRelations(string ID)
        /// "/classes/" + ID +"/classes"
        /// </summary>
        Classes_Classes,

        /// <summary>
        /// GetClassification(string ID)
        /// "/classes/" + ID + "/classification";
        /// </summary>
        Classes_Classification,

        /// <summary>
        /// GetProperties(string ID)
        /// "/classes/" + ID + "/properties"
        /// </summary>
        Classes_Properties,

        /// <summary>
        /// GetSubClass(string ID)
        /// "/classes/" + ID + "/subclasses"
        /// </summary>
        Classes_Subclasses,

        /// <summary>
        /// GetSuperClass(string ID)
        /// "/classes/" + ID + "/superclasses"
        /// </summary>
        Classes_Superclasses,

        /// <summary>
        /// GetRelations(string ID)
        /// "/classes/" + ID + "/templates"
        /// </summary>
        Classes_Templates,

        /// <summary>
        /// ExactSearch(string stringToFind, string offset)
        /// "/esearch/" + stringToFind + "/" + offset;
        /// </summary>
        Esearch,

        /// <summary>
        /// GetRoleData(string ID)
        /// "/roles/" + ID
        /// </summary>
        Roles,

        /// <summary>
        /// SandboxExactSearch(string stringToFind, string offset)
        /// "/sandbox/esearch/" + stringToFind + "/" + offset;
        /// </summary>
        SandboxEsearch,

        /// <summary>
        /// SandboxContainsSearch(string stringToFind, string offset)
        /// "/sandbox/search/" + stringToFind + "/" + offset
        /// </summary>
        SandboxSearch,

        /// <summary>
        /// SandboxTemplateExactSearch(string stringToFind, string offset)
        ///  "/sandbox/templates/esearch/" + stringToFind + "/" + offset;
        /// </summary>
        SandboxTemplatesEsearch,

        /// <summary>
        /// SandboxTemplateContainsSearch(string stringToFind, string offset)
        /// "/sandbox/templates/search/" + stringToFind + "/" + offset;
        /// </summary>
        SandboxTemplatesSearch,

        /// <summary>
        /// ContainsSearch(string stringToFind, string offset)
        /// "/search/" + stringToFind + "/" + offset
        /// </summary>
        Search,


        /// <summary>
        /// GetStatus(string ID)
        /// "/templates/" + ID
        /// </summary>
        Templates,

        /// <summary>
        /// AddTemplate(string path, bool isBoolean)
        /// /templates
        /// </summary>
        TemplatesAdd,

        /// <summary>
        /// PostTemplates(QMXF templates)
        /// "/templates"
        /// </summary>
        TemplatesPost,

        /// <summary>
        /// TemplateExactSearch(string stringToFind, string offset)
        /// "/templates/esearch/" + stringToFind + "/" + offset;
        /// </summary>
        TemplatesEsearch,

        /// <summary>
        /// TemplateContainsSearch(string stringToFind, string offset)
        /// "/templates/search/" + stringToFind + "/" + offset
        /// </summary>
        TemplatesSearch,

        /// <summary>
        /// TemplateGetProperties(string ID)
        /// "/templates/" + ID + "/properties"
        /// </summary>
        Templates_Properties,

        /// <summary>
        /// GetRoles(string ID)
        /// "/templates/" + ID + "/roles"
        /// </summary>
        Templates_Roles,




    }
}
