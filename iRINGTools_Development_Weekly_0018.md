## Agenda ##
  * Continue Related Items Discussion
  * Status
  * WCF 4 Adapter
  * .NET 4 Web.config

## Attendees ##
  * Rob
  * Mohd
  * Hahn
  * Robertson
  * Monika
  * Gert
  * Koos
  * Robin

## Related Items ##
  * DatabaseDictionary
    * Needed to be modified to properly describe relationships
    * MyGeneration
    * Natural keys vs. primary keys & foreign keys
    * Ninject Binding and DataDictionary
  * Use Case
    * Lines related to Valves by natural composite key
      * Line.Area, Line.Trainnumber, Line.System, Line.Lineno
      * Valve.Varea, Valve.VTrain, Valve.Vnum, Valve.VLineno
  * Relationship definition
  * DataDictionary vs. DatabaseDictionary
    * DataDictionary is used by Adapter to drive interaction with all datalayers
    * DatabaseDictionary is used by NHDataLayer to drive generation of NH and Adapter artifacts.
  * Scope of Estimate
    * DatabaseDictionary class (inherits from DataDictionary)
    * DatabaseDictionary Editor
    * EntityGenerator
    * NHibernateDataLayer
    * DataDictionary (needs to be done in conjunction with DatabaseDictionary)
    * Mapping (need to review current design)
    * Mapping Editor
    * ProjectionEngines (We can help with these)
> Assume 3 days (24 hrs) for each.  Depends on Mapping changes.
    * Xml - Reporting Object
    * Dto - DataExchange Object
    * Qtxf - QXF (Template-based Xml) with Label-based element names
    * Rdf - Semantic Representation