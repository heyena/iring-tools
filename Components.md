# iRINGTools Components #

[iRINGTools](http://code.google.com/p/iring-tools/downloads/detail?name=iRINGTools-111.zip) is comprised of:
  * **[iRINGTools Adapter](http://code.google.com/p/iring-tools/downloads/detail?name=iRINGAdapter-111.zip)**
    * **Adapter Service** - WCF REST Service for converting legacy systems to ISO 15026 and pulling data from ISO 15926 into legacy systems.
    * **Interface Service** - [Joseki](http://www.joseki.org) SPARQL Protocol server packaged for use as ISO 15926 instance data endpoint.
    * **Mapping Editor** - Silverlight application for mapping legacy systems to ISO 15926.
    * **Tools**
      * **AdapterClient** - simple desktop client for making Pull requests.
      * **EncryptCredentials** - command line tool for encrypting credentials in web.configs
  * **[iRINGTools Sandbox](http://code.google.com/p/iring-tools/downloads/detail?name=iRINGSandbox-111.zip)**
    * **Reference Data Service** - WCF REST Service providing federated query across multiple ISO 15926 reference data endpoints.
    * **Sandbox Service** - [Joseki](http://www.joseki.org) SPARQL Protocol server packaged for use as ISO 15926 reference data endpoint.
    * **Reference Data Editor** Silverlight application for browsing ISO 15926 reference data
    * **[QXF](http://ns.ids-adi.org/qxf/index.html) Tools** (Temporary Solution)
      * **InformationModel Spreadsheet** - Excel spreadsheet for defining classes and templates.
      * **QMXFGenerator** - command line tool for generating QMXF from a the InformationModel spreadsheet.
      * **Transformations** - DTD, XSD, XML and XSLT files for coverting QMXF to RDF.

Other Pages: [RoadMap](RoadMap.md) | [ISO15926](ISO15926.md) | [Documentation](Documentation.md) | [Contributors](Contributors.md)