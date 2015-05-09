# **iRINGTools** 2.00.03.2742 #

A patch release of [iRINGTools 2.0](http://code.google.com/p/iring-tools/wiki/ReleaseNotice_203) that includes bug fixes.

The download is available [here](http://iring-tools.googlecode.com/files/iRINGTools-2.0.3.zip).
The SDK can be downloaded from [here](http://iring-tools.googlecode.com/files/iRINGTools-SDK-2.0.3.zip)

[Installation Guide](http://iring-tools.googlecode.com/files/iRINGTools_Installation_Guide_v2.0.1.pdf)

[Users Guide](http://iring-tools.googlecode.com/files/iRINGTools_Users_Guide_v2.0.1.pdf)

[SDK Guide](http://iring-tools.googlecode.com/files/iRINGTools_SDK_Guide_v2.0.2.pdf)

## New Features ##
  * Added ConfigurationTool

## Bugs Fixed ##
  * URI of Related Individual is not unique
  * Identifier is missing from the URI on index format
  * BaseGraphUri is not valid for all services.
  * indexStyle is not honored on data service.
  * data service allows other formats, but defaults to raw XML
  * rdf:type vs ClassificationTemplate/ClassificationOfIndividual
  * Move configuration of above to global scope.
  * Nulls in DataLayer should not create empty Individual or relating Template
  * MappingEditor is not producing the properly qualified propertyName for related objects

Other Pages: [RoadMap](RoadMap.md) | [ISO15926](ISO15926.md) | [Documentation](Documentation.md) | [Contributors](Contributors.md)