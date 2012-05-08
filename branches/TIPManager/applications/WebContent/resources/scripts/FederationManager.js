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
var oldx=270;
var oldy=105;

function showDialog(width, height, title, message, buttons, callback) {
	  if (message.indexOf('\\r\\n') != -1)
	    var msg = message.replace('\\r\\n', '\r\n');
	  else
	    var msg = message;

	  if (msg.indexOf("\\") != -1)
	    var msgg = msg.replace(/\\\\/g, "\\");
	  else
	    var msgg = msg;
	  
		var style = 'style="margin:0;padding:0;width:' + width + 'px;height:' + height + 'px;border:1px solid #aaa;overflow:auto"';
		Ext.Msg.show({
			title: title,
			msg: '<textarea ' + style + ' readonly="yes">' + msgg + '</textarea>',
			buttons: buttons,
			fn: callback
		});
}
		
Ext.onReady(function () {

	Ext.BLANK_IMAGE_URL = 'resources/images/s.gif';
	Ext.QuickTips.init();
	
	Ext.get('about-link').on('click', function(){
    var win = new Ext.Window({    
      title: 'About Federation Manager',
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
		layout: 'border',
		region:'west',
		title : 'Federation',
		collapsible: true,
		collapsed: false,
		collapseMode:'mini',
		width: 260,
	    minSize: 175,
	    maxSize: 500,
	    border: 1,
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
    var showPopup = false;
    
    if(node.parentNode.text == 'ID Generators'|| node.text == 'ID Generators'){
      tabIconClass = 'tabsIdGen';
      showPopup = true;
    }else if(node.parentNode.text == 'Namespaces'|| node.text == 'Namespaces'){
      tabIconClass = 'tabsNameSpace';
      showPopup = true;
    }else if(node.parentNode.text == 'Repositories' || node.text == 'Repositories'){
      tabIconClass = 'tabsRepository';
    }

    if(showPopup){
    	if(Ext.getCmp('newwin-' + node.id)){
    		
    		Ext.getCmp('newwin-' + node.id).show();
    	}else{
        	Ext.getBody().mask("Loading...", "x-mask-loading");
        	
        	// create a new popupPanel and add to window
            var newWinPanel = new FederationManager.WindowPanel({
            	showPopup: showPopup,
                //id:'tab-' + node.id,
                configData: formData,
                nId:node.id,
                url: 'postFederation',                        
                single: true, // important, as many layouts can occur
                layout:'fit',
                closable: true,
                defaults:{
                  layout:'form',
                  labelWidth:100,
                  hideMode:'offsets',                                
                  deferredRender: false
                }                        
              });
            oldx = oldx + 8;
            oldy = oldy + 22;
            
            var win = new Ext.Window({
                closable: true,
                resizable: true,
                autoScroll: true,
                x:oldx,
                y:oldy,
                id: 'newwin-' + node.id,
                modal: false,
                layout: 'fit',
                //title: 'Add New Application',
                title: label,
                iconCls: tabIconClass,
                height: 291,
                width: 430,
                plain: false,
                items: newWinPanel,
                listeners: {
                  afterlayout: function (pane) {
                    Ext.getBody().unmask();
                  },
                  beforeClose: function(window){
                	  //*****Ext.getCmp('contentPanel').enable();
                	  oldx = oldx - 8; 
                	  oldy = oldy - 22;
                  }
                }
              });
              win.show();
              // Forcibly disable validation 
              newWinPanel.form.getForm().clearInvalid();    		
    	}

          
          //Ext.getCmp('contentPanel').disable();
    }else{
    	//Ext.getBody().mask("Loading...", "x-mask-loading");
	    var newTab = new FederationManager.ContentPanel({
	        //title: label,
	    	showPopup: showPopup,
	        id:'tab-' + node.id,
	        configData: formData,
	        nId:node.id,
	        url: 'postFederation',                        
	        single: true, // important, as many layouts can occur
	        layout:'fit',
	        //iconCls: tabIconClass,
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
	    
	    if(Ext.getCmp('contentPanel').enabled==undefined){
			Ext.getCmp('contentPanel').enable();
		}
		
		newTab.setTitle(label);
		newTab.setIconClass(tabIconClass);
		tabPanel.add(newTab).show();
		
		// Forcibly disable validation 
	    newTab.form.getForm().clearInvalid();
	}

    
});

	searchPanel = new FederationManager.SearchPanel({
    id: 'search-panel',
    title: 'Reference Data Search',
    region: 'south',
    height: 250,
	collapsible:true,
	collapseMode:'mini',
    collapsed: false,
    searchUrl: 'refdata',
    limit: 100
  });

	searchPanel.on('openAddTab', function(panel, tabId, label, formType, formData) {
		var tabIconClass;  
		if(formType == 'template'){
			tabIconClass = 'tabTemplate';
		}else{
			tabIconClass ='tabClass';
		}
	    var classTemplatePanel = new FederationManager.ClassTemplatePanel({
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
	    classTemplatePanel.on('EditTempRole', function(id) {
	    	
	    	var editTemplateRole = new FederationManager.EditTemplateRole({});
	    	
			var win = new Ext.Window({
			      closable: true,
			      modal: false,
			      id: 'editwin-' + id,
			      layout: 'fit',
			      title: 'Role Range/Value and Restrictions',
			      minimizable : false,
			      height: 400,
			      width: 600,
			      resizable: false,
			      plain: true,
			      items: editTemplateRole
			    });

			    win.show();
		  });

	    Ext.getCmp('contentPanel').enable();
	    tabPanel.add(classTemplatePanel).show();

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
			xtype: 'box',
		      region: 'north',
		      applyTo: 'header',
		      border: false,
		      height: 55
		  },
			federationPanel,
			centrePanel
		]
	});

});