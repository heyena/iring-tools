using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExtremeML.Packaging;
using ExtremeML.Spreadsheet.Address;
using IDS_ADI.InformationModel;
using org.ids_adi.qmxf;
using org.iringtools.library;
using org.iringtools.utility;

namespace QMXFGenerator
{
  internal class Program
  {
    #region Private Members

    private static string _excelFilePath = String.Empty;
    private static string _qmxfFilePath = String.Empty;
    private static string _processedFilePath = String.Empty;
    private static string _proxyHost = String.Empty;
    private static string _proxyPort = String.Empty;
    private static string _proxyCredentials = String.Empty;
    private static string _classRegistryBase = String.Empty;
    private static string _templateRegistryBase = String.Empty;
    private static string _targetRepositoryForClass = string.Empty;
    private static string _targetRepositoryForTemplate = string.Empty;
    private static string _updateRun = string.Empty;
    private static string _clearAllBeforeUpdate = string.Empty;
    private static string _autoCloseOnComplete = string.Empty;
    
    
    private static SpreadsheetDocumentWrapper _document = null;
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

    #endregion Private Members

    private static void Main(string[] args)
    {
      try
      {
        {
          var qmxf = new QMXF();
          if (Initialize(args))
          {
            using (_document = SpreadsheetDocumentWrapper.Open(_excelFilePath))
            {
              _refdataClient = new WebHttpClient(_refdataServiceUri);
              _classWorksheet = GetWorksheet(_document, "Class");
              _classSpecializationWorksheet = GetWorksheet(_document, "Class Specialization");
              Console.WriteLine("Processing Classes...");
              qmxf.classDefinitions = ProcessClass(_classWorksheet, _classSpecializationWorksheet);

              _classificationWorksheet = GetWorksheet(_document, "Classification");

              Console.WriteLine("Processing Classifications...");
              ProcessClassDefinitions(_classificationWorksheet, qmxf.classDefinitions);

              _baseTemplateWorksheet = GetWorksheet(_document, "Base Template");

              Console.WriteLine("Processing Base Templates...");
              qmxf.templateDefinitions = ProcessBaseTemplate(_baseTemplateWorksheet);

              var specializedIndividualTemplateWorksheet = GetWorksheet(_document, "Specialized Individual Template");

              Console.WriteLine("Processing Specialized Individual Templates...");
              qmxf.templateQualifications = ProcessSpecializedIndividualTemplates(specializedIndividualTemplateWorksheet);

              specializedIndividualTemplateWorksheet = null;
              _baseTemplateWorksheet = null;
              _classSpecializationWorksheet = null;
              _classWorksheet = null;
            }
            ///Post Classes and Templates individually to refdataService
            //var error = false;
            var errorMessage = String.Empty;
            if (!string.IsNullOrEmpty(_updateRun))
            {

              if (!string.IsNullOrEmpty(_clearAllBeforeUpdate))
              {
                ClearRepository(_targetRepositoryForClass);
                ClearRepository(_targetRepositoryForTemplate);
              }
            
              foreach (var cls in qmxf.classDefinitions)
              {
                try
                {
                  if (!CheckUri(cls.identifier))
                  {
                    Utility.WriteString("Cannot Post Example namespace " + cls.identifier + "\r\n", "error.log", true);
                    continue;
                  }
                  var q = new QMXF {targetRepository = _targetRepositoryForClass};
                  q.classDefinitions.Add(cls);
                  var resp = _refdataClient.Post<QMXF, Response>("/classes", q, true);
                  if (resp.Level == StatusLevel.Error)
                  {
                    Console.WriteLine("Error posting class: " + cls.name[0].value);
                    Utility.WriteString("Error posting class: " + cls.name[0].value + "\r\n" + resp.ToString() + "\r\n", "error.log", true);
                  }
                  else
                    Console.WriteLine("Success: posted class: " + cls.name[0].value);
                }
                catch (Exception ex)
                {
                  Utility.WriteString("Error posting class: " + cls.name[0].value + "\r\n" + ex.Message + "\r\n", "error.log", true);
                }
              }
              ///Post baseTemplates
              ///
              
              foreach (var t in qmxf.templateDefinitions)
              {
                try
                {
                  errorMessage = String.Empty;
                  if (!CheckUri(t.identifier))
                  {
                    //error = true;
                    //Utility.WriteString("Cannot Post Example namespace " + t.identifier + "\r\n", "error.log", true);
                    errorMessage = "Cannot Post Example namespace " + t.identifier + "\r\n";
                  }

                  foreach (var r in t.roleDefinition)
                  {
                    if (string.IsNullOrEmpty(r.range))
                    {
                      //Utility.WriteString("\r\n" + r.identifier + " do not have range defined \r\n", "error.log", true);
                      //Console.WriteLine("error in template " + t.identifier + " see : error.log");
                      errorMessage = errorMessage + "\r\n" + r.identifier + " do not have range defined \r\n";
                    }
                    if (CheckUri(r.identifier))
                      errorMessage = errorMessage + "Cannot Post Example namespace " + r.identifier + "\r\n";

                  }
                  if (!String.IsNullOrWhiteSpace(errorMessage))
                  {
                    throw new ApplicationException(errorMessage);
                  }

                  var q = new QMXF {targetRepository = _targetRepositoryForTemplate};
                  q.templateDefinitions.Add(t);
                  var resp = _refdataClient.Post<QMXF, Response>("/templates", q, true);
                  if (resp.Level == StatusLevel.Error)
                  {
                    Console.WriteLine("Error posting baseTemplate: " + t.name[0].value);
                    Utility.WriteString("Error posting baseTemplate: " + t.name[0].value + "\r\n" + resp.ToString() + "\r\n", "error.log", true);
                  }
                  else
                    Console.WriteLine("Success: posted baseTemplate: " + t.name[0].value);
                }
                catch (Exception ex)
                {
                  Utility.WriteString("Error posting baseTemplate: " + t.name[0].value + "\r\n" + ex.Message + "\r\n", "error.log", true);
                }
              }
              ///Post Specialised templates
              foreach (var t in qmxf.templateQualifications)
              {
                try
                {
                  errorMessage = String.Empty;
                  if (!CheckUri(t.identifier))
                  {
                    //Utility.WriteString("Cannot Post Example namespace " + t.identifier + "\r\n", "error.log", true);
                    errorMessage = "Cannot Post Example namespace " + t.identifier + "\r\n";
                    //error = true;
                  }
                  foreach (
                    var r in
                      t.roleQualification.Where(
                        r =>
                        string.IsNullOrEmpty(r.range) && (r.value == null || string.IsNullOrEmpty(r.value.reference))))
                  {
                    //Utility.WriteString("\r\n" + r.identifier + " do not have range defined \r\n", "error.log", true);
                    errorMessage = errorMessage + "\r\n" + r.identifier + " do not have range defined \r\n";
                    //Console.WriteLine("error in template " + t.identifier + " see : error.log");
                    //error = true;
                  }
                  if (!String.IsNullOrWhiteSpace(errorMessage))
                  {
                    throw new ApplicationException(errorMessage);
                  }
                  var q = new QMXF { targetRepository = _targetRepositoryForTemplate };
                  q.templateQualifications.Add(t);
                  var resp = _refdataClient.Post<QMXF, Response>("/templates", q, true);
                  if (resp.Level == StatusLevel.Error)
                  {
                    Console.WriteLine("Error posting specializedTemplate: " + t.name[0].value);
                    Utility.WriteString("Error posting specializedTemplate: " + t.name[0].value + "\r\n" + resp.ToString() + "\r\n", "error.log",
                                        true);
                  }
                  else
                    Console.WriteLine("Success: posted specializedTemplate: " + t.name[0].value);
                }
                catch (Exception ex)
                {
                  Utility.WriteString("Error posting specializedTemplate: " + t.name[0].value + "\r\n" + ex.Message + "\r\n", "error.log", true);
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Utility.WriteString("\r\n" + ex.ToString() + "\r\n", "error.log", true);
        Console.WriteLine("Failure: See log file: error.log");
      }

      if (string.IsNullOrEmpty(_autoCloseOnComplete))
      {
        Console.ReadKey();
      }
    }

    private static bool CheckUri(string uri)
    {
      if (uri.Contains("example"))
        return false;
      else
        return true;
    }

    private static WorksheetPartWrapper GetWorksheet(SpreadsheetDocumentWrapper document, string sheetName)
    {
      return document.WorkbookPart.WorksheetParts[sheetName];
    }

    private string GetCellValue(WorksheetPartWrapper part, int startCol, int startRow)
    {
      var row = part.Worksheet.SheetData.Rows.FirstOrDefault(c => c.RowIndex == startRow);
      return row.GetCell(startCol, false).CellValue.Value;
    }

    private static void ProcessClassDefinitions(WorksheetPartWrapper _classificationWorksheet,
                                                List<ClassDefinition> list)
    {
      try
      {
        _classifications = MarshallToList(_classificationWorksheet);
        foreach (var c in _classifications)
        {
          var query = from cls in _classes
                      where
                        cls[(int) ClassColumns.Label].ToString().Trim().Equals(
                          c[(int) ClassificationColumns.Classified].ToString())
                      select cls[(int) ClassColumns.ID];
          var cl = list.SingleOrDefault(l => l.name[0].value.Equals(c[(int) ClassificationColumns.Class].ToString()));
          if (cl != null && query != null && query.Count() > 0)
          {
            cl.classification.Add(new Classification
              {
                label = c[(int) ClassificationColumns.Classified].ToString(),
                lang = "en",
                reference = query.FirstOrDefault().ToString()
              });
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
          _targetRepositoryForClass = System.Configuration.ConfigurationManager.AppSettings["TargetRepositoryNameForClass"];
          _targetRepositoryForTemplate = System.Configuration.ConfigurationManager.AppSettings["TargetRepositoryNameForTemplate"];
          _refdataServiceUri = System.Configuration.ConfigurationManager.AppSettings["RefdataServiceUri"];
          _updateRun = System.Configuration.ConfigurationManager.AppSettings["UpdateRun"];
          _clearAllBeforeUpdate = System.Configuration.ConfigurationManager.AppSettings["ClearAllBeforeUpdate"];
          _autoCloseOnComplete = System.Configuration.ConfigurationManager.AppSettings["AutoCloseOnComplete"];        
        }
        else
        {
          _excelFilePath = args[0];
        }

        if (_excelFilePath == String.Empty)
        {
          Console.WriteLine("Usage: \r\n");
          Console.WriteLine("   qmxfgen.exe <excel file> <output file>");
          return false;
        }
        _proxyHost = System.Configuration.ConfigurationManager.AppSettings["ProxyHost"];
        _proxyPort = System.Configuration.ConfigurationManager.AppSettings["ProxyPort"];
        _proxyCredentials = System.Configuration.ConfigurationManager.AppSettings["ProxyCredentialToken"];

        var useTestRegistry = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["UseTestRegistry"]);

        if (useTestRegistry)
        {
          var testRegistryBase = System.Configuration.ConfigurationManager.AppSettings["TestRegistryBase"];

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
        Utility.WriteString("\nError Initializing \r\n" + ex.ToString() + "\r\n", "error.log", true);
        throw ex;
      }
    }

    private static List<ClassDefinition> ProcessClass(WorksheetPartWrapper classPart,
                                                      WorksheetPartWrapper specializationPart)
    {
      var rowIndex = 0;
      var idx = 0;
      try
      {
        _classes = MarshallToList(classPart);
        _classSpecializations = MarshallToList(specializationPart);

        var classDefinitions = new List<ClassDefinition>();
        foreach (var row in _classes)
        {
          var load = row[(int) ClassColumns.Load];
          rowIndex = Convert.ToInt32(row[row.Count - 1]);
          if (load.ToString() == "Load") continue;
          var identifier = row[(int) ClassColumns.ID];
          var label = row[(int) ClassColumns.Label];
          var description = row[(int) ClassColumns.Description];
          var entityType = row[(int) ClassColumns.EntityType];

          if (load == null || load.ToString().Trim() == String.Empty)
          {
            bool isIdentifierExist = !(identifier == null || identifier.ToString() == String.Empty);
            bool isLabelExist = !(label == null || label.ToString() == String.Empty);

            if (isIdentifierExist && !isLabelExist) 
            {
              Utility.WriteString(string.Format("\r\nIdentifier {0} does not have a label!!", identifier.ToString()), "error.log",true);
            }
            else if(!isIdentifierExist && isLabelExist)
            {
              Utility.WriteString(string.Format("\r\nLabel {0} does not have an identifier!!", label.ToString()), "error.log",true);
            }
            continue;
          }

          var classDefinition = new ClassDefinition();

          var name = String.Empty;
          if (label != null)
          {
            name = label.ToString();

            if (name != String.Empty)
            {
              var englishUsName = new QMXFName
                {
                  lang = "en",
                  value = name,
                };

              classDefinition.name = new List<QMXFName>
                {
                  englishUsName
                };
            }
          }

          if (identifier == null || identifier.ToString() == String.Empty)
          {
            identifier = GenerateId(_classRegistryBase, name);
            //write to the in-memory list
            _classes[idx][(int) ClassColumns.ID] = identifier;
            //write to the sheet, but offset counters for 1-based array
            classPart.Worksheet.SetCellValue(new GridReference(rowIndex - 1, (int) ClassColumns.ID), identifier);
          }

          classDefinition.identifier = identifier.ToString();
          if (description != null && description.ToString() != String.Empty)
          {
            var englishUsDescription = new Description
              {
                lang = "en",
                value = description.ToString(),
              };
            classDefinition.description = new List<Description>
              {
                englishUsDescription,
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
         
          var sList = from specialization in _classSpecializations
                                   where
                                     Convert.ToString(specialization[(int)ClassSpecializationColumns.Superclass]) ==
                                     name
                                   select specialization;
          if (sList.Count() > 0)
          {
            var classSpecialization = ProcessClassSpecialization(name, sList);

          if (classSpecialization.Count > 0)
            classDefinition.specialization = classSpecialization;
          }
          load = String.Empty;
          idx++;
          if (string.IsNullOrEmpty(ent))/// must have entity type
          {
            Utility.WriteString(string.Format("\r\nClass {0} does not have a entity type!!", name), "error.log",true);
          }
          else
          {
            classDefinitions.Add(classDefinition);
          }
        }
        Console.WriteLine("  processed " + idx + " classes.");
        return classDefinitions;
      }
      catch (Exception ex)
      {
        Utility.WriteString("\r\nError Processing Class \r\n Worksheet: " + classPart +
                            "\t Row: " + idx + " \r\n" + ex.ToString() + "\r\n", "error.log",true);
        throw ex;
      }
    }

    private static string GenerateId(string registryBase, string name)
    {
      try
      {
        if (!string.IsNullOrEmpty(registryBase))
          return string.Format("{0}R{1}", registryBase,
                               Guid.NewGuid().ToString().Replace("_", "").Replace("-", "").ToUpper());
        else
        {
          Utility.WriteString("Failed to create id for " + name, "error.log",true);
          throw new Exception("GenerateID: Failed to create id ");
        }
      }
      catch (Exception ex)
      {
        Utility.WriteString("Error Generating ID\r\n" + ex.ToString() + "\r\n", "error.log",true);
        throw ex;
      }
    }

    private static List<Specialization> ProcessClassSpecialization(string className, IEnumerable<ArrayList> specializationList)
    {
      try
      {
        List<Specialization> classSpecializations = new List<Specialization>();

        //Find the class specializations
        //var specializationList = from specialization in _classSpecializations
        //                         where
        //                           Convert.ToString(specialization[(int)ClassSpecializationColumns.Superclass]) ==
        //                           className
        //                         select specialization;
        //Get their details from the Class List
        var superclasses = new List<ArrayList>();

        foreach (var specialization in specializationList)
        {
          object subclass = specialization[(int) ClassSpecializationColumns.Subclass];
          object supclass = specialization[(int)ClassSpecializationColumns.Superclass];
          var supc = from @class in _classes
                   where Convert
                           .ToString(@class[(int)ClassColumns.Label])
                           .Trim() == supclass.ToString().Trim()
                   select @class;
          var sc = from @class in _classes
                      where Convert
                              .ToString(@class[(int) ClassColumns.Label])
                              .Trim() == subclass.ToString().Trim()
                      select @class;
          if (supc.Count() > 0 && supc.FirstOrDefault().Count > 0)
          {
            if (sc.Count() > 0 && sc.FirstOrDefault().Count > 0)
          {
              superclasses.Add(sc.FirstOrDefault());
          }
          else
          {
            Utility.WriteString("\r\n" + subclass.ToString() + " Was Not Found in Class List", "error.log", true);
            }
          }
          else
          {
            Utility.WriteString("\r\n" + supclass.ToString() + " Was Not Found in Class List", "error.log", true);
          }
        }

        //Use the details for each to create the Specializations and add to List to return
        foreach (ArrayList superClassRow in superclasses)
        {
          object superclassIdentifier = superClassRow[(int) ClassColumns.ID];
          object superclassName = superClassRow[(int) ClassColumns.Label];

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
        Utility.WriteString("\nError Processing Class Specialization \r\n" +
                            "Worksheet: " + _classSpecializationWorksheet + " \r\n" + ex.ToString() + "\r\n", "error.log",
                            true);
        throw ex;
      }
    }

    private static List<TemplateDefinition> ProcessBaseTemplate(WorksheetPartWrapper part)
    {
      var rowIndex = 0;
      var idx = 0;
      try
      {
        _baseTemplates = MarshallToList(part);
        var templateDefinitions = new List<TemplateDefinition>();
        foreach (var row in _baseTemplates)
        {
          rowIndex = Convert.ToInt32(row[row.Count - 1]);
          var load = row[(int) TemplateColumns.Load];
          if (load == null || load.ToString().Trim() == String.Empty || load.ToString() == "Load") continue;
          var templateIdentifier = row[(int) TemplateColumns.ID];
          var templateName = row[(int) TemplateColumns.Name];
          var description = row[(int) TemplateColumns.Description];
          var templateDefinition = new TemplateDefinition();
          var name = String.Empty;
          if (templateName != null)
          {
            name = templateName.ToString();
            if (name != String.Empty)
            {
              var englishUsName = new QMXFName
                {
                  lang = "en",
                  value = name,
                };
              templateDefinition.name = new List<QMXFName>
                {
                  englishUsName
                };
            }
          }
          if (templateIdentifier == null || templateIdentifier.ToString() == String.Empty)
          {
            templateIdentifier = GenerateId(_templateRegistryBase, name);
            //write to the in-memory list
            foreach (var b in _baseTemplates.Where(b => Convert.ToInt32(b[b.Count - 1]) == rowIndex))
            {
              b[(int) TemplateColumns.ID] = templateIdentifier;
            }
            //write to the sheet, but offset counters for 1-based array
            part.Worksheet.SetCellValue(new GridReference(rowIndex - 1, (int) TemplateColumns.ID), templateIdentifier);
          }
          templateDefinition.identifier = templateIdentifier.ToString().Trim();
          if (description != null && description.ToString() != String.Empty)
          {
            var englishUsDescription = new Description
              {
                lang = "en",
                value = description.ToString(),
              };
            templateDefinition.description = new List<Description>
              {
                englishUsDescription,
              };
          }
          templateDefinition.roleDefinition = ProcessRoleDefinition(templateDefinition.name.FirstOrDefault().value,
                                                                    row, Convert.ToInt32(row[row.Count - 1]), part);
          load = String.Empty;
          templateDefinitions.Add(templateDefinition);
          idx++;
        }
        Console.WriteLine("  processed " + idx + " base templates.");
        return templateDefinitions;
      }
      catch (Exception e)
      {
        Utility.WriteString("\nError Processing Template \r\n Worksheet: " + part.Name + "\tRow: "
                            + idx + " \r\n" + e.ToString() + "\r\n", "error.log", true);
        throw e;
      }
    }

    private static List<RoleDefinition> ProcessRoleDefinition(string templateName, IList row, int rowIndex,
                                                              WorksheetPartWrapper part)
    {
      try
      {
        var idx = 0;
        var roleDefinitions = new List<RoleDefinition>();
        for (var roleIndex = 0; roleIndex <= (int) RoleColumns.Max - 1; roleIndex++)
        {
          var roleOffset = (int) TemplateColumns.Roles + ((int) RoleColumns.Count*roleIndex);
          var identifier = row[(int) RoleColumns.ID + roleOffset];
          var label = row[(int) RoleColumns.Name + roleOffset];
          var description = row[(int) RoleColumns.Description + roleOffset];
          var type = row[(int) RoleColumns.Type + roleOffset];

          if (label == null || label.ToString().Trim() == String.Empty) continue;
          var name = label.ToString();
          var roleDefinition = new RoleDefinition();

          var englishUsName = new QMXFName
            {
              lang = "en",
              value = name,
            };

          roleDefinition.name = new List<QMXFName>
            {
              englishUsName
            };

          if (string.IsNullOrEmpty(identifier.ToString()))
          {
            identifier = GenerateId(_templateRegistryBase, name);

            //write to the in-memory list
            _baseTemplates[idx][(int) RoleColumns.ID + roleOffset] = identifier;

            //write to the sheet, but offset counters for 1-based array
            part.Worksheet.SetCellValue(new GridReference(rowIndex - 1, (int) RoleColumns.ID + roleOffset), identifier);
          }
          roleDefinition.identifier = identifier.ToString();

          if (description != null && description.ToString() != String.Empty)
          {
            var englishUsDescription = new Description
              {
                lang = "en",
                value = description.ToString(),
              };
            roleDefinition.description = englishUsDescription;
          }
          object clist;
          if (!string.IsNullOrEmpty(type.ToString()))
          {
            var query = from clss in _classes
                        where
                          Convert.ToString(clss[(int) ClassColumns.Label].ToString().ToUpper()) ==
                          type.ToString().ToUpper()
                        select clss;
            if (query.FirstOrDefault() != null &&
                query.FirstOrDefault()[(int) ClassColumns.Label].ToString().Trim().Equals(type.ToString()))
            {
              roleDefinition.range = query.FirstOrDefault()[(int) ClassColumns.ID].ToString().Trim();
            }
            else
            {
              Utility.WriteString(
                "\r\n" + type.ToString() + " Was Not Found in Class List While Processing Role Definition", "error.log",
                true);
            }
          }
          else
          {
            Utility.WriteString(
              "\nType Was Not Set for Role Definition \"" + englishUsName.value + "\" on template \"" + templateName +
              "\".", "error.log", true);
          }
          roleDefinitions.Add(roleDefinition);
        }
        return roleDefinitions;
      }
      catch (Exception e)
      {
        Utility.WriteString("\nError Processing Role \r\n Row: " + rowIndex + " \r\n" + e.ToString() + "\r\n", "error.log",
                            true);
        throw e;
      }
    }

    private static List<TemplateQualification> ProcessSpecializedIndividualTemplates(WorksheetPartWrapper part)
    {
      var rowIndex = 0;
      var idx = 0;
      try
      {
        _siTemplates = MarshallToList(part);
        var templateQualifications = new List<TemplateQualification>();
        foreach (var row in _siTemplates)
        {
          rowIndex = Convert.ToInt32(row[row.Count - 1]);
          var load = row[(int) TemplateColumns.Load];

          if (load == null || load.ToString().Trim() == String.Empty || load.ToString() == "Load") continue;
          var templateIdentifier = row[(int) TemplateColumns.ID];
          var templateName = row[(int) TemplateColumns.Name];
          var description = row[(int) TemplateColumns.Description];
          var parentTemplate = row[(int) TemplateColumns.ParentTemplate];
          var templateQualification = new TemplateQualification();
          var name = String.Empty;
          if (templateName != null)
          {
            name = templateName.ToString();
            if (name != String.Empty)
            {
              var englishUsName = new QMXFName
                {
                  lang = "en",
                  value = name,
                };
              templateQualification.name = new List<QMXFName>
                {
                  englishUsName
                };
            }
          }
          if (string.IsNullOrEmpty(templateIdentifier.ToString()))
          {
            templateIdentifier = GenerateId(_templateRegistryBase, name);
            _siTemplates[rowIndex - 1][(int) TemplateColumns.ID] = templateIdentifier;
            part.Worksheet.SetCellValue(new GridReference(rowIndex - 1, (int) TemplateColumns.ID), templateIdentifier);
          }
          templateQualification.identifier = templateIdentifier.ToString().Trim();
          if (!string.IsNullOrEmpty(description.ToString()))
          {
            var englishUsDescription = new Description
              {
                lang = "en",
                value = description.ToString(),
              };
            templateQualification.description = new List<Description>
              {
                englishUsDescription,
              };
          }
          if (!string.IsNullOrEmpty(parentTemplate.ToString()))
          {
            var query = from template in _baseTemplates
                        where Convert.ToString(template[(int) TemplateColumns.Name]) == parentTemplate.ToString()
                        select template;

            var parentRow = query.FirstOrDefault();
            if (parentRow != null)
            {
              var templateQualifiesId = parentRow[(int) TemplateColumns.ID];
              if (templateQualifiesId == null)
              {
                Utility.WriteString(
                  "Template Qualification \"" + templateQualification.identifier + "\" qualifies ID not found.\r\n",
                  "error.log", true);
              }
              templateQualification.qualifies = (templateQualifiesId ?? "").ToString().Trim();
              templateQualification.roleQualification =
                ProcessRoleQualification(templateQualification.name.FirstOrDefault().value, row, parentRow, rowIndex, part);
            }
            else
            {
              Utility.WriteString(
                parentTemplate.ToString() + " Was Not Found in Template List While Processing Specialized Templates.\r\n",
                "error.log", true);
            }
          }
          load = String.Empty;
          idx++;
          if (templateQualification.roleQualification.Count > 0)
          {
            templateQualifications.Add(templateQualification);
          }
          else
            Utility.WriteString(
              "Template Qualification \"" + templateQualification.identifier + "\" RoleQualifications failed.\r\n",
              "error.log", true);
        }
        Console.WriteLine("  processed " + idx + " Specialized templates.");
        return templateQualifications;
      }
      catch (Exception ex)
      {
        Utility.WriteString("\nError Processing Individual Template \r\n" +
                            "Worksheet: " + part.Name + "\tRow: " + idx +
                            " \r\n" + ex.ToString() + "\r\n", "error.log", true);
        throw ex;
      }
    }

    private static List<RoleQualification> ProcessRoleQualification(string templateName, IList row, IList parentRow,
                                                                    int rowIndex, WorksheetPartWrapper part)
    {
      var roleIndex = 0;
      var idx = 0;
      try
      {
        var roleQualifications = new List<RoleQualification>();

        for (roleIndex = 0; roleIndex <= (int) RoleColumns.Max - 1; roleIndex++)
        {
          var roleOffset = (int) TemplateColumns.Roles + ((int) RoleColumns.Count*roleIndex);
          var identifier = row[(int) RoleColumns.ID + roleOffset];
          var label = row[(int) RoleColumns.Name + roleOffset];
          var description = parentRow[(int) RoleColumns.Description + roleOffset];
          var type = row[(int) RoleColumns.Type + roleOffset];
          var value = row[(int) RoleColumns.Value + roleOffset];
          var parentRole = parentRow[(int) RoleColumns.ID + roleOffset];

          if (label == null || label.ToString().Trim() == String.Empty) continue;
          var name = label.ToString();

          if (parentRole == null)
          {
            Utility.WriteString(
              "Error Processing Role Qualification: Role \"" + name + "\" at index " + roleIndex + " on template \"" +
              templateName + "\" not found.\r\n", "error.log", true);
            continue;
          }
          if (string.IsNullOrEmpty(identifier.ToString())) // == null || identifier.ToString() == String.Empty)
          {
            identifier = GenerateId(_templateRegistryBase, name);

            //write to the in-memory list
            _siTemplates[idx][(int) RoleColumns.ID + roleOffset] = identifier;

            //write to the sheet, but offset counters for 1-based array
            part.Worksheet.SetCellValue(new GridReference(rowIndex - 1, (int) RoleColumns.ID + roleOffset), identifier);
          }
          var roleQualification = new RoleQualification {identifier = identifier.ToString()};

          var englishUsName = new QMXFName
            {
              lang = "en",
              value = name,
            };

          roleQualification.name = new List<QMXFName>
            {
              englishUsName
            };

          if (description != null && description.ToString() != String.Empty)
          {
            var englishUsDescription = new Description
              {
                lang = "en",
                value = description.ToString(),
              };

            roleQualification.description = new List<Description>
              {
                englishUsDescription
              };
          }

          roleQualification.qualifies = (parentRole ?? "").ToString().Trim();

          if (!string.IsNullOrEmpty(type.ToString()))
          {
            var query = from @class in _classes
                        where Convert.ToString(@class[(int) ClassColumns.Label]).Trim() == type.ToString().Trim()
                        select @class;

            if (query.FirstOrDefault() != null)
            {
              var classId = query.FirstOrDefault()[(int) ClassColumns.ID];
              if (classId != null)
              {
                roleQualification.range = classId.ToString().Trim();
              }
              else
              {
                Utility.WriteString(
                  "\r\n" + type.ToString() + " Does not have an id in Class List While Processing Role Qualification",
                  "error.log", true);
              }
            }
            else
            {
              Utility.WriteString(
                "\r\n" + type.ToString() + " Was Not Found in Class List While Processing Role Qualification",
                "error.log", true);
            }
          }
          if (value != null && value.ToString() != String.Empty)
          {
            var query = from @class in _classes
                        where Convert.ToString(@class[(int) ClassColumns.Label]) == value.ToString()
                        select @class;

            if (query.FirstOrDefault() != null)
            {
              roleQualification.value = new QMXFValue { reference = query.FirstOrDefault()[(int)ClassColumns.ID].ToString().Trim() };
            }
            else
            {
              Utility.WriteString(
               "\r\n" + value.ToString() + " Was Not Found in Class List While Processing Role Qualification",
               "error.log", true);
            }

          }
          else
          {
            //Utility.WriteString("\nType/Value Was Not Set for Role Qualification \"" + englishUsName.value + "\" on template \"" + templateName + "\".", "error.log", true);
          }
          roleQualifications.Add(roleQualification);
        }

        return roleQualifications;
      }
      catch (Exception ex)
      {
        Utility.WriteString("\nError Processing Role Qualification \r\n" +
                            "\nRow: " + roleIndex + " \r\n" + ex.ToString() + "\r\n", "error.log", true);
        throw ex;
      }
    }

    private static List<ArrayList> MarshallToList(WorksheetPartWrapper part)
    {
      try
      {
        var vals = string.Empty;
        var table = new List<ArrayList>();
        foreach (var row in part.Worksheet.SheetData.Rows)
        {
          var value = row.GetCellValue<string>(0);

          var rw = new ArrayList();
          for (var i = 0; i <= row.Worksheet.ColumnSets[0].Columns.Count; i++)
          {
            vals = row.GetCellValue<string>(i) != null ? row.GetCellValue<string>(i).Trim() : string.Empty;
            rw.Add(vals);
          }

          if (rw.Count <= 0) continue;
          rw.Add(row.RowIndex.Value.ToString());
          table.Add(rw);
        }
        return table;
      }
      catch (Exception ex)
      {
        Utility.WriteString("\r\n" + ex.ToString() + "\r\n", "error.log", true);
        throw ex;
      }
    }

    private static void ClearRepository(String repositoryName)
    {
      try
      {
        var q = new QMXF { targetRepository = repositoryName };
        var resp = _refdataClient.Post<QMXF, Response>("/clearall", q, true);
        if (resp.Level == StatusLevel.Error)
        {
          Console.WriteLine("Error in clearing repository: " + repositoryName);
          Utility.WriteString("Error in clearing repository: " + repositoryName + "\r\n" + resp.ToString() + "\r\n", "error.log", true);
        }
        else
          Console.WriteLine("Success: cleared repository: " + repositoryName);
      }
      catch (Exception ex)
      {
        Utility.WriteString("Error in clearing repository: " + repositoryName + "\r\n", "error.log", true);
      }
 
    }
  }
}