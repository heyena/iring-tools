# iRINGTools-2.02.00.4466 #

A minor release of iRINGTools 2.2 that includes new features.

There are 2 packages:

  * iRINGTools Adapter - The original C#-based iRINGTools web applications and services, some apps have been consolidated into the new AdapterManager.  This requires IIS6 or higher with .NET 4.0 on Windows.
  * iRINGTools Core - A new set of Java-based web applications and services for centralized Directory, Reference Data, Comparison and Exchange services. This requires Apache Tomcat 6 on any OS.


The downloads are available here:
  * [iRINGTools-Adapter-2.2.0](http://iring-tools.googlecode.com/files/iRINGTools-Adapter-2.2.0.zip)
  * [iRINGTools-Core-2.2.0](http://iring-tools.googlecode.com/files/iRINGTools-Core-2.2.0.zip)

A new SDK will be available soon.

A new Installation Guide will be available soon.

## New Features ##
  * New "Open Grid" on Adapter Manager to view raw application data
  * Exchange Manager to exchange data by configurable pool size
  * Improve NHibernate session management and data paging
  * Support XML/JSON posting via Data Service
  * Support URI mapping
  * Enable data layer refresh via Adapter Service
  * Various error handling improvement

## Bug Fixes ##
  * Fix issues:
    * [IRT-122](http://jira.iringug.org:8080/browse/IRT-122)
    * [IRT-124](http://jira.iringug.org:8080/browse/IRT-124)
    * [IRT-127](http://jira.iringug.org:8080/browse/IRT-127)
    * [IRT-131](http://jira.iringug.org:8080/browse/IRT-131)
  * Fix metadata query to query for database tables and views
  * Fix duplicate keys and properties
  * Fix dynamic data layer list population
  * Fix bugs with facade exchange utility

Other Pages: [RoadMap](RoadMap.md) | [ISO15926](ISO15926.md) | [Documentation](Documentation.md) | [Contributors](Contributors.md)