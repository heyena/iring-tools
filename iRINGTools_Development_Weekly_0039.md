## Agenda ##
  * Issues & New Features
    * Identity Issue (How and When we need to create and use IdsAdi Identities)
    * dotNetRdf Issue (Resolved)
    * Shelved Mapping Changes
    * Pending URI Changes for DataTransfer (DTI & DTO)
    * RefData Editor Issues
    * Mapping Editor Issues
    * Logging Idea
    * New DataLayers
  * What goes in the release?

## Attendees ##
  * Rob
  * Hahn
  * Ritu
  * Koos
  * Robertson
  * Gert
  * Aswini
  * Mohamed

## Issues & New Features ##
  * IdsAdiIdentity and ID Generator.
    * IdsAdiIdentities are not easily made/used
    * RDS/WIP
    * Solution is...
      * use IdsAdi accounts for now
      * Add ID Generator to 2.1
      * Add Feature to RefDataEditor to ask for credentials and pass them to the server.
  * dotNetRdf Issue will be in 2.0 - Rob/Koos
  * Mapping Changes will be in 2.1
  * URI Changes will be 2.0 - Hahn/Rob
  * Logging Idea deferred to Robin/Darius/Lee/others?
  * RefDataEditor Issues
    * fix these now - Rob/Robertson
      * Copy to clipboard button can go, Silverlight provide Ctrl-C/Ctrl-V functionality. - Rob
      * Form should fit in the default pane size. (Resizing repository combo might help) - Robertson
      * Role Editor should be wider (2x) - Robertson
      * Range ComboBox should be wider, and have an "Select" Button. - Robertson
      * Range ComboBox should only show the Label and not "SelectedClass". - Robertson
      * OnOK should wait until success to close form. - Rob
    * fixed in 2.1?
      * Many Expected Errors - this stinks!
      * Dupes being created? - Mohd
      * Entity Type should be assignable instead of needing to type it.
      * Role Range/Value could be shown in Tempalte Editor?

