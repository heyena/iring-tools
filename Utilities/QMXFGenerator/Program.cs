using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;
using IDS_ADI.InformationModel;
using org.ids_adi.qmxf;
using org.iringtools.utility;
using ExtremeML.Packaging;
using ExtremeML.Spreadsheet;
using System.Text.RegularExpressions;
using ExtremeML.Spreadsheet.Address;

namespace QMXFGenerator
{
    class Program
    {
        #region Private Members
        private static string _excelFilePath = String.Empty;
        private static string _qmxfFilePath = String.Empty;
        private static string _proxyHost = String.Empty;
        private static string _proxyPort = String.Empty;
        private static string _proxyCredentials = String.Empty;
        private static string _idsADICredentials = String.Empty;
        private static string _classRegistryBase = String.Empty;
        private static string _templateRegistryBase = String.Empty;
        private static string _targetRepository = string.Empty;
        private static SpreadsheetDocumentWrapper document = null;

        private static WorksheetPartWrapper _classWorksheet = null;
        private static WorksheetPartWrapper _classSpecializationWorksheet = null;
        private static WorksheetPartWrapper _baseTemplateWorksheet = null;
        private static WorksheetPartWrapper _classificationWorksheet = null;
        private static WebHttpClient _refdataClient = null;
        private static string _refdataServiceUri = null;
        private static List<ArrayList> _classes = new List<ArrayList>();
        private static List<ArrayList> _classSpecializations = new List<ArrayList>();
        private static List<ArrayList> _classifications = new List<ArrayList>();
        private static List<ArrayList> _baseTemplates = new List<ArrayList>();
        private static List<ArrayList> _siTemplates = new List<ArrayList>();
        private static List<ArrayList> _roles = new List<ArrayList>();
        #endregion

        static void Main(string[] args)
        {

            try
            {
                {

                    if (Initialize(args))
                        using (document = SpreadsheetDocumentWrapper.Open(_excelFilePath))
                        {

                            _refdataClient = new WebHttpClient(_refdataServiceUri);
                            QMXF qmxf = new QMXF();

                            _classWorksheet = GetWorksheet(document, "Class");
                            _classSpecializationWorksheet = GetWorksheet(document, "Class Specialization");

                            Console.WriteLine("Processing Classes...");
                            qmxf.classDefinitions = ProcessClass(_classWorksheet, _classSpecializationWorksheet);
                            Console.WriteLine("  processed " + _classes.Count + " classes.");
                            _classificationWorksheet = GetWorksheet(document, "Classification");

                            Console.WriteLine("Processing Classifications...");
                            ProcessClassDefinitions(_classificationWorksheet, qmxf.classDefinitions);

                            _baseTemplateWorksheet = GetWorksheet(document, "Base Template");

                            Console.WriteLine("Processing Base Templates...");
                            qmxf.templateDefinitions = ProcessBaseTemplate(_baseTemplateWorksheet);
                            Console.WriteLine("  processed " + _baseTemplates.Count + " base templates.");
                            WorksheetPartWrapper specializedIndividualTemplateWorksheet = GetWorksheet(document, "Specialized Individual Template");

                            Console.WriteLine("Processing Specialized Individual Templates...");
                            qmxf.templateQualifications = ProcessSpecializedIndividualTemplates(specializedIndividualTemplateWorksheet);
                            Console.WriteLine("  processed " + _siTemplates.Count + " templates.");

                            specializedIndividualTemplateWorksheet = null;

                            _baseTemplateWorksheet = null;

                            _classSpecializationWorksheet = null;

                            _classWorksheet = null;

                            ///Post Classes and Templates induvidually to refdataService
                            ///lets write the qmxf 
                            Utility.Write<QMXF>(qmxf, _qmxfFilePath, false);
                            foreach (var cls in qmxf.classDefinitions)
                            {
                                var q = new QMXF { targetRepository = _targetRepository };
                                q.classDefinitions.Add(cls);
                                _refdataClient.Post<QMXF>("/classes", q, true);
                                Console.WriteLine("Success: posted class: " + cls.name[0].value);
                            }

                            ///Post baseTemplates
                            foreach (var t in qmxf.templateDefinitions)
                            {
                                var q = new QMXF { targetRepository = _targetRepository };
                                q.templateDefinitions.Add(t);
                                _refdataClient.Post<QMXF>("/templates", q, true);
                                Console.WriteLine("Success: posted baseTemplate: " + t.name[0].value);
                            }

                            ///Port Specialised templates
                            foreach (var t in qmxf.templateQualifications)
                            {
                                var q = new QMXF { targetRepository = _targetRepository };
                                q.templateQualifications.Add(t);
                                _refdataClient.Post<QMXF>("/templates", q, true);
                                Console.WriteLine("Success: posted spesialized template: ", t.name[0].value);
                            }

                            Console.WriteLine("Success: qmxf was generated: " + _qmxfFilePath);
                        }
                }
            }
            catch (Exception ex)
            {
                Utility.WriteString("\n" + ex.ToString() + "\n", "error.log", true);
                Console.WriteLine("Failure: See log file: error.log");
            }

            Console.ReadKey();
        }

        private static WorksheetPartWrapper GetWorksheet(SpreadsheetDocumentWrapper document, string sheetName)
        {
            var sheet = document.WorkbookPart.WorksheetParts[sheetName];
            //   var sheet = document.WorkbookPart.GetTablePart(sheetName).Table;
            return sheet;
        }

        private string GetCellValue(WorksheetPartWrapper part, int startCol, int startRow)
        {
            //string reference = startCol + startRow;
            var row = part.Worksheet.SheetData.Rows.FirstOrDefault(c => c.RowIndex == startRow);
            var col = row.GetCell(startCol, false);
            //get exact cell based on reference 
            return col.CellValue.Value;
        }

        private static void ProcessClassDefinitions(WorksheetPartWrapper _classificationWorksheet, List<ClassDefinition> list)
        {
            try
            {
                _classifications = MarshallToList(_classificationWorksheet);
                foreach (var c in _classifications)
                {

                    var query = from cls in _classes
                                where cls[(int)ClassColumns.Label].ToString().Trim().Equals(c[(int)ClassificationColumns.Classified].ToString())
                                select cls[(int)ClassColumns.ID];
                    var cl = list.SingleOrDefault(l => l.name[0].value.Equals(c[(int)ClassificationColumns.Class].ToString()));
                    if (cl != null && query != null && query.Count() > 0)
                    {
                        cl.classification.Add(new Classification { label = c[(int)ClassificationColumns.Classified].ToString(), lang = "en", reference = query.FirstOrDefault().ToString() });
                    }
                    else
                    {
                        Console.WriteLine("Error: classification: " + c[(int)ClassificationColumns.Classified].ToString() + " off class " + c[(int)ClassificationColumns.Class].ToString() + " not valid..");
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        private static bool Initialize(string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    _excelFilePath = System.Configuration.ConfigurationManager.AppSettings["ExcelFilePath"];
                    _qmxfFilePath = System.Configuration.ConfigurationManager.AppSettings["QMXFFilePath"];
                    _targetRepository = System.Configuration.ConfigurationManager.AppSettings["TargetRepositoryName"];
                    _refdataServiceUri = System.Configuration.ConfigurationManager.AppSettings["RefdataServiceUri"];
                }
                else
                {
                    _excelFilePath = args[0];
                    _qmxfFilePath = args[1];

                }

                if (_excelFilePath == String.Empty || _qmxfFilePath == String.Empty)
                {
                    Console.WriteLine("Usage: \n");
                    Console.WriteLine("   qmxfgen.exe <excel file> <output file>");
                    return false;
                }

                _proxyHost = System.Configuration.ConfigurationManager.AppSettings["ProxyHost"];
                _proxyPort = System.Configuration.ConfigurationManager.AppSettings["ProxyPort"];
                _proxyCredentials = System.Configuration.ConfigurationManager.AppSettings["ProxyCredentialToken"];
                _idsADICredentials = System.Configuration.ConfigurationManager.AppSettings["IDSADICredentialToken"];

                bool useTestRegistry = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["UseTestRegistry"]);

                if (useTestRegistry)
                {
                    string testRegistryBase = System.Configuration.ConfigurationManager.AppSettings["TestRegistryBase"];

                    _classRegistryBase = testRegistryBase;
                    _templateRegistryBase = testRegistryBase;
                }
                else
                {
                    _classRegistryBase = System.Configuration.ConfigurationManager.AppSettings["ClassRegistryBase"];
                    _templateRegistryBase = System.Configuration.ConfigurationManager.AppSettings["TemplateRegistryBase"];
                }

                return true;
            }
            catch (Exception ex)
            {
                Utility.WriteString("\nError Initializing \n" + ex.ToString() + "\n", "error.log", true);
                throw ex;
            }
        }

        private static List<ClassDefinition> ProcessClass(WorksheetPartWrapper classPart, WorksheetPartWrapper specializationPart)
        {
            int rowIndex = 0;
            try
            {
                _classes = MarshallToList(classPart);
                _classSpecializations = MarshallToList(specializationPart);

                List<ClassDefinition> classDefinitions = new List<ClassDefinition>();
                foreach (ArrayList row in _classes)
                {
                    object load = row[(int)ClassColumns.Load];

                    if (load != null && load.ToString().Trim() != String.Empty)
                    {
                        object identifier = row[(int)ClassColumns.ID];
                        object label = row[(int)ClassColumns.Label];
                        object description = row[(int)ClassColumns.Description];
                        object entityType = row[(int)ClassColumns.EntityType];

                        ClassDefinition classDefinition = new ClassDefinition();

                        string name = String.Empty;
                        if (label != null)
                        {
                            name = label.ToString();

                            if (name != String.Empty)
                            {
                                QMXFName englishUSName = new QMXFName
                                {
                                    lang = "en",
                                    value = name,
                                };

                                classDefinition.name = new List<QMXFName>
                { 
                  englishUSName 
                };
                            }
                        }

                        if (identifier == null || identifier.ToString() == String.Empty)
                        {
                            identifier = GenerateID(_classRegistryBase, name);
                            //write to the in-memory list
                            _classes[rowIndex][(int)ClassColumns.ID] = identifier;
                            //write to the sheet, but offset counters for 1-based array
                            classPart.Worksheet.SetCellValue(new GridReference(rowIndex + 1, (int)ClassColumns.ID + 1), identifier);
                        }

                        classDefinition.identifier = identifier.ToString();
                        if (description != null && description.ToString() != String.Empty)
                        {
                            Description englishUSDescription = new Description
                            {
                                lang = "en",
                                value = description.ToString(),
                            };

                            classDefinition.description = new List<Description> 
              {
                englishUSDescription,
              };
                        }
                        string ent = entityType.ToString();
                        if (!string.IsNullOrEmpty(ent))
                        {
                            classDefinition.entityType = new EntityType
                            {
                                reference = ent
                            };
                        }

                        List<Specialization> classSpecialization = ProcessClassSpecialization(name);

                        if (classSpecialization.Count > 0)
                            classDefinition.specialization = classSpecialization;

                        load = String.Empty;
                        if (entityType != null)
                            classDefinitions.Add(classDefinition);
                    }

                    rowIndex++;
                }
                return classDefinitions;
            }
            catch (Exception ex)
            {
                Utility.WriteString("\nError Processing Class \n Worksheet: " + classPart +
                                    "\t Row: " + rowIndex + " \n" + ex.ToString() + "\n", "error.log");
                throw ex;
            }
        }

        private static string GenerateID(string registryBase, string name)
        {
            try
            {
                string identifier = String.Empty;

                WebCredentials webCredentials = null;
                if (_idsADICredentials != String.Empty)
                {
                    webCredentials = new WebCredentials(_idsADICredentials);
                    webCredentials.Decrypt();
                }

                WebProxy webProxy = null;
                if (_proxyHost != String.Empty)
                {
                    WebCredentials proxyCredentials = new WebCredentials(_proxyCredentials);

                    webProxy = new WebProxy(_proxyHost, Convert.ToInt32(_proxyPort));
                    webProxy.Credentials = proxyCredentials.GetNetworkCredential();
                }

                WebHttpClient webHttpClient = new WebHttpClient(
                  "https://secure.ids-adi.org/registry",
                  webCredentials.GetNetworkCredential(),
                  webProxy);

                string registryOperation = HttpUtility.UrlEncode("acquire");
                string encodedRegistryBase = HttpUtility.UrlEncode(registryBase);
                string registryComment = HttpUtility.UrlEncode(name);

                string relativeUri =
                  "?registry-op=" + registryOperation +
                  "&registry-base=" + encodedRegistryBase +
                  "&registry-comment=" + registryComment;

                RegistryResult registryResult = webHttpClient.Get<RegistryResult>(relativeUri, false);
                //Utility.WriteString("\n" + registryResult.registryid, "Generated IDs.log", true);

                return registryResult.registryid;
            }
            catch (Exception ex)
            {
                Utility.WriteString("\nError Generating ID \n" + ex.ToString() + "\n", "error.log");
                throw ex;
            }
        }

        private static List<Specialization> ProcessClassSpecialization(string className)
        {
            try
            {
                List<Specialization> classSpecializations = new List<Specialization>();

                //Find the class specializations
                var specializationList = from specialization in _classSpecializations
                                         where Convert.ToString(specialization[(int)ClassSpecializationColumns.Superclass]) == className
                                         select specialization;

                //Get their details from the Class List
                List<ArrayList> superclasses = new List<ArrayList>();

                foreach (ArrayList specialization in specializationList)
                {
                    object subclass = specialization[(int)ClassSpecializationColumns.Subclass];

                    var query = from @class in _classes
                                where Convert.ToString(@class[(int)ClassColumns.Label]).Trim() == subclass.ToString().Trim()
                                select @class;

                    if (query.Count() > 0 && query.FirstOrDefault().Count > 0)
                    {
                        superclasses.Add(query.FirstOrDefault());
                    }
                    else
                    {
                        Utility.WriteString("\n " + subclass.ToString() + " Was Not Found in Class List", "error.log", true);
                    }
                }

                //Use the details for each to create the Specializations and add to List to return
                foreach (ArrayList superClassRow in superclasses)
                {
                    object superclassIdentifier = superClassRow[(int)ClassColumns.ID];
                    object superclassName = superClassRow[(int)ClassColumns.Label];

                    if (superclassIdentifier != null && superclassName != null &&
                        superclassIdentifier.ToString() != String.Empty)
                    {
                        Specialization specialization = new Specialization
                        {
                            label = superclassName.ToString(),
                            reference = superclassIdentifier.ToString().Trim(),
                        };

                        classSpecializations.Add(specialization);
                    }
                }

                return classSpecializations;
            }
            catch (Exception ex)
            {
                Utility.WriteString("\nError Processing Class Specialization \n" +
                                    "Worksheet: " + _classSpecializationWorksheet + " \n" + ex.ToString() + "\n", "error.log", true);
                throw ex;
            }
        }

        private static List<TemplateDefinition> ProcessBaseTemplate(WorksheetPartWrapper part)
        {
            int rowIndex = 0;
            try
            {
                _baseTemplates = MarshallToList(part);
                List<TemplateDefinition> templateDefinitions = new List<TemplateDefinition>();
                foreach (ArrayList row in _baseTemplates)
                {
                    object load = row[(int)TemplateColumns.Load];
                    if (load != null && load.ToString().Trim() != String.Empty && load.ToString() != "Load")
                    {
                        object templateIdentifier = row[(int)TemplateColumns.ID];
                        object templateName = row[(int)TemplateColumns.Name];
                        object description = row[(int)TemplateColumns.Description];
                        TemplateDefinition templateDefinition = new TemplateDefinition();
                        string name = String.Empty;
                        if (templateName != null)
                        {
                            name = templateName.ToString();
                            if (name != String.Empty)
                            {
                                QMXFName englishUSName = new QMXFName
                                {
                                    lang = "en",
                                    value = name,
                                };

                                templateDefinition.name = new List<QMXFName>
                                { 
                                  englishUSName 
                                };
                            }
                        }
                        if (templateIdentifier == null || templateIdentifier.ToString() == String.Empty)
                        {
                            templateIdentifier = GenerateID(_templateRegistryBase, name);
                            //write to the in-memory list
                            _baseTemplates[rowIndex][(int)TemplateColumns.ID] = templateIdentifier;
                            //write to the sheet, but offset counters for 1-based array
                            part.Worksheet.SetCellValue(new GridReference(rowIndex + 1, (int)TemplateColumns.ID + 1), templateIdentifier);
                        }
                        templateDefinition.identifier = templateIdentifier.ToString().Trim();
                        if (description != null && description.ToString() != String.Empty)
                        {
                            Description englishUSDescription = new Description
                            {
                                lang = "en",
                                value = description.ToString(),
                            };
                            templateDefinition.description = new List<Description> 
                            {
                                englishUSDescription,
                            };
                        }
                        templateDefinition.roleDefinition = ProcessRoleDefinition(templateDefinition.name.FirstOrDefault().value, row, rowIndex, part);
                        load = String.Empty;
                        templateDefinitions.Add(templateDefinition);
                    }
                    rowIndex++;
                }
                return templateDefinitions;
            }
            catch (Exception e)
            {
                Utility.WriteString("\nError Processing Template \n Worksheet: " + part.Name + "\tRow: "
                                     + rowIndex + " \n" + e.ToString() + "\n", "error.log", true);
                throw e;
            }
        }

        private static List<RoleDefinition> ProcessRoleDefinition(string templateName, ArrayList row, int rowIndex, WorksheetPartWrapper part)
        {
            try
            {
                List<RoleDefinition> roleDefinitions = new List<RoleDefinition>();
                for (int roleIndex = 0; roleIndex <= (int)RoleColumns.Max - 1; roleIndex++)
                {
                    int roleOffset = (int)TemplateColumns.Roles + ((int)RoleColumns.Count * roleIndex);

                    object identifier = row[(int)RoleColumns.ID + roleOffset];
                    object label = row[(int)RoleColumns.Name + roleOffset];
                    object description = row[(int)RoleColumns.Description + roleOffset];
                    object type = row[(int)RoleColumns.Type + roleOffset];

                    if (label != null && label.ToString().Trim() != String.Empty)
                    {
                        string name = label.ToString();
                        RoleDefinition roleDefinition = new RoleDefinition();

                        QMXFName englishUSName = new QMXFName
                        {
                            lang = "en",
                            value = name,
                        };

                        roleDefinition.name = new List<QMXFName>
            { 
              englishUSName 
            };

                        if (identifier == null || identifier.ToString() == String.Empty)
                        {
                            identifier = GenerateID(_templateRegistryBase, name);

                            //write to the in-memory list
                            _baseTemplates[rowIndex][(int)RoleColumns.ID + roleOffset] = identifier;

                            //write to the sheet, but offset counters for 1-based array
                            part.Worksheet.SetCellValue(new GridReference(rowIndex + 1, (int)RoleColumns.ID + roleOffset + 1), identifier);
                        }

                        roleDefinition.identifier = identifier.ToString();

                        if (description != null && description.ToString() != String.Empty)
                        {
                            Description englishUSDescription = new Description
                            {
                                lang = "en",
                                value = description.ToString(),
                            };

                            roleDefinition.description = englishUSDescription;
                        }

                        if (type != null && type.ToString() != String.Empty)
                        {
                            var query = from @class in _classes
                                        where Convert.ToString(@class[(int)ClassColumns.Label]) == type.ToString()
                                        select @class;
                            if (query.FirstOrDefault() != null && query.FirstOrDefault()[(int)ClassColumns.Label].ToString().Trim().Equals(type.ToString()))
                            {
                                roleDefinition.range = query.FirstOrDefault()[(int)ClassColumns.ID].ToString().Trim();
                            }
                            else
                            {
                                Utility.WriteString("\n " + type.ToString() + " Was Not Found in Class List While Processing Role Definition", "error.log", true);
                            }
                        }
                        else
                        {
                            Utility.WriteString("\nType Was Not Set for Role Definition \"" + englishUSName.value + "\" on template \"" + templateName + "\".", "error.log", true);
                        }
                        roleDefinitions.Add(roleDefinition);
                    }
                }

                return roleDefinitions;
            }
            catch (Exception e)
            {
                Utility.WriteString("\nError Processing Role \n Row: " + rowIndex + " \n" + e.ToString() + "\n", "error.log", true);
                throw e;
            }
        }

        private static List<TemplateQualification> ProcessSpecializedIndividualTemplates(WorksheetPartWrapper part)
        {
            int rowIndex = 0;
            try
            {
                _siTemplates = MarshallToList(part);
                List<TemplateQualification> templateQualifications = new List<TemplateQualification>();
                foreach (ArrayList row in _siTemplates)
                {
                    object load = row[(int)TemplateColumns.Load];

                    if (load != null && load.ToString().Trim() != String.Empty && load.ToString() != "Load")
                    {
                        object templateIdentifier = row[(int)TemplateColumns.ID];
                        object templateName = row[(int)TemplateColumns.Name];
                        object description = row[(int)TemplateColumns.Description];
                        object parentTemplate = row[(int)TemplateColumns.ParentTemplate];
                        TemplateQualification templateQualification = new TemplateQualification();
                        string name = String.Empty;
                        if (templateName != null)
                        {
                            name = templateName.ToString();
                            if (name != String.Empty)
                            {
                                QMXFName englishUSName = new QMXFName
                                {
                                    lang = "en",
                                    value = name,
                                };
                                templateQualification.name = new List<QMXFName>
                    { 
                      englishUSName 
                    };
                            }
                        }
                        if (templateIdentifier == null || templateIdentifier.ToString() == String.Empty)
                        {
                            templateIdentifier = GenerateID(_templateRegistryBase, name);
                            //write to the in-memory list
                            _siTemplates[rowIndex][(int)TemplateColumns.ID] = templateIdentifier;
                            //write to the sheet, but offset counters for 1-based array
                            part.Worksheet.SetCellValue(new GridReference(rowIndex + 1, (int)TemplateColumns.ID + 1), templateIdentifier);
                        }
                        templateQualification.identifier = templateIdentifier.ToString().Trim();
                        if (description != null && description.ToString() != String.Empty)
                        {
                            Description englishUSDescription = new Description
                            {
                                lang = "en",
                                value = description.ToString(),
                            };
                            templateQualification.description = new List<Description> 
                  {
                    englishUSDescription,
                  };
                        }
                        if (parentTemplate != null && parentTemplate.ToString() != String.Empty)
                        {
                            var query = from template in _baseTemplates
                                        where Convert.ToString(template[(int)TemplateColumns.Name]) == parentTemplate.ToString()
                                        select template;

                            ArrayList parentRow = query.FirstOrDefault();
                            if (parentRow != null)
                            {
                                object templateQualifiesId = parentRow[(int)TemplateColumns.ID];
                                if (templateQualifiesId == null)
                                {
                                    Utility.WriteString("Template Qualification \"" + templateQualification.identifier + "\" qualifies ID not found.\n", "error.log", true);
                                }
                                templateQualification.qualifies = (templateQualifiesId ?? "").ToString().Trim();
                                templateQualification.roleQualification = ProcessRoleQualification(templateQualification.name.FirstOrDefault().value, row, parentRow);
                            }
                            else
                            {
                                Utility.WriteString(parentTemplate.ToString() + " Was Not Found in Template List While Processing Specialized Templates.\n", "error.log", true);
                            }
                        }
                        load = String.Empty;
                        templateQualifications.Add(templateQualification);
                    }
                    rowIndex++;
                }
                return templateQualifications;
            }
            catch (Exception ex)
            {
                Utility.WriteString("\nError Processing Individual Template \n" +
                                    "Worksheet: " + part.Name + "\tRow: " + rowIndex +
                                    " \n" + ex.ToString() + "\n", "error.log", true);
                throw ex;
            }
        }

        private static List<RoleQualification> ProcessRoleQualification(string templateName, ArrayList row, ArrayList parentRow)
        {
            int roleIndex = 0;

            try
            {
                List<RoleQualification> roleQualifications = new List<RoleQualification>();

                for (roleIndex = 0; roleIndex <= (int)RoleColumns.Max - 1; roleIndex++)
                {
                    int roleOffset = (int)TemplateColumns.Roles + ((int)RoleColumns.Count * roleIndex);

                    object label = row[(int)RoleColumns.Name + roleOffset];
                    object description = row[(int)RoleColumns.Description + roleOffset];
                    object type = row[(int)RoleColumns.Type + roleOffset];
                    object value = row[(int)RoleColumns.Value + roleOffset];

                    object parentRole = parentRow[(int)RoleColumns.ID + roleOffset];

                    if (label != null && label.ToString().Trim() != String.Empty)
                    {
                        string name = label.ToString();

                        if (parentRole == null)
                        {
                            Utility.WriteString("Error Processing Role Qualification: Role \"" + name + "\" at index " + roleIndex + " on template \"" + templateName + "\" not found.\n", "error.log", true);
                            continue;
                        }

                        RoleQualification roleQualification = new RoleQualification();

                        QMXFName englishUSName = new QMXFName
                        {
                            lang = "en",
                            value = name,
                        };

                        roleQualification.name = new List<QMXFName>
            { 
              englishUSName 
            };

                        if (description != null && description.ToString() != String.Empty)
                        {
                            Description englishUSDescription = new Description
                            {
                                lang = "en",
                                value = description.ToString(),
                            };

                            roleQualification.description = new List<Description>
              {
                englishUSDescription
              };
                        }

                        roleQualification.qualifies = (parentRole ?? "").ToString().Trim();

                        if (type != null && type.ToString() != String.Empty)
                        {
                            var query = from @class in _classes
                                        where Convert.ToString(@class[(int)ClassColumns.Label]).Trim() == type.ToString().Trim()
                                        select @class;

                            if (query.FirstOrDefault() != null)
                            {
                                object classId = query.FirstOrDefault()[(int)ClassColumns.ID];
                                if (classId != null)
                                {
                                    roleQualification.range = classId.ToString().Trim();
                                }
                                else
                                {
                                    Utility.WriteString("\n " + type.ToString() + " Does not have an id in Class List While Processing Role Qualification", "error.log", true);
                                }
                            }
                            else
                            {
                                Utility.WriteString("\n " + type.ToString() + " Was Not Found in Class List While Processing Role Qualification", "error.log", true);
                            }
                        }
                        else if (value != null && value.ToString() != String.Empty)
                        {
                            var query = from @class in _classes
                                        where Convert.ToString(@class[(int)ClassColumns.Label]) == value.ToString()
                                        select @class;

                            if (query.FirstOrDefault() != null)
                            {
                                roleQualification.value = new QMXFValue
                                {
                                    reference = query.FirstOrDefault()[(int)ClassColumns.ID].ToString().Trim(),
                                };
                            }
                        }
                        else
                        {
                            Utility.WriteString("\nType/Value Was Not Set for Role Qualification \"" + englishUSName.value + "\" on template \"" + templateName + "\".", "error.log", true);
                        }

                        roleQualifications.Add(roleQualification);
                    }
                }

                return roleQualifications;
            }
            catch (Exception ex)
            {
                Utility.WriteString("\nError Processing Role Qualification \n" +
                                    "\nRow: " + roleIndex + " \n" + ex.ToString() + "\n", "error.log", true);
                throw ex;
            }
        }

        private static List<ArrayList> MarshallToList(WorksheetPartWrapper part)
        {
            try
            {
                string vals = string.Empty;
                List<ArrayList> table = new List<ArrayList>();
                ArrayList rw;

                foreach (var row in part.Worksheet.SheetData.Rows.Where(r => r.RowIndex != 1))
                {
                    var value = row.GetCellValue<string>(0);
                    if (value == null || value.ToUpper() != "X") continue;
                    rw = new ArrayList();
                    for (int i = 0; i <= row.Cells.Count - 1; i++)
                    {
                        if (row.GetCellValue<string>(i) != null)
                        {
                            vals = row.GetCellValue<string>(i);
                        }
                        else
                        {
                            vals = string.Empty;
                        }
                        rw.Add(vals);
                    }

                    if (rw.Count > 0)
                        table.Add(rw);
                }
                return table;
            }
            catch (Exception ex)
            {
                Utility.WriteString("\n" + ex.ToString() + "\n", "error.log", true);
                throw ex;
            }
        }
    }
}