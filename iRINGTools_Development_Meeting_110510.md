# Attendees #
  * Mohd
  * Neha
  * Rashmi
  * Aswini
  * Gert
  * Koos
  * Hahn
  * Rob

# Agenda #
  * Status Report
  * Discuss Mapping Changes to support Composite Mapping
  * Discuss UI Consistency
  * 2.1 Release (What's Left?)

# Status Report #
  * Mohd - Part 8 Testing
    * Aswini & Rashmi helping
    * Had a meeting
      * Problem creating new repository in Federation Manager
      * PostToRepository - SPARQL Update
    * Rob mentioned that we should use C# refdata service because that is what will be in 2.1
    * Rob also mentioned that Java service completion and FedManager testing should be done immediately after release.
  * Aswini
    * Search Panel (Java) is complete.
    * Regarding New Repository problem - have been investigating and have found the problem.
  * Rashmi
    * waiting for PostToRepository.
  * Neha
    * Search Panel (C#) has an issue with Timeout not sure why.
    * StateManager is now working in AdapterManager
  * Gert
    * Testing AdapterManager
    * Investigating SearchPanel (C#) problem
      * GetDef
        * Includes Counts of...
          * Classification
          * Superclasses
          * Members
          * Subclasses
          * Templates
  * Perhaps we could break it up like so:
    * GetClassifications
    * GetSuperclasses
    * GetMembers
    * GetSubclasses
    * GetTemplates
    * Prototyped Ninject XML-less config
  * Koos
    * Relationships in the Dictionary

