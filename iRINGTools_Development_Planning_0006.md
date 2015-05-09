# Attendees #
  * Hahn
  * Mohd
  * Koos
  * Pavan
  * Robertson
  * Rob

# Issues #
  * GraphName cannot be the same as ANY DataObjectName
  * ValueList resolution fails on nulls - chekc both directions
  * DataLayer does not truncate values that are too long - fixed in trunk
  * DataLayer does not handle null values for nullable fields - fixed in trunk
  * Cannot handle null roles in Template Instances - maybe fixed in trunk
  * Cannot handle multi-column keys
  * Performance of Refresh using SemWeb is very poor
  * Performance of Pull is subject to lag due to SPARQL chattiness
  * IService.Generated methods do not work if more than one app
  * NHibernate could be configured for Normalized Database, but the DTOLayer, and specifically the mapping cannot handle it.
  * Mapping branching, would allow a sinlge Instrument graph to produce specialized Instrument Instance like Vortex Flow Meter.
  * Mapping level can only be so deep due to XmlSerializer.
  * DBDictionaryUtility does not handle Large DatabaseDictionary Xml due to WCF message size restrictions (should be configuration fix).
  * DBDictionaryUtility does not support proxy credentials or endpoint credentials.
  * Need to Enhance the build script to include SVN revision and centrally controlled version number in AssemblyInfo.
  * Need to handle and/or report to user issue with invalid configuration in DBDictionary and Mapping.
  * All UIs need to adjust the state of the UI based on the context, or currently selected item/current task.
  * RefDataEditor cannot create/edit Qualified Template.
  * DBDictionaryUtility does not verify the ConnectionString - fixed in trunk

# Action Items #
  * Team to enter these items into Google Code.