# **iRINGTools** 1.1.1 #

The first patch release for iRINGTools 1.1 that includes QMXF/QXF tools and bug fixes in Reference Data Service, Reference Data Editor and Adapter Service.  The downloads are available [here](http://code.google.com/p/iring-tools/downloads/list)

## New Features ##
  * QMXF Generator and empty InformationModel spreadsheet
  * Julian Bourne's QXF Transforms included in Tools

## Improved Features ##
  * Fully revamped and significantly improved [Installation Guide](http://iring-tools.googlecode.com/svn/wiki/iRINGTools_Installation_Guide_v1_1_1.pdf)

## Bugs Fixed ##
  * In Reference Data Editor, when a template item is selected and 'Edit Template' is clicked then template details are not populated
  * In Reference Data Editor, QMXF for selected tree nodes are not cached
  * In Reference Data Editor, error is raised when there are no results
  * In Reference Data Editor, buttons disable on call and do not re-enable on error
  * In Reference Data Editor, unable to Add/edit classes

## Known Bugs ##
  * In Mapping Editor, collapsing a node fires the definition fetch, unnecessarily.
  * In Mapping Editor, only certain tree nodes are cached.
  * In Mapping Editor, the GraphMap details should include DataObject name.

Other Pages: [Components](Components.md) | [RoadMap](RoadMap.md) | [ISO15926](ISO15926.md) | [Documentation](Documentation.md) | [Contributors](Contributors.md)