/*
 * @File Name : FederationManager.js
 * @Path : resources/js
 * @Using Lib : Ext JS Library 3.2.1(lib/ext-3.2.1)
 * @author by Ritu Garg
 *
 * This file intended to make Layout of Federation Manager
 * by using different Extjs custom classes
 *
 */
var federationPanel
Ext.onReady(function () {

	Ext.BLANK_IMAGE_URL = 'resources/images/s.gif';
	Ext.QuickTips.init();

	Ext.state.Manager.setProvider(new Ext.state.CookieProvider());

	 federationPanel = new FederationManager.FederationPanel({
	  id:'federation-panel',
	  region:'west',

	  collapsible: true,
	  collapsed: false,

	  border: true,
	  split: true,

	  width: 350,
  	  minSize: 175,
  	  maxSize: 500,
	  //url: 'federation'
          url:'federation-tree.json'
	});

	var tabPanel = new Ext.TabPanel({
		region: 'center',
		id: 'contentPanel',
		margins: '0 5 0 0',
                disabled:true,
		enableTabScroll: true                
	});

         tabPanel.on('tabChange', function(p){
           if(Ext.getCmp('contentPanel').items.length !=0){ // check is there any tab in contentPanel
            var nodeObj = federationPanel.getNodeBySelectedTab(Ext.getCmp('contentPanel').getActiveTab())
            if(nodeObj){
                federationPanel.selectNode(nodeObj)
            }
           }else{
              // code for unselect
           }
        });
        
	federationPanel.on('edit', function(panel, node, label, formData) {

                var newTab = new FederationManager.ContentPanel({
                        title: label,
                        id:'tab-' + node.id,
                        configData: formData,
                        nId:node.id,
                        //url: 'postFederation',
                        url:'save-form.php',
                        single: true, // important, as many layouts can occur
                        layout:'fit',
                        
                        closable: true,
                        defaults:{
                                layout:'form',
                                labelWidth:100,

                                // as we use deferredRender:false we mustn't
                                // render tabs into display:none containers
                                hideMode:'offsets',                                
                                deferredRender: false
                        }                        
                });
                Ext.getCmp('contentPanel').enable()
                tabPanel.add(newTab);
                tabPanel.activate(newTab); 

	});
      
	var viewport = new Ext.Viewport({
		layout: 'border',
		renderTo: Ext.getBody(),
		items: [{
				region: 'north',
				baseCls : 'x-plain',
				height: 65, // give north and south regions a height
				margins: '-10 5 0 0',
				contentEl:'header'
			},
			federationPanel,
			tabPanel
		]
	});

});