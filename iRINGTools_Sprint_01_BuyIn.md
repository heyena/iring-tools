# Sprint 1 Buy-In Meeting #

## Agenda ##
  * Review the Estimates
  * Clarify and Questions

## Attendees ##
  * Darius Kanga
  * Tom Struzik
  * Ravi Grampurohit
  * Rahul Patil
  * Koos Strydom
  * Robertson Sagolsem
  * Jim Kinter
  * Mohd Moubarak
  * Hahn Le

## Estimation Process ##
  * Development team had 2 meetings...
    * [iRINGTools\_Sprint\_01\_Estimation\_01](iRINGTools_Sprint_01_Estimation_01.md)
    * [iRINGTools\_Sprint\_01\_Estimation\_02](iRINGTools_Sprint_01_Estimation_02.md)
  * Rob did a Effort vs. Work analysis and created a Task Board (http://scrumy.com/iring-tools)
  * The estimates worked out that there was just enough effort to complete all of the items.
  * Koos pointed out that there are some interdependencies.
    * Rob agreed, and noted that no Gant chart was done.
    * However, the team had considered the interdependencies during their estimation meetings.
  * Koos noted that some breathing room was desired by the team.

## Review the Estimates ##
UI clarifications for the following Issues.
  * [Issue 80](https://code.google.com/p/iring-tools/issues/detail?id=80)
    * Darius suggested the possible use of Tabs to seperate Maps from ValueLists
    * Team decided to try it.  If it proves too difficult, then we could simply put them in a sub-node.
  * [Issue 81](https://code.google.com/p/iring-tools/issues/detail?id=81)
    * Rahul asked if we could simply auto-save.
    * Rob pointed out that when we remove code generation, the save will be the only barrier to behavior changes.
  * [Issue 87](https://code.google.com/p/iring-tools/issues/detail?id=87)
    * How should related items appear in DataObject pane?
    * Team decided on the following:
```
 Line
  tag
  diameter
  Valve (1.*)
   tag
   type
 Valve
  tag
  type
```
    * Koos and Darius pointed out that sometimes Valve should appear separately and sometimes not.
    * Hahn noted that we need to handle both cases.
    * Agreed to add a property to the DataDictionary to control whether object is visible
  * [Issue 92](https://code.google.com/p/iring-tools/issues/detail?id=92)
    * Rob suggested we push this item off to the next Sprint.
    * Darius suggested that it needs to be fixed, as this is restricting use.
    * The UI estimate was unclear, and was probably too small.
    * Team decided to not extend the UI now (-3 days), but to enable the feature in XML configuration.
  * [Issue 94](https://code.google.com/p/iring-tools/issues/detail?id=94)
    * Darius suggested that Repository Names be added to ALL duplicates when dupes are found.
    * Koos suggested that Repository Names be added to ALL items
    * Team decided on the following:
```
TAG NAME (ReferenceData)
TAG NAME (My Private Sandbox)
TAG NAME (My Private Sandbox) [Duplicate]
TAG NAME (My Private Sandbox) [Duplicate 2]
```
  * Darius added [Issue 89](https://code.google.com/p/iring-tools/issues/detail?id=89)
    * Rob mentioned that entity type was excluded for a reason, but would get back to the team.
    * Upon further analysis, it was decided to add and see how it goes.
    * Rob agreed that this item was very small, and easy to add.
      * Make TextBox editable in UI, and write rdf:type statement on QMXF Post (+1 day).
  * [Issue 105](https://code.google.com/p/iring-tools/issues/detail?id=105), [Issue 110](https://code.google.com/p/iring-tools/issues/detail?id=110), [Issue 111](https://code.google.com/p/iring-tools/issues/detail?id=111)
    * Team discussed possible UI variations regarding Repository scope, and Templates.
    * Team decided on the following:
      * Classes UI would be changed
        * Specialization and Classification would:
          * Remove "Add" and "Remove" buttons.
          * Add "Edit" button.
        * Specialization and Classification UIs would be added.
          * list additional information (repository, read/write access)
          * "Add" and "Remove" buttons
          * Combo box to select writable repository used to Add.
      * Template UI would get new Parent Template selection field.
      * Templates UI would get a radio button for
        * Base Template
          * Disable Parent Template selection field
        * Specialized Template
          * Enable Parent Template Selection
            * OnSelect - populate roles from parent
            * Role Name and Description are not editable.
      * Template UI - Roles Block would get new Value selection field.
      * Templates UI - Roles Block would get new radio button for
        * Value
          * Disable Range selection field
          * Enable Value selection field
        * Range
          * Enable Range selection field
          * Disable Value selection field

## ActionItems ##
  * Team to document UI changes for each Issue above with PowerPoint Slides.
  * Team to send slides to Darius.