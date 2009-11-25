using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;
using IDS_ADI.InformationModel;
using org.iringtools.utility;
using org.iringtools.utility.Loggers;
using org.ids_adi.qmxf;

namespace QMXFGenerator
{
    class Program
    {
        #region MessageBox
        [DllImport("User32.dll")]
        public static extern int MessageBox(int h, string m, string c, int type);
        #endregion

        #region Logger
        private ILogger _logger;
        public ILogger Logger
        {
            get
            {   // Lazy instantiation
                if (_logger == null)
                    _logger = new DefaultLogger();
                return _logger;
            }
            set { _logger = value; }
        }

        #endregion

        #region Private Members
        private static string _excelFilePath = String.Empty;
        private static string _qmxfFilePath = String.Empty;
        private static string _proxyHost = String.Empty;
        private static string _proxyPort = String.Empty;
        private static string _proxyCredentials = String.Empty;
        private static string _idsADICredentials = String.Empty;
        private static string _classRegistryBase = String.Empty;
        private static string _templateRegistryBase = String.Empty;

        private static Worksheet _classWorksheet = null;
        private static Worksheet _baseTemplateWorksheet = null;

        private static SortedDictionary<string, string> _classes = new SortedDictionary<string, string>();
        private static SortedDictionary<string, string> _templates = new SortedDictionary<string, string>();
        private static SortedDictionary<string, string> _roles = new SortedDictionary<string, string>();
        #endregion

        static void Main(string[] args)
        {
            Workbook workbook = null;
            try
            {
                if (Initialize(args))
                {
                    workbook = ExcelLibrary.OpenWorkbook(_excelFilePath);

                    QMXF qmxf = new QMXF();

                    _classWorksheet = ExcelLibrary.GetWorksheet(workbook, (int)InformationModelWorksheets.Class);
                    Worksheet classSpecializationWorksheet = ExcelLibrary.GetWorksheet(workbook, (int)InformationModelWorksheets.ClassSpecialization);

                    qmxf.classDefinitions = ProcessClass(_classWorksheet, classSpecializationWorksheet);

                    _baseTemplateWorksheet = ExcelLibrary.GetWorksheet(workbook, (int)InformationModelWorksheets.BaseTemplate);

                    qmxf.templateDefinitions = ProcessBaseTemplate(_baseTemplateWorksheet);

                    Worksheet specializedIndividualTemplateWorksheet = ExcelLibrary.GetWorksheet(workbook, (int)InformationModelWorksheets.SpecializedIndividualTemplate);

                    qmxf.templateQualifications = ProcessSpecializedIndividualTemplates(specializedIndividualTemplateWorksheet);

                    specializedIndividualTemplateWorksheet = null;

                    _baseTemplateWorksheet = null;

                    classSpecializationWorksheet = null;

                    _classWorksheet = null;

                    Utility.Write<QMXF>(qmxf, _qmxfFilePath, false);

                    ExcelLibrary.CloseWorkbook(workbook);
                    workbook = null;

                    MessageBox(0, "Success!", "QMXFGenerator", 0);
                }
            }
            catch (Exception e)
            {
                Utility.WriteString("\n" + e.ToString() + "\n", "error.log", true);
                ExcelLibrary.CloseWorkbook(false, workbook);
                workbook = null;
                MessageBox(0, "Failed! See log file: error.log", "QMXFGenerator", 0);
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
                    //[<class ids file>] [<template ids file>] [<role ids file>]
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
            catch (Exception e)
            {
                Utility.WriteString("\nError Initializing \n" + e.ToString() + "\n", "error.log", true);
                throw e;
            }
        }

        private static List<ClassDefinition> ProcessClass(Worksheet classWorksheet, Worksheet classSpecializationWorksheet)
        {
            int rowIndex = 0;
            try
            {
                Range labelHeader = classWorksheet.get_Range("F1", Type.Missing);
                Range classRange = classWorksheet.get_Range("A2", "H" + labelHeader.CurrentRegion.Rows.Count);

                List<ArrayList> classList = MarshallToList(classRange);

                List<ClassDefinition> classDefinitions = new List<ClassDefinition>();

                foreach (ArrayList row in classList)
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
                                    lang = "EN-US",
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
                            classList[rowIndex][(int)ClassColumns.ID] = identifier;

                            //write to the sheet, but offset counters for 1-based array
                            classRange.set_Item(rowIndex + 1, (int)ClassColumns.ID + 1, identifier);
                        }

                        classDefinition.identifier = identifier.ToString().Trim();

                        if (description != null && description.ToString() != String.Empty)
                        {
                            Description englishUSDescription = new Description
                            {
                                lang = "EN-US",
                                value = description.ToString(),
                            };

                            classDefinition.description = new List<Description> 
            {
              englishUSDescription,
            };
                        }

                        if (entityType != null && entityType.ToString() != String.Empty)
                        {
                            classDefinition.entityType = new EntityType
                            {
                                reference = entityType.ToString()
                            };
                        }

                        List<Specialization> classSpecialization = ProcessClassSpecialization(
                          classList, classSpecializationWorksheet, name);

                        if (classSpecialization.Count > 0)
                            classDefinition.specialization = classSpecialization;

                        load = String.Empty;

                        classDefinitions.Add(classDefinition);
                    }

                    rowIndex++;
                }
                return classDefinitions;
            }
            catch (Exception e)
            {
                Utility.WriteString("\nError Processing Class \n Worksheet: " + classWorksheet.Name +
                                    "\t Row: " + rowIndex + " \n" + e.ToString() + "\n", "error.log");
                throw e;
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
            catch (Exception e)
            {
                Utility.WriteString("\nError Generating ID \n" + e.ToString() + "\n", "error.log");
                throw e;
            }
        }

