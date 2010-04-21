using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using System.Xml.Linq;
using System.Xml.Xsl;
using Ninject;

namespace org.iringtools.adapter
{
  public class RDFTransform : ITransformationLayer
  {
    private AdapterSettings _settings = null;
    private ApplicationSettings _applicationSettings = null;
    
    [Inject]
    public RDFTransform(IKernel kernel, AdapterSettings settings, ApplicationSettings applicationSettings)
    {
      _settings = settings;
      _applicationSettings = applicationSettings;
    }

    public XElement Transform(string graphName, XElement dtoListXML)
    {
      XElement rdf = null;
      try
      {
        string scope = _applicationSettings.ProjectName + "." + _applicationSettings.ApplicationName;
        string mappingPath = _settings.XmlPath + "Mapping." + scope + ".xml";
        string dto2qxfPath = _settings.BaseDirectoryPath + @"Transforms\dto2qxf.xsl";
        string qxf2rdfPath = _settings.BaseDirectoryPath + @"Transforms\qxf2rdf.xsl";

        string dtoFilePath = _settings.XmlPath + "DTO." + scope + "." + graphName + ".xml";
        string qxfPath = _settings.XmlPath + "QXF." + scope + "." + graphName + ".xml";
        string rdfFileName = "RDF." + scope + "." + graphName + ".xml";
        string rdfPath = _settings.XmlPath + rdfFileName;

        XElement mappingXML = Utility.ReadXml(mappingPath);

        dtoListXML.Save(dtoFilePath);

        XsltArgumentList xsltArgumentList = new XsltArgumentList();
        xsltArgumentList.AddParam("dtoFilePath", String.Empty, dtoFilePath);
        xsltArgumentList.AddParam("graphName", String.Empty, graphName);

        // Transform mapping + dto to qxf
        XElement qxf = Utility.Transform(mappingXML, dto2qxfPath, xsltArgumentList);

        // Transform qxf to rdf
        rdf = Utility.Transform(qxf, qxf2rdfPath, null);
      }
      catch (Exception exception)
      {
        throw new Exception("Error transforming DTO to RDF: " + exception.ToString(), exception);
      }

      return rdf;
    }
  }
}
