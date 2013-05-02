package org.iringtools.hibernate;

import java.util.List;

import org.iringtools.common.response.Status;
import org.iringtools.ext.ResponseExtension;
import org.iringtools.ext.StatusExtension;
import org.iringtools.library.DataObject;
import org.iringtools.library.DatabaseDictionary;
import org.iringtools.refdata.response.Response;

public class EntityGenerator {
	
	 private String _namespace = "";
	 private HibernateSettings _settings = null;
	 private String NAMESPACE_PREFIX = "org.iringtools.adapter.datalayer.proj_";
	 private String COMPILER_VERSION = "v3.5";
	 private List<DataObject> _dataObjects = null;
	 private StringBuilder _mappingBuilder = null;
	 
	    
	 
    public EntityGenerator(HibernateSettings settings)
    {
      _settings = settings;
    }

    public Response Generate(DatabaseDictionary dbSchema, String projectName, String applicationName)
    {
      ResponseExtension response = new ResponseExtension();
      Status status = new Status();

      if (!dbSchema.getDataObjects().isEmpty())
      {
        _namespace = NAMESPACE_PREFIX + projectName + "." + applicationName;
        _dataObjects = dbSchema.getDataObjects();

        try
        {
          status.setIdentifier(String.format("%1$s.%2$s", projectName, applicationName));
	        
         // Directory.CreateDirectory(_settings["XmlPath"]);

          _mappingBuilder = new StringBuilder();
          _mappingWriter = new XmlTextWriter(new StringWriter(_mappingBuilder));
          _mappingWriter.Formatting = Formatting.Indented;

          _mappingWriter.WriteStartElement("hibernate-mapping", "urn:nhibernate-mapping-2.2");
          _mappingWriter.WriteAttributeString("default-lazy", "true");

          _dataObjectBuilder = new StringBuilder();
          _dataObjectWriter = new IndentedTextWriter(new StringWriter(_dataObjectBuilder), "  ");

          _dataObjectWriter.WriteLine(Utility.GeneratedCodeProlog);
          _dataObjectWriter.WriteLine("using System;");
          _dataObjectWriter.WriteLine("using System.Collections.Generic;");
          _dataObjectWriter.WriteLine("using Iesi.Collections.Generic;");
          _dataObjectWriter.WriteLine("using org.iringtools.library;");
          _dataObjectWriter.WriteLine();
          _dataObjectWriter.WriteLine("namespace {0}", _namespace);
          _dataObjectWriter.Write("{"); // begin namespace block
          _dataObjectWriter.Indent++;

/*          foreach (DataObject dataObject in dbSchema.dataObjects)
          {
            // create namespace for dataObject
            dataObject.objectNamespace = _namespace;

            CreateNHibernateDataObjectMap(dataObject);
          }

          _dataObjectWriter.Indent--;
          _dataObjectWriter.WriteLine("}"); // end namespace block                

          _mappingWriter.WriteEndElement(); // end hibernate-mapping element
          _mappingWriter.Close();
*/
          String mappingXml = _mappingBuilder.ToString();
          String sourceCode = _dataObjectBuilder.ToString();

/*          #region Compile entities
          Dictionary<string, string> compilerOptions = new Dictionary<string, string>();
          compilerOptions.Add("CompilerVersion", COMPILER_VERSION);

          CompilerParameters parameters = new CompilerParameters();
          parameters.GenerateExecutable = false;
          parameters.ReferencedAssemblies.Add("System.dll");
          parameters.ReferencedAssemblies.Add(_settings["BinaryPath"] + "Iesi.Collections.dll");
          parameters.ReferencedAssemblies.Add(_settings["BinaryPath"] + "iRINGLibrary.dll");
          NHIBERNATE_ASSEMBLIES.ForEach(assembly => parameters.ReferencedAssemblies.Add(_settings["BinaryPath"] + assembly));


          Utility.Compile(compilerOptions, parameters, new string[] { sourceCode });
          #endregion Compile entities

          #region Writing memory data to disk
          string hibernateConfig = CreateConfiguration(
            (Provider)Enum.Parse(typeof(Provider), dbSchema.Provider, true), 
            dbSchema.ConnectionString, dbSchema.SchemaName);
          Utility.WriteString(hibernateConfig, _settings["XmlPath"] + "nh-configuration." + projectName + "." + applicationName + ".xml", Encoding.UTF8);
          Utility.WriteString(mappingXml, _settings["XmlPath"] + "nh-mapping." + projectName + "." + applicationName + ".xml", Encoding.UTF8);
          Utility.WriteString(sourceCode, _settings["CodePath"] + "Model." + projectName + "." + applicationName + ".cs", Encoding.ASCII);
          DataDictionary dataDictionary = CreateDataDictionary(dbSchema.dataObjects);
          Utility.Write<DataDictionary>(dataDictionary, _settings["XmlPath"] + "DataDictionary." + projectName + "." + applicationName + ".xml");
          #endregion
*/
          status.getMessages().getItems().add("Entities generated successfully.");
        }
        catch (Exception ex)
        {
          throw new Exception("Error generating application entities " + ex);

          //no need to status, thrown exception will be statused above.
        }
      }

      response.append(status);
      return response;
    }
    
}
