## Agenda ##
  * Sprint 1 and 2.0.0 release
  * Status Reports
  * Koos' DataLayer / Gert DataLayer

## Attendees ##
  * Robertson
  * Rob
  * Robin
  * Hahn
  * Ritu
  * Koos
  * Gert
  * Mohd

## Sprint 1 Completion and 2.0.0 Release ##
  * MVC Dlls
  * Awaiting dotNetRDf change to enable better deployment
  * XmlProjectionEngine
  * Sprint Review Meeting

## Status ##
  * Robertson
    * Externalizing SPARQLs - awaiting code review.
    * Had a discussion with Mohd - added new SPARQLs.
  * Rob
    * Preparing for Internal Sprint
    * Working with SDK implementors to deploy and test Datalayers
    * Trying to work out generic Identity Solution that makes sense for our project.
  * Gert
    * Working on ExcelDataLayer
      * Complex Ranges
      * Removed Code Generation and went with dynamic DataDictionary and a generic class.
      * Configure on an as-needed basis.
    * Drawing Pretty Pictures.
  * Mohd
    * Still working with Darius to nail down requirements.
    * In the meantime working with Robertson.
    * May be bugs reported through testing.
  * Koos
    * Primairly busy with setting up and testing in Internal Environment.
    * Creating DataLayers and deploying them.
    * Using DemoControlPanel for Middleware.
  * Ritu
    * Prototyped Marshalling of Directory XML into JSON.
    * Helped out with Sprint Planning.
    * Documenting the PHP environment and how to setup.
    * Next will prototype the About Screen.
  * Hahn
    * Completed ProjectionEngines to complete Related Item scope.
      * XmlProjectionEngine bug where we need to return XDocument on XElement
      * Need JSONProjectionEngine for web developers
      * DTOProjectionEngines still need to be changed to support Related Items.
      * Need DTOProjectionEngine to use JSON for performance.
    * Working on prototyping WSO2 Sequences.