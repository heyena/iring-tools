# Attendees #
  * Mohd
  * Neha
  * Rashmi
  * Aswini
  * Hahn
  * Rob

# Agenda #
  * Status Report
  * Discuss UI Consistency

# Status #
  * Neha
    * has been busy with IIP Project
  * Rashmi working on Edit Class - still waiting for PostToReptository
    * Will try to use ARQ to implement PostToTRepository
  * Aswini
    * fixed Add Repository in FedManager
    * working on testing new repository in query - also waiting for PostToRepository
    * When renaming a repository, the tab name does not get updated.
    * Will work on label resolution issue when DataModel uses Java RefDataService.
    * Also will investigate why we have 2 settings for the same service.
  * Mohd
    * setup test environment (Koos helped)
    * started testing
    * PostClass failed in C# refdata service, will debug.
    * Will verify if templates have the same problem.
  * Rob
    * Ninject with Gert
    * Investigated Label Resolution
    * Installation Planning
  * Fang
    * Working NHibernate Configuration Wizard
    * Add message box to alert success or failure on Save.
    * Hahn found a problem with available property list.
    * Rob found a problem with relationships property list.
  * Hahn
    * Working on DtoProjectionEngine2
      * Now handles One to Many Relationships - will require changes to XchMgr and Core Services.
      * Will eventually support composite Mapping - will require change to above AND extend the mapping.
    * Rob would like to get Related Items changes into ExchangeManger for 2.1 and also:
      * Filtered Exchange
      * Cosmetic Stuff

# Discuss UI Consistency #
  * Neha mentioned that we are missing Delete Graph.
  * There are lots of things that need to be tweaked, and other missing items.
  * Rob will provide a list someday