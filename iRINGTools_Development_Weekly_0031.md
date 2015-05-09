## Agenda ##
  * Status Reports
  * Plan Release

## Attendees ##
  * Ritu
  * Hahn
  * Rob
  * Gert
  * Robertson
  * Aswini
  * Mohd

## Status ##
  * Robertson - Reviewing ongoing changes, and awaiting tasks.
  * Mohd
    * Finished the SPARQL changes for classes, based on discussion and email.
    * Planning on testing single query functions.
    * Need a OWL file with All parts and instances required for testing.
  * Gert
    * Creating a prototype UI with ASP.NET MVC and EXT JS.
    * Planning on polishing the prototype for demo to UserGroup.
    * Will demo MVC and EXT later.
  * Hahn
    * Working on prototyping services
      * DirectoryService to serve the directory.xml with Exchange Definitions.
      * DifferencingService
        * Gets the Identifiers from the Adapter and produces a DXI based on ExchangeId.
        * Gets the DTO from the Adapter and produces a DXO based on page of DXI.
    * Planning on prototyping the Grid for Aswini and Ritu.
  * Aswini
    * Created design slide of R&A Grid Details.
    * Concerned about marshalling performance in PHP.
  * Ritu
    * Working on the class for marshalling in PHP.
  * Rob
    * Building and Deploying and assisting Gert.
    * Planning on delivering the relase ASAP.

## MVC Demo ##

## Plan ##
  * iRINGTools.ESB - Aswini, Hahn and Ritu
  * ASP.NET MVC & EXT JS - Gert - will coordinate with Aswini and Ritu.
  * Release 2.0.0
    * Build and Package - Rob
    * Part 8 SPARQL - Mohd
    * Part 8 RefDataService change - Robertson
    * RdfProjectionEngine.GetDataObjects is not done.
    * XmlProjectionEngine.GetXml is not done.