# Attendees #
  * Gert
  * Neha
  * Rashmi
  * Aswini
  * Hahn
  * Fang

# Agenda #
  * Update on Subteam meetings
  * iRINGSandbox environment could not deploy...
  * AdapterManager Question
  * JIRA Taskboard


# RefData Subteam #
  * SearchPanel
    * Tree problems
  * Class View/Edit Form

# iRINGSandbox Environment #
  * When deploying 2.0.5 AdapterBinding was bad, result was the adapter/help page would not load
  * When deploying 2.1 similar problem occured and it was a conflict with Facade and Sandbox (see below)
  * Facade and Sandbox problems - were resolved
  * AdapterManager Script path problems - were resolved
  * Cannot configure DataDictionary in AdapterManager
  * Cannot Fetch dataobjects in AppEditor (2.1)
  * DataLayer list is not loading into ComboBox (2.1)

# AdapterManager DataLayer Configurable Flag #
  * Use Base Class and reflection to determine flag value.
    * BaseConfigurableDataLayer - resolved!

# AdapterProvider Issue #
  * InitializeScope is overriding posted Binding in UpdateBinding
    * Recommended to use false in loadDataLayer parameter - Neha is testing - does not work

# Officially Switched to iRINGUG JIRA (http://jira.iringug.org) #
  * You will need to Sign Up for an account on the new JIRA
    * Recommended to use iRINGUserGroup Wiki credentials.
  * Manually move any missing issues you are assigned from Bechtel JIRA to the new one.
  * Start self-assigning Issues form TODO column in new JIRA.



