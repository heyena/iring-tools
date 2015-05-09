# iRINGTools-2.01.00.3963 #

A minor release of iRINGTools 2.1 that includes new features.

There are now 2 packages:

  * iRINGTools Adapter - The original C#-based iRINGTools web applications and services, some apps have been consolidated into the new AdapterManager.  This requires IIS6 or higher with .NET 4.0 on Windows.
  * iRINGTools Core - A new set of Java-based web applications and services for centralized Directory, Reference Data, Comparison and Exchange services. This requires Apache Tomcat 6 on any OS.


The downloads are available here:
  * [iRINGTools-Adapter-2.1.0](http://iring-tools.googlecode.com/files/iRINGTools-Adapter-2.1.0.zip)
  * [iRINGTools-Core-2.1.0](http://iring-tools.googlecode.com/files/iRINGTools-Core-2.1.0.zip)

A new SDK will be available soon.

A new Installation Guide will be available soon.

## New Features ##
  * Core
    * Exchange Manager
      * Application Data Grid
      * Exchange Data Grid
    * Federation Manager (Alpha)
    * Directory Service
    * Exchange Service
    * Diferencing Service
    * History Service
    * Java-based Reference Data Service (Alpha)
  * Adapter
    * JSON Projection
    * HTML Projection (CSS customizable)
    * Adapter Manager
      * Removes Silverlight Requirement.
      * Spreadsheet Configuration (requires SpreadsheetDataLayer)
      * NHibernate Configuration
      * Drag and Drop Mapping

Other Pages: [RoadMap](RoadMap.md) | [ISO15926](ISO15926.md) | [Documentation](Documentation.md) | [Contributors](Contributors.md)