# **iRINGTools** 1.2.0 Beta 2 #

A beta release of iRINGTools 1.2 that includes NHibernate and Multi-Project/Application support in the Adapter and SemWeb SPARQL endpoints for Interface and Sandbox.  The download is available [here](http://iring-tools.googlecode.com/files/iRINGTools-120beta2.zip)

[Installation Guide](http://iring-tools.googlecode.com/files/iRINGTools_Installation_Guide_v1_2_0.Draft.pdf)

[Migration Guide](http://iring-tools.googlecode.com/files/iRINGTools_Migration_Guide_v1_2_0.Draft.pdf)

## New Features ##
  * Multi-Project/Application Support in the Adapter Service.
  * PullDTO feature in the Adapter Service.
  * NHibernate Datalayer to enable non-commercial Oracle support and above features.
  * SemWeb SPARQL Endpoint for Interface Service.
  * SemWeb SPARQL Endpoint for Sandbox Service.
  * DBDictionaryUtility for bootstrapping the AdapterService with your legacy application.

## Known Bugs ##
  * PullDTO is not working.
  * TripleStore refesh is broken due changes in Mapping.
  * SPARQL pull is broken due to changes in Mapping.
  * RefDataService cannot write to SemWeb SandboxService, but still works with Joseki SandboxService.

Other Pages: [Components](Components.md) | [RoadMap](RoadMap.md) | [ISO15926](ISO15926.md) | [Documentation](Documentation.md) | [Contributors](Contributors.md)