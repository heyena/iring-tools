using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Office.Core;
using Excel = Microsoft.Office.Interop.Excel;
using log4net;
using Ninject;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;

namespace Hatch.DataLayers.iPasXL
{
  public class iPasXLEntityGenerator
  { 
    private string NAMESPACE_PREFIX = "Hatch.DataLayers.iPasXL.Model_";
    private string COMPILER_VERSION = "v4.0";
    private List<string> EXTRA_ASSEMBLIES = new List<string>() 
    {      
    };

    private static readonly ILog _logger = LogManager.GetLogger(typeof(iPasXLEntityGenerator));

    private string _namespace = String.Empty;
    private iPasXLSettings _settings = null;
    private IndentedTextWriter _dataObjectWriter = null;
    private StringBuilder _dataObjectBuilder = null;

    public iPasXLEntityGenerator(iPasXLSettings settings)
    {
      _settings = settings;
    }

    public Response Generate(string filePath, string projectName, string applicationName)
    {
      Response response = new Response();
      Status status = new Status();

      iPasXLConfiguration iPasXLConfig = CreateiPasXlConfig(filePath);

      if (iPasXLConfig.Worksheets != null)
      {
        _namespace = NAMESPACE_PREFIX + projectName + "_" + applicationName;        

        try
        {
          status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

          Directory.CreateDirectory(_settings["XmlPath"]);
                              
          _dataObjectBuilder = new StringBuilder();
          _dataObjectWriter = new IndentedTextWriter(new StringWriter(_dataObjectBuilder), "  ");

          _dataObjectWriter.WriteLine(Utility.GeneratedCodeProlog);
          _dataObjectWriter.WriteLine("using System;");
          _dataObjectWriter.WriteLine("using System.Collections.Generic;");          
          _dataObjectWriter.WriteLine("using org.iringtools.library;");
          _dataObjectWriter.WriteLine();
          _dataObjectWriter.WriteLine("namespace {0}", _namespace);
          _dataObjectWriter.Write("{"); // begin namespace block
          _dataObjectWriter.Indent++;

          foreach (iPasXLWorksheet cfWorksheet in iPasXLConfig.Worksheets)
          {
            CreateGenericDataObjectMap(cfWorksheet);
          }

          _dataObjectWriter.Indent--;
          _dataObjectWriter.WriteLine("}"); // end namespace block          

          string sourceCode = _dataObjectBuilder.ToString();

          #region Compile entities
          Dictionary<string, string> compilerOptions = new Dictionary<string, string>();
          compilerOptions.Add("CompilerVersion", COMPILER_VERSION);

          CompilerParameters parameters = new CompilerParameters();
          parameters.GenerateExecutable = false;
          parameters.ReferencedAssemblies.Add("System.dll");          
          parameters.ReferencedAssemblies.Add(_settings["BinaryPath"] + "iRINGLibrary.dll");
          EXTRA_ASSEMBLIES.ForEach(assembly => parameters.ReferencedAssemblies.Add(_settings["BinaryPath"] + assembly));


          Utility.Compile(compilerOptions, parameters, new string[] { sourceCode });
          #endregion Compile entities

          #region Writing memory data to disk
          Utility.Write<iPasXLConfiguration>(iPasXLConfig, _settings["XmlPath"] + "iPasXL-Configuration." + projectName + "." + applicationName + ".xml", true);
          Utility.WriteString(sourceCode, _settings["CodePath"] + "Model." + projectName + "." + applicationName + ".cs", Encoding.ASCII);
          #endregion

          status.Messages.Add("Entities generated successfully.");
        }
        catch (Exception ex)
        {
          throw new Exception("Error generating application entities " + ex);

          //no need to status, thrown exception will be statused above.
        }
      }

      response.Append(status);
      return response;
    }

