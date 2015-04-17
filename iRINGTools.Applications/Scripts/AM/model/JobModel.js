Ext.define('AM.model.JobModel', {
    extend: 'Ext.data.Model',

    fields: [
        {

            name: 'job_Id'
        },
        {
            name: 'SiteId'
        },
        {
            name: 'Scope'
          
        },
        {
            name: 'App'
        },
        {
            name: 'DataObject'   
        },
        {
            name: 'Active'
            
        },
        {
            name: 'Last_Start_DateTime'
        },
        {
            name: 'Next_Start_DateTime'
        },
        {
            name: 'Start_DateTime'
        },
        {
            name: 'End_DateTime'
        }, 
        {
            name: 'PlatformId'
        }, 
        {
            name: 'TotalRecords'
        },
        {
            name: 'CachedRecords'
        }
    ]
});
   
   