        private static List<Specialization> ProcessClassSpecialization(
          List<ArrayList> classList, Worksheet classSpecializationWorksheet, string className)
        {
            try
            {
                Range selectionStart = classSpecializationWorksheet.get_Range("C1", Type.Missing); //Superclass Header
                Range classSpecializationRange = classSpecializationWorksheet.get_Range("A2", "D" + selectionStart.CurrentRegion.Rows.Count);

                List<Specialization> classSpecializations = new List<Specialization>();

                List<ArrayList> classSpecializationList = MarshallToList(classSpecializationRange);

                //Find the class specializations
                var specializationList =
                      from specialization in classSpecializationList
                      where Convert.ToString(specialization[(int)ClassSpecializationColumns.Subclass]) == className
                      select specialization;

                //Get their details from the Class List
                List<ArrayList> superclasses = new List<ArrayList>();

                foreach (ArrayList specialization in specializationList)
                {
                    object superclass = specialization[(int)ClassSpecializationColumns.Superclass];

                    var query =
                          from @class in classList
                          where Convert.ToString(@class[(int)ClassColumns.Label]) == superclass.ToString()
                          select @class;

                    if (query.FirstOrDefault().Count > 0)
                    {
                        superclasses.Add(query.FirstOrDefault());
                    }
                    else
                    {
                        Utility.WriteString("\n " + superclass.ToString() + " Was Not Found in Class List", "error.log", true);
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
            catch (Exception e)
            {
                Utility.WriteString("\nError Processing Class Specialization \n" +
                                    "Worksheet: " + classSpecializationWorksheet.Name + " \n" + e.ToString() + "\n", "error.log", true);
                MessageBox(0, "Failed! See log file: error.log", "QMXFGenerator", 0);
                throw e;
            }
        }

        private static List<TemplateDefinition> ProcessBaseTemplate(Worksheet worksheet)
        {
            int rowIndex = 0;
            try
            {
                Range labelHeader = worksheet.get_Range("I1", Type.Missing);
                Range templateRange = worksheet.get_Range("A2", "AX" + labelHeader.CurrentRegion.Rows.Count);

                List<ArrayList> templateList = MarshallToList(templateRange);

                List<TemplateDefinition> templateDefinitions = new List<TemplateDefinition>();

                foreach (ArrayList row in templateList)
                {
                    object load = row[(int)TemplateColumns.Load];

                    if (load != null && load.ToString().Trim() != String.Empty)
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
                                    lang = "EN-US",
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
                            templateList[rowIndex][(int)TemplateColumns.ID] = templateIdentifier;

                            //write to the sheet, but offset counters for 1-based array
                            templateRange.set_Item(rowIndex + 1, (int)TemplateColumns.ID + 1, templateIdentifier);
                        }

                        templateDefinition.identifier = templateIdentifier.ToString().Trim();

                        if (description != null && description.ToString() != String.Empty)
                        {
                            Description englishUSDescription = new Description
                            {
                                lang = "EN-US",
                                value = description.ToString(),
                            };

                            templateDefinition.description = new List<Description> 
            {
              englishUSDescription,
            };
                        }

                        templateDefinition.roleDefinition = ProcessRoleDefinition(
                          row, rowIndex, templateList, templateRange);

                        load = String.Empty;

                        templateDefinitions.Add(templateDefinition);
                    }

                    rowIndex++;
                }

                return templateDefinitions;
            }
            catch (Exception e)
            {
                Utility.WriteString("\nError Processing Template \n Worksheet: " + worksheet.Name + "\tRow: "
                                     + rowIndex + " \n" + e.ToString() + "\n", "error.log", true);
                throw e;
            }
        }

        private static List<RoleDefinition> ProcessRoleDefinition(
          ArrayList row, int rowIndex, List<ArrayList> templateList, Range templateRange)
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
                            lang = "EN-US",
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
                            templateList[rowIndex][(int)RoleColumns.ID + roleOffset] = identifier;

                            //write to the sheet, but offset counters for 1-based array
                            templateRange.set_Item(rowIndex + 1, (int)RoleColumns.ID + roleOffset + 1, identifier);
                        }

                        roleDefinition.identifier = identifier.ToString();

                        if (description != null && description.ToString() != String.Empty)
                        {
                            Description englishUSDescription = new Description
                            {
                                lang = "EN-US",
                                value = description.ToString(),
                            };

                            roleDefinition.description = englishUSDescription;
                        }

                        if (type != null && type.ToString() != String.Empty)
                        {
                            Range labelHeader = _classWorksheet.get_Range("F1", Type.Missing);
                            Range classRange = _classWorksheet.get_Range("A2", "H" + labelHeader.CurrentRegion.Rows.Count);

                            List<ArrayList> classList = MarshallToList(classRange);

                            var query =
                              from @class in classList
                              where Convert.ToString(@class[(int)ClassColumns.Label]) == type.ToString()
                              select @class;

                            if (query.FirstOrDefault() != null && query.FirstOrDefault()[(int)ClassColumns.ID].ToString().Trim().Equals(type.ToString()))
                            {
                                roleDefinition.range = query.FirstOrDefault()[(int)ClassColumns.ID].ToString().Trim();
                            }
                            else
                            {
                                Utility.WriteString("\n " + type.ToString() + " Was Not Found in Class List While Processing Role Definition", "error.log", true);
                            }
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

        private static List<TemplateQualification> ProcessSpecializedIndividualTemplates(Worksheet worksheet)
        {
            int rowIndex = 0;
            try
            {
                Range labelHeader = worksheet.get_Range("I1", Type.Missing);
                Range templateRange = worksheet.get_Range("A2", "AX" + labelHeader.CurrentRegion.Rows.Count);

                List<ArrayList> templateList = MarshallToList(templateRange);

                List<TemplateQualification> templateQualifications = new List<TemplateQualification>();

                foreach (ArrayList row in templateList)
                {
                    object load = row[(int)TemplateColumns.Load];

                    if (load != null && load.ToString().Trim() != String.Empty)
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
                                    lang = "EN-US",
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
                            templateList[rowIndex][(int)TemplateColumns.ID] = templateIdentifier;

                            //write to the sheet, but offset counters for 1-based array
                            templateRange.set_Item(rowIndex + 1, (int)TemplateColumns.ID + 1, templateIdentifier);
                        }

                        templateQualification.identifier = templateIdentifier.ToString().Trim();

                        if (description != null && description.ToString() != String.Empty)
                        {
                            Description englishUSDescription = new Description
                            {
                                lang = "EN-US",
                                value = description.ToString(),
                            };

                            templateQualification.description = new List<Description> 
            {
              englishUSDescription,
            };
                        }

                        if (parentTemplate != null && parentTemplate.ToString() != String.Empty)
                        {
                            labelHeader = _baseTemplateWorksheet.get_Range("I1", Type.Missing);
                            Range basetemplateRange = _baseTemplateWorksheet.get_Range("A2", "AX" + labelHeader.CurrentRegion.Rows.Count);

                            List<ArrayList> baseTemplateList = MarshallToList(basetemplateRange);

                            var query =
                              from template in baseTemplateList
                              where Convert.ToString(template[(int)TemplateColumns.Name]) == parentTemplate.ToString()
                              select template;

                            ArrayList parentRow = query.FirstOrDefault();

                            if (parentRow != null)
                            //if (parentRow[(int)TemplateColumns.ID].ToString().Trim().Equals(parentTemplate.ToString()))
                            {
                                templateQualification.qualifies = parentRow[(int)TemplateColumns.ID + 1].ToString().Trim();

                                templateQualification.roleQualification = ProcessRoleQualification(row, parentRow);
                            }
                            else
                            {
                                Utility.WriteString("\n " + parentTemplate.ToString() + " Was Not Found in Template List While Processing Specialized Templates", "error.log", true);
                            }
                        }

                        load = String.Empty;

                        templateQualifications.Add(templateQualification);
                    }

                    rowIndex++;
                }

                return templateQualifications;
            }
            catch (Exception e)
            {
                Utility.WriteString("\nError Processing Individual Template \n" +
                                    "Worksheet: " + worksheet.Name + "\tRow: " + rowIndex +
                                    " \n" + e.ToString() + "\n", "error.log", true);
                throw e;
            }
        }

        private static List<RoleQualification> ProcessRoleQualification(ArrayList row, ArrayList parentRow)
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

                    object parentRole = parentRow[(int)RoleColumns.ID + 1 + roleOffset];

                    if (label != null && label.ToString().Trim() != String.Empty)
                    {
                        string name = label.ToString();

                        RoleQualification roleQualification = new RoleQualification();

                        QMXFName englishUSName = new QMXFName
                        {
                            lang = "EN-US",
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
                                lang = "EN-US",
                                value = description.ToString(),
                            };

                            roleQualification.description = new List<Description>
                            {
                                englishUSDescription
                            };
                        }

                        roleQualification.qualifies = parentRole.ToString().Trim();

                        Range labelHeader = _classWorksheet.get_Range("F1", Type.Missing);
                        Range classRange = _classWorksheet.get_Range("A2", "H" + labelHeader.CurrentRegion.Rows.Count);

                        List<ArrayList> classList = MarshallToList(classRange);

                        if (type != null && type.ToString() != String.Empty)
                        {
                            var query =
                              from @class in classList
                              where Convert.ToString(@class[(int)ClassColumns.Label]) == type.ToString()
                              select @class;

                            if (query.FirstOrDefault() != null && query.FirstOrDefault()[(int)ClassColumns.ID + 1].ToString().Trim().Equals(type.ToString()))
                            {
                                roleQualification.range = query.FirstOrDefault()[(int)ClassColumns.ID + 1].ToString().Trim();
                            }
                            else
                            {
                                Utility.WriteString("\n " + type.ToString() + " Was Not Found in Class List While Processing Role Qualification", "error.log", true);
                            }
                        }
                        else if (value != null && value.ToString() != String.Empty)
                        {

                            var query =
                              from @class in classList
                              where Convert.ToString(@class[(int)ClassColumns.Label]) == value.ToString()
                              select @class;

                            if (query.FirstOrDefault() != null)
                            {
                                roleQualification.value = new QMXFValue
                                {
                                    reference = query.FirstOrDefault()[(int)ClassColumns.ID + 1].ToString().Trim(),
                                };
                            }

                        }

                        roleQualifications.Add(roleQualification);
                    }

                }

                return roleQualifications;
            }
            catch (Exception e)
            {
                Utility.WriteString("\nError Processing Role Qualification \n" +
                                    "\nRow: " + roleIndex + " \n" + e.ToString() + "\n", "error.log", true);
                throw e;
            }
        }

        private static List<ArrayList> MarshallToList(Range range)
        {
            try
            {
                int index = 1;
                List<ArrayList> table = new List<ArrayList>();
                ArrayList row = new ArrayList();
                foreach (object value in (object[,])range.Value2)
                {
                    if (value != null)
                    {
                        row.Add(value);
                    }
                    else
                    {
                        row.Add(null);
                    }

                    if (index == range.Columns.Count)
                    {
                        table.Add(row);
                        row = new ArrayList();

                        index = 1;
                    }
                    else
                    {
                        index++;
                    }
                }

                return table;
            }
            catch (Exception e)
            {
                Utility.WriteString("\n" + e.ToString() + "\n", "error.log", true);
                throw e;
            }
        }
    }
}