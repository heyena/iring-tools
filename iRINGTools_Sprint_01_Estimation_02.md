# Sprint 1 Estimation Meeting 2 #

## Agenda ##
  * Review the Estimates from each team member
  * VisualStudio 2010?

## Attendees ##
  * Rob
  * Robertson
  * Gert
  * Koos
  * Hahn
  * Monika
  * Mohd

## Estimates ##
  * Estimate Requirements
    * List User Stories (Issue)
    * List Technical Tasks
    * Estimate Days for each Technical Task

  * [Issue 25](https://code.google.com/p/iring-tools/issues/detail?id=25) - Logging and Silverlight Error Handling - 8 days
    * Implement Error Handling in Silverlight using OnCompletedEventArgs - 3 days
      * DataDictionayEditor - 1 day
      * MappingEditor - 1 day
      * RefDataEditor - 1 day
    * Ensure Silverlight errors are handled properly as well - 3 days
      * DataDictionayEditor - 1 day
      * MappingEditor - 1 day
      * RefDataEditor - 1 day
    * Ensure all errors are logged using Log4Net befroe returning - 2 days
      * AdapterService - 1 day
      * RefDataService - 1 day

  * [Issue 80](https://code.google.com/p/iring-tools/issues/detail?id=80) - Value List Editor - 5 days
    * Development - 3 days
      * Extend CustomItemTree class to accomodate valueMap - 1/2 day
      * Extend Mapping UI to use class changes - 1/2 day
      * Enable ValueMap CRUD - 2 days
    * Testing - 2 days

  * [Issue 81](https://code.google.com/p/iring-tools/issues/detail?id=81) - Warn of Unsaved Changes - 5 days
    * Disable/Enable Save button - 2 days
    * Warn if un-saved and trying to leave - 1/2 day
    * Investigate JavaScript solution - 1 - 2 days
    * Testing - 1 day

  * [Issue 87](https://code.google.com/p/iring-tools/issues/detail?id=87) - Handle Related Items - Blocked on [Issue 98](https://code.google.com/p/iring-tools/issues/detail?id=98) - 8 days
    * Enhancing the DataObject view - 2 days
    * Building mapping expression for related object - 2 day
    * Adding Multi-part identifier - 2 days
    * Testing - 2 days

  * [Issue 88](https://code.google.com/p/iring-tools/issues/detail?id=88) - Property/Relationship Templates - 2 days
    * Fix Bug - 1 day
    * Testing - 1 day

  * [Issue 92](https://code.google.com/p/iring-tools/issues/detail?id=92) - Conditional Mapping - blocked on [Issue 98](https://code.google.com/p/iring-tools/issues/detail?id=98) - 10 days
    * Coordinate Changes in Mapping - 2 day
    * Enhance Mapping Processing to handle conditions - 3 days
    * Exend Mapping UI - Conditions/on template/roles? - 3 days
    * Testing - 2 days

  * [Issue 94](https://code.google.com/p/iring-tools/issues/detail?id=94) - Format Duplicate Result - 2 days

  * [Issue 98](https://code.google.com/p/iring-tools/issues/detail?id=98) - Remove Code Generation - 17 days
    * Mapping structure - 1 day
    * DTO Service - 13 days
      * dto2qxf transformation - 1 day
      * dto2rdf transformation - 2 days
      * get/getList DTO - 2 days
      * post/postLit DTO - 1 day
      * multi-part identifiers - 2 days
      * related items - 3 days
      * others - 2 days
    * Mapping Editor - 3 days
      * dto2mapping transformation - 1 day
      * CRUD mapping processing - 2 days

  * [Issue 105](https://code.google.com/p/iring-tools/issues/detail?id=105) - Selectable Repository / Proper Scoping - 6 days
    * Modify QMXF and change service to accomodate mulitple repositories - 1 day
    * Modify UI to handle QMXF modifications - 4 days
    * Testing - 1 day

  * [Issue 109](https://code.google.com/p/iring-tools/issues/detail?id=109) - Part 8 Compliance - 9 days
    * Understand what RDF should look like - 3 days
      * Reading on owl
      * Insert the data into the triple store and design queries to display it
    * Read the data - 3 days
      * Validate QMXF population
      * Contact domain expert
    * Add queries and functions to add/modify/delete part 8 reference data in the reference data service - 3 days
      * RefDataService should be able to differentiate between rdswip/camelot/part8

  * [Issue 110](https://code.google.com/p/iring-tools/issues/detail?id=110) - Role Flexibility - 3 days

  * [Issue 111](https://code.google.com/p/iring-tools/issues/detail?id=111) - Specialize Templates - 5 days
    * All New DAL, BLL and Controller - 3 days
    * UI changes - 1 day
    * Testing - 1 day

## Scrumy ##
  * All Issues and Estimates have been entered into Scrumy (http://scrumy.com/iring-tools)

## VisualStudio 2010 ##
Team decided:
  * we will use .NET 4 Silverlight 4 and VS 2010.
  * we will wait until everyone has installed before switching.

## ActionItems ##
  * Install VisualStudio 2010 ASAP
  * Send daily progress reports to mailing list, including:
    * completed work items.
    * how much effort is left on current work item?
    * any roadblocks that you have encountered.
  * update Scrumy daily.