    private iPasXLConfiguration CreateiPasXlConfig(string filePath)
    {      
      Excel.Application xlApplication = null;
      Excel.Workbook xlWorkBook = null;
      iPasXLConfiguration config = new iPasXLConfiguration()
      {
        Worksheets = new List<iPasXLWorksheet>()
      };

      try
      {
        xlApplication = new Excel.Application();
        xlWorkBook = xlApplication.Workbooks.Open(filePath, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

        config.Location = filePath;

        foreach (Excel.Worksheet xlWorkSheet in xlWorkBook.Worksheets)
        {
          iPasXLWorksheet cfWorkSheet = new iPasXLWorksheet() {
            Name = xlWorkSheet.Name,
            Columns = new List<iPasXLColumn>()
          };

          Excel.Range usedRange = xlWorkSheet.UsedRange;

          for(int i = 1; i <= usedRange.Columns.Count; i++) 
          {
            string header = usedRange.Cells[1, i].Value2;
            if (header != null && !header.Equals(String.Empty))             
            {
              iPasXLColumn cfColumn = new iPasXLColumn() {
                Index = i,
                Name = header,
                DataType = DataType.String
              };

              cfWorkSheet.Columns.Add(cfColumn);
            }
          }

          if (cfWorkSheet.Columns.Count > 0)
          {
            cfWorkSheet.Identifier = cfWorkSheet.Columns[0].Name;
            config.Worksheets.Add(cfWorkSheet);
          }
        }

        return config;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Createing iPasXL Configuration: " + ex);
        return config;
      }
      finally
      {
        if (xlWorkBook != null)
        {
          xlWorkBook.Close(true, Type.Missing, Type.Missing);
          System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorkBook);
          xlWorkBook = null;
        }

        if (xlApplication != null)
        {
          xlApplication.Quit();
          System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApplication);
          xlApplication = null;
        }

        GC.Collect();
      }
    }

    private void CreateGenericDataObjectMap(iPasXLWorksheet cfWorkSheet)    
    {
      _dataObjectWriter.WriteLine();
      _dataObjectWriter.WriteLine("public class {0} : IDataObject", cfWorkSheet.Name);
      _dataObjectWriter.WriteLine("{"); // begin class block
      _dataObjectWriter.Indent++;
      
      #region Process columns
      if (cfWorkSheet.Columns != null && cfWorkSheet.Columns.Count > 0)
      {
        foreach (iPasXLColumn column in cfWorkSheet.Columns)
        {
          _dataObjectWriter.WriteLine("public virtual {0} {1} {{ get; set; }}", column.DataType, column.Name);                      
        }

        // Implements GetPropertyValue of IDataObject
        _dataObjectWriter.WriteLine();
        _dataObjectWriter.WriteLine("public virtual object GetPropertyValue(string propertyName)");
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++; _dataObjectWriter.WriteLine("switch (propertyName)");
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++;        

        foreach (iPasXLColumn column in cfWorkSheet.Columns)
        {
          _dataObjectWriter.WriteLine("case \"{0}\": return {0};", column.Name);
        }

        _dataObjectWriter.WriteLine("default: throw new Exception(\"Property [\" + propertyName + \"] does not exist.\");");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");

        // Implements SetPropertyValue of IDataObject
        _dataObjectWriter.WriteLine();
        _dataObjectWriter.WriteLine("public virtual void SetPropertyValue(string propertyName, object value)");
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("switch (propertyName)");
        _dataObjectWriter.Write("{");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine();

        foreach (iPasXLColumn column in cfWorkSheet.Columns)
        {
          _dataObjectWriter.WriteLine("case \"{0}\":", column.Name);
          _dataObjectWriter.Indent++;

          bool isColumnNullable = (column.DataType == DataType.String);
          if (isColumnNullable)
          {
            _dataObjectWriter.WriteLine("if (value != null) {0} = Convert.To{1}(value);", column.Name, column.DataType);
          }
          else
          {
            _dataObjectWriter.WriteLine("{0} = (value != null) ? Convert.To{1}(value) : default({1});", column.Name, column.DataType);
          }

          _dataObjectWriter.WriteLine("break;");
          _dataObjectWriter.Indent--;
        }

        _dataObjectWriter.WriteLine("default:");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("throw new Exception(\"Property [\" + propertyName + \"] does not exist.\");");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");
        #endregion Process columns

        #region generate GetRelatedObjects method
        _dataObjectWriter.WriteLine();
        _dataObjectWriter.WriteLine(@"public virtual IList<IDataObject> GetRelatedObjects(string relatedObjectType)");
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("switch (relatedObjectType)");
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++;

        _dataObjectWriter.WriteLine("default:");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("throw new Exception(\"Related object [\" + relatedObjectType + \"] does not exist.\");");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");
        #endregion

        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}"); // end class block        
      }
    }
  }
}
