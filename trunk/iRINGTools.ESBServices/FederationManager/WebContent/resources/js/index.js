Ext.onReady(function(){
    var p = new Ext.Panel({
        title: 'My Panel',
        collapsible:true,
        renderTo: 'panel-basic',
        width:400
    });
    
    p.load({  
        url: 'FederationManager/repositories.action',  
        method: 'GET'
    });  
});
