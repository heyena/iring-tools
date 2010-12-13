Ext.onReady(function () {

	Ext.BLANK_IMAGE_URL = 'resources/images/s.gif';
	Ext.QuickTips.init();

	Ext.state.Manager.setProvider(new Ext.state.CookieProvider());

	var federationPanel = new FederationManager.FederationPanel({
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

	var contentPanel = new Ext.TabPanel({
		region: 'center',
		id: 'contentPanel',
		xtype: 'tabpanel',
		margins: '0 5 0 0',
		enableTabScroll: true,
                single: true, // important, as many layouts can occur
                defaults:{
                        //layout:'form',
                        labelWidth:100

                        // as we use deferredRender:false we mustn't
                        // render tabs into display:none containers
                        ,hideMode:'offsets'
                }
	});

	federationPanel.on('open', function(panel, node, label, formData) {

                var newTab = new FederationManager.ContentPanel({
                        title: label,
                        configData: formData,
                        url: 'postFederation',
                        closable: true
                         //deferredRender: false
                        //layout:'form'
                        
                });

                contentPanel.add(newTab);
                contentPanel.activate(newTab);
			

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
			contentPanel
		]
	});

});