## Agenda ##
  * Overview
  * Reviewing Issues
  * Plan forward Mapping Editor
  * Hard stop at 9:30AM EST

## Attendees ##
  * Koos
  * Gert
  * Mohd
  * Rob
  * Hahn
  * Pavan

## Overview ##
  * Bootstrapping and Packaging
  * Now Focused on...
    * MappingEdidtor
    * Part 8 Compliance
  * Problems found yesterday
    * Model wants to specialize a new class as a existing one.
    * QMXF cannot define Sepcialization on it own.
    * RefDataEditor cannot post a class definition in a federated manner.

## AdapterService Issues ##

| **Priority** | **Issue #** | **Issue Description** | **Assigned** |
|:-------------|:------------|:----------------------|:-------------|
| 1 | [Issue 60](https://code.google.com/p/iring-tools/issues/detail?id=60) | SPARQL generation does not handle mapping changes. | Monika |
| 1 |  | TripleStore needs to be application specific, or maybe project specific | Rob |

## MappingEditor Issues ##

Please work off issues in order and keep the list statused.  Feel free to take items that are assigned to other off clock developers, if you can complete it immediately.  Be sure to coordinate with assigned developer.

| **Priority** | **Issue #** | **Issue Description** | **Assigned** |
|:-------------|:------------|:----------------------|:-------------|
| 2 | done | MakeClassRole Button | Hahn |
| 3 | done | Fixed Roles should be filled on Adding TemplateMap | Hahn |
| 3 | done | When a role is mapped, the details need to update | Hahn |
| 3 | done | Reference Roles - being mapped with the wrong classId | Koos |
| 3 |  | Detail Pane Cleanup | Pavan |
| 5 | [Issue 52](https://code.google.com/p/iring-tools/issues/detail?id=52) | In MappingEditor class roles are not identified if more that one level of superclassing is used | Hahn |
| 9 |  | Second clicks on Nodes makes web service call, in Search and Mapping Trees. | Koos |
| 9 | [Issue 53](https://code.google.com/p/iring-tools/issues/detail?id=53) | In MappingEditor non-class roles should be editable depending on value(literal/reference)/range. | Pavan |