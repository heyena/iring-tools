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
var federationPanel;
var searchPanel;

Ext.onReady(function () {

	Ext.BLANK_IMAGE_URL = 'resources/images/s.gif';
	Ext.QuickTips.init();
	
	Ext.get('about-link').on('click', function(){
    var win = new Ext.Window({    
      title: 'About Exchange Manager',
      bodyStyle: 'background-color:white;padding:5px',
      width: 700,
      height: 500,
      closable: true,
      resizable: false,
      autoScroll: true,                
      buttons: [{
        text: 'Close',
        handler: function(){
          Ext.getBody().unmask();
          win.close();
        }
      }],
      autoLoad: 'about-federation-manager.jsp',
      listeners: {
        close:{
          fn: function(){
            Ext.getBody().unmask();
          }
        }
      }
    });
    
    Ext.getBody().mask();    
    win.show();
  });
  
	Ext.state.Manager.setProvider(new Ext.state.CookieProvider());

	federationPanel = new FederationManager.FederationPanel({
		id:'federation-panel',
		region:'west',
		
		collapsible: false,
		collapsed: false,
		
		border: false,
		frame: false,
		split: true,
		width: 260,
		  minSize: 175,
		  maxSize: 500,
		url: 'federation'
		});

	var tabPanel = new Ext.TabPanel({
		region: 'center',
		id: 'contentPanel',     // used in FederationManager.js
		//margins: '0 5 0 0',
                //disabled:true,
		border: true,
		enableTabScroll: true                
	});

	tabPanel.on('tabChange', function(tabContainer){
		if(tabContainer.items.length !=0){ // check is there any tab
		  var nodeObj = federationPanel.getNodeBySelectedTab(tabContainer.getActiveTab());
		  if(nodeObj){
	      federationPanel.selectNode(nodeObj);
		  }
		}else{
	    // code for unselect
		}
	});
	federationPanel.on('openAddTab', function(panel, tabId, label, formData) {
		var tabIconClass;  
		if(tabId == 'addTemplate'){
			tabIconClass = 'tabTemplate';
		}else{
			tabIconClass ='tabClass';
		}
	    var newTab = new FederationManager.ClassTemplatePanel({
	    	title: label,
	        id:tabId,
	        configData: formData,
	        single: true, // important, as many layouts can occur
	        layout:'fit',
	        autoScroll: true,
	        iconCls: tabIconClass,
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

	    Ext.getCmp('contentPanel').enable();
	    tabPanel.add(newTab).show();

		});
	federationPanel.on('opentab', function(panel, node, label, formData) {
    var tabIconClass;    
    if(node.parentNode.text == 'ID Generators'|| node.text == 'ID Generators'){
      tabIconClass = 'tabsIdGen';
    }else if(node.parentNode.text == 'Namespaces'|| node.text == 'Namespaces'){
      tabIconClass = 'tabsNameSpace';
    }else if(node.parentNode.text == 'Repositories' || node.text == 'Repositories'){
      tabIconClass = 'tabsRepository';
    }
    var newTab = new FederationManager.ContentPanel({
      title: label,
      id:'tab-' + node.id,
      configData: formData,
      nId:node.id,
      url: 'postFederation',                        
      single: true, // important, as many layouts can occur
      layout:'fit',
      iconCls: tabIconClass,
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

    Ext.getCmp('contentPanel').enable();
    tabPanel.add(newTab).show();

	});

	searchPanel = new FederationManager.SearchPanel({
    id: 'search-panel',
    title: 'Reference Data Search',
    region: 'south',
    height: 300,
    collapseMode: 'mini',
    collapsible: true,
    collapsed: false,
    searchUrl: 'refdata',
    limit: 100
  });

	searchPanel.on('openAddTab', function(panel, tabId, label, formType, formData) {
		var tabIconClass;  
		if(tabId == 'addTemplate'){
			tabIconClass = 'tabTemplate';
		}else{
			tabIconClass ='tabClass';
		}
	    var newTab = new FederationManager.ClassTemplatePanel({
	    	title: label,
	        id:tabId,
	        configData: formType,
	        parentNode:formData,
	        single: true, // important, as many layouts can occur
	        layout:'fit',
	        autoScroll: true,
	        iconCls: tabIconClass,
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

	    Ext.getCmp('contentPanel').enable();
	    tabPanel.add(newTab).show();

		});
	searchPanel.on('onRemove', function(type) {
		alert(type);
	  });
		
    var centrePanel = new Ext.Panel({
        id: 'centre-panel',
        region: 'center',
        layout: 'border',
        collapsible: false,
        closable: true,
        enableTabScroll: true,
        border: true,
        split: true,
        items: [searchPanel, tabPanel]
      });
    
	var viewport = new Ext.Viewport({
		layout: 'border',
		renderTo: Ext.getBody(),
		items: [{
			region: 'north',
			//baseCls : 'x-plain',
			height: 55, // give north and south regions a height
			//margins: '-10 5 0 0',
			border: false,
			contentEl:'header'
		  },
			federationPanel,
			centrePanel
		]
	});

});