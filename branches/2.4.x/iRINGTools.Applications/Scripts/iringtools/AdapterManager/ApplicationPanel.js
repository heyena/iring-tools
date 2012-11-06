Ext.ns('AdapterManager');
/**
* @class AdapterManager.ApplicationPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/

AdapterManager.ApplicationPanel = Ext.extend(Ext.Panel, {
    //layout: 'anchor',
	//height:'100%',
	//autoScroll: true,
	autoHeight:true,
	//collapsible:false,
    border: false,
    frame: false,
    split: false,
    node: null,
    state: null,
	//count:0,
	//countForValue:0,
    scope: null,
    record: null,
    id: null,
    form: null,
    url: null,
    key:"",
	value:"",
    /**
    * initComponent
    * @protected
    */
	 addSettings : function(key, value, nameID, valueID){
				return[ {
							  xtype: 'container',
							  style: 'margin:3 0 0 0;',
							  layout:'column',
							  items: [
							  /*{
							    xtype: 'settingfield',
								text:'Thiis is test label...'
								//anchor:'100%'
							  }*/
						   {
		 						xtype: 'textfield',
								name:nameID,
								value:key,
								//allowBlank: false,
								//id:'name-1',
								columnWidth: 0.33,//0.43,
								//style: 'margin:0 0 0 20;',
								
                           },
						   {
								xtype: 'textfield',
								name:valueID,
								value:value,
								//inputType: this.inpType,//'password',
								//id:'value-1',
								columnWidth: 0.33,//0.24,
								style: 'margin:0 0 0 3;',
								//margin:'0 0 0 3'
                           },
						   {
								xtype: 'checkbox',
								columnWidth: 0.10,//0.12,
								//margin:'0 0 0 15',//'0 0 0 15'
								style: 'margin:0 0 0 8;',
								//action:'checkMe'
								handler: function(checkbox, checked) {
										   //alert('clicked...');
										   if(checked)
											 checkbox.findParentByType('container').items.items[1].el.dom.type = 'password';
										   else
											 checkbox.findParentByType('container').items.items[1].el.dom.type =  'text';
											 
				        	    }
                           },
							 {
								xtype: 'button',
								//flex: 1,
								text: 'Add',
								//action:'AddMe',
								//icon: '../ux/css/images/right2.gif',//'add-button',
								//margin:'0 0 0 3',
								columnWidth: 0.12,
								style: 'margin:0 0 0 2;',
								//style: 'float: right;',
								tooltip: 'Click to Add settings',
								handler : function (){
								         
										 var counter = parseInt(this.findParentByType('fieldset').items.items[this.findParentByType('fieldset').items.length-1].items.items[0].name.substring(3,this.findParentByType('fieldset').items.items[this.findParentByType('fieldset').items.length-1].items.items[0].name.length));
								         if(this.findParentByType('fieldset').items.items[this.findParentByType('fieldset').items.length-1].items.items[0].name.substring(3,this.findParentByType('fieldset').items.items[this.findParentByType('fieldset').items.length-1].items.items[0].name.length) != ""){
										    var nameID = 'key'+(counter+1);
										    var valueID = 'value'+(counter+1);
										 }else{
										     var nameID = 'key1';
										     var valueID = 'value1';
										 }
										 
										 //var nameID ='key'+myThis.count;
										 //var valueID = 'value'+myThis.countForValue;
										 var newPanel = new AdapterManager.ApplicationPanel();
										 var abc =  newPanel.addSettings("","" , nameID, valueID);
										 this.findParentByType('fieldset').add(abc);
										 this.findParentByType('fieldset').doLayout();
								}
							},
							{
								xtype: 'button',
								//flex: 1,
								text: 'Delete',
								//margin:'0 0 0 3',
								//action:'DeleteMe',
								//icon :'../../ux/css/images/right2.gif',//'remove-button',
								columnWidth: 0.10,
								style: 'margin:0 0 0 5;',
								style: 'float: right;',
								tooltip: 'Click to Delete settings',
								handler : function (){
										 this.findParentByType('container').destroy();
										 //myThis.findParentByType('window').doLayout();
										 //this.findParentByType('fieldset').doLayout();
								         //var abc = addSettings();
										 //Ext.getCmp('settingsContainer').add(abc);
										 //console.log(abc);
										 
								}
						   }
							     
							  ]
						}
	                         
	   ]
	},
	listeners:{
	  afterrender: function( me, eOpts){
	  if (this.record != null) {
	     if(this.record.Configuration!=null){
	       if (this.record.Configuration.AppSettings != null) {
		      if(this.record.Configuration.AppSettings.Settings!=null){
			        for(var i=1;i<this.record.Configuration.AppSettings.Settings.length;i++){
						  this.key = this.record.Configuration.AppSettings.Settings[i].Key;
						  this.value = this.record.Configuration.AppSettings.Settings[i].Value;
						  var newSetting = this.addSettings(this.key,this.value, ('key'+i), ('value'+i));
						  me.findById('settingfieldset').add(newSetting);
						 //this.findParentByType('fieldset').add(newSetting);
						 //this.findParentByType('fieldset').doLayout();
			  }
			}
		 }
	  }
	    
	  }
	  
	  }
	},
    initComponent: function () {
        myThis = this;
        this.addEvents({
            close: true,
            save: true,
            reset: true,
            validate: true,
            tabChange: true,
            refresh: true,
            selectionchange: true,
            configure: true
        });

        var scope = "";
        var showconfigure = "";
        var id = "";

        if (this.scope != null) {
            scope = this.scope.Name;
        }

        if (this.id != null) {
            id = this.id;
        }

			
        var name = "";
        var description = "";
        var dataLayer = "";
        var assembly = "";
        if (this.record != null) {
            name = this.record.Name;
            description = this.record.Description;
            dataLayer = this.record.DataLayer;
            assembly = this.record.Assembly;
            showconfigure = false;
			if (this.record != null){
			  if(this.record.Configuration!=null){
			     if(this.record.Configuration.AppSettings!=null){
				    if(this.record.Configuration.AppSettings.Settings!=null){
				        this.key = this.record.Configuration.AppSettings.Settings[0].Key;
			            this.value = this.record.Configuration.AppSettings.Settings[0].Value;
				  } 
			    }  
			  }
		    }
			
			
        }
        else {
            showconfigure = true;
        }

        var dataLayersStore = new Ext.data.JsonStore({
            // store configs            
            autoDestroy: true,
            url: 'directory/dataLayers',
            // reader configs
            root: 'items',
            idProperty: 'assembly',
            fields: [
                { name: 'assembly', mapping: 'Assembly', allowBlank: false },
                { name: 'name', mapping: 'Name', allowBlank: false },
                { name: 'configurable', mapping: 'Configurable', allowBlank: false }
            ]
        });

        var panel = Ext.getCmp(id);
        dataLayersStore.on('beforeload', function (store, options) {
            panel.body.mask('Loading...', 'x-mask-loading');
        }, this);

        dataLayersStore.on('load', function (store, records, options) {
            panel.body.unmask();
        }, this);

        var cmbDataLayers = new Ext.form.ComboBox({
            fieldLabel: 'Data Layer',
            boxMaxWidth: 240,//250,
            width: 240,//250,
            forceSelection: true,
            typeAhead: true,
            triggerAction: 'all',
            lazyRender: true,
            //mode: 'remote',
            store: dataLayersStore,
            displayField: 'name',
            valueField: 'assembly',
            hiddenName: 'Assembly',
            value: assembly
        });

        cmbDataLayers.on('select', function (combo, record, index) {
            if (record != null && this.record != null) {
                this.record.DataLayer = record.data.name;
                this.record.Assembly = record.data.assembly;
            }
        }, this);

        //that = this;
 
        this.form = new Ext.FormPanel({
            labelWidth: 70, // label settings here cascade unless
            url: this.url,
            method: 'POST',
            bodyStyle: 'padding:10px 5px 0',

            border: false, // removing the border of the form
          
            frame: false,
            closable: true,
            defaults: {
                width: 310,
                msgTarget: 'side'
            },
            defaultType: 'textfield',

            items: [
          { fieldLabel: 'Scope', name: 'Scope', xtype: 'hidden', width: 300, value: scope, allowBlank: false },
          { fieldLabel: 'Application', name: 'Application', xtype: 'hidden', width: 300, value: name, allowBlank: false },
          { fieldLabel: 'Name', name: 'Name', xtype: 'textfield', width: 300, value: name, allowBlank: false },
          { fieldLabel: 'Description', name: 'Description', allowBlank: true, xtype: 'textarea', width: 300, height: 150, value: description },
          cmbDataLayers,
          {
          	    xtype: 'container',
          	    layout: {
          	        type: 'column'
          	    },
          	    items: [
									 {
									     xtype: 'label',
									     text: 'Settings:',
									     //title: 'Column 1',
									     columnWidth: 0.34
									 },
									{
									    xtype: 'label',
									    text: 'Name',
									    //title: 'Column 2',
									    style: 'font-weight:bold;',
									    columnWidth: 0.27
									},
									{
									    xtype: 'label',
									    text: 'Value',
									    //title: 'Column 3',
									    style: 'font-weight:bold;',
									    title: 'Column 1',
									    columnWidth: 0.22//0.18
									},
									{
									    xtype: 'label',
									    text: 'Encrypt',
									    //title: 'Column 4',
									    style: 'font-weight:bold;',
									    columnWidth: 0.17//0.15
									}

						  ]
          	},
			{
						xtype: 'fieldset',
						border:false,
						collapsible:false,
						autoHeight:true,
						id:'settingfieldset',
						style: 'margin:0 0 0 64;',
						//layout: {
						//	type: 'vbox'
						//},
						items: [
						
						{
							  xtype: 'container',
							  layout:'column',
							  items: [
							 
						   {
		 						xtype: 'textfield',
								//id:'name-1',
								name:'Key',
								value:this.key,
								//allowBlank: false,
								columnWidth: 0.33,//0.43,
								//style: 'margin:0 0 0 20;',
								
                           },
						   {
								xtype: 'textfield',
								//inputType: this.inpType,//'password',
								//id:'value-1',
								name:'Value',
								value:this.value,
								columnWidth: 0.33,//0.24,
								style: 'margin:0 0 0 3;',
								//margin:'0 0 0 3'
                           },
						   {
								xtype: 'checkbox',
								columnWidth: 0.10,//0.12,
								//margin:'0 0 0 15',//'0 0 0 15'
								style: 'margin:0 0 0 8;',
								//action:'checkMe',
								handler: function(checkbox, checked) {
										   //alert('clicked...');
										   if(checked)
											 checkbox.findParentByType('container').items.items[1].el.dom.type = 'password';
										   else
											 checkbox.findParentByType('container').items.items[1].el.dom.type =  'text';
											 
				        	    }
				
                          },
							 {
								xtype: 'button',
								//flex: 1,
								text: 'Add',
								//action:'AddMe',
								//icon: '../ux/css/images/right2.gif',//'add-button',
								//margin:'0 0 0 3',
								columnWidth: 0.12,
								style: 'margin:0 0 0 2;',
								//style: 'float: right;',
								tooltip: 'Click to Add settings',
								handler : function (){
								         var counter = parseInt(this.findParentByType('fieldset').items.items[this.findParentByType('fieldset').items.length-1].items.items[0].name.substring(3,this.findParentByType('fieldset').items.items[this.findParentByType('fieldset').items.length-1].items.items[0].name.length));
								         if(this.findParentByType('fieldset').items.items[this.findParentByType('fieldset').items.length-1].items.items[0].name.substring(3,this.findParentByType('fieldset').items.items[this.findParentByType('fieldset').items.length-1].items.items[0].name.length) != ""){
										    var nameID = 'key'+(counter+1);
										    var valueID = 'value'+(counter+1);
										 }else{
										     var nameID = 'key1';
										     var valueID = 'value1';
										 }
								         var abc = myThis.addSettings("", "", nameID, valueID);
										 this.findParentByType('fieldset').add(abc);
										 this.findParentByType('fieldset').doLayout();
										 //Ext.getCmp('settingfieldset').add(abc);
										 //Ext.getCmp('settingfieldset').doLayout();
										 
								}
							},
							{
								xtype: 'button',
								//flex: 1,
								text: 'Delete',
								//margin:'0 0 0 3',
								//action:'DeleteMe',
								//icon :'../../ux/css/images/right2.gif',//'remove-button',
								columnWidth: 0.10,
								style: 'margin:0 0 0 5;',
								style: 'float: right;',
								tooltip: 'Click to Delete settings',
								handler : function (){
										 this.findParentByType('container').destroy();
										 Ext.getCmp('settingfieldset').doLayout();
										 var wind = myThis.findParentByType('window');
										 /*wind.on('beforeResize', function ()
										 {
											alert('beforeResize...');
											//win.destroy();
											//win.close();
										 }, this);*/
								}
						   }
							     
							  ]
						}
						
						
							
				]
			}
      ],
            buttonAlign: 'left', // buttons aligned to the left            
            autoDestroy: false
        });

        this.items = [
  		    this.form
  	    ];

        this.bbar = this.buildToolbar(showconfigure);

        // super
        AdapterManager.ApplicationPanel.superclass.initComponent.call(this);

        //var data = dataLayersStore.getById(dataLayer);
        //cmbDataLayers.Value = data;

    },

    buildToolbar: function (showconfigure) {
        return [
					{
						xtype: 'tbfill'
					}, 
					{
						xtype: "tbbutton",
						text: 'Ok',
						//icon: 'Content/img/16x16/document-save.png',
						//tooltip: 'Save',
						disabled: false,
						handler: this.onSave,
						scope: this
					 }, 
					 {
						xtype: "tbbutton",
						text: 'Cancel',
						//icon: 'Content/img/16x16/edit-clear.png',
						//tooltip: 'Clear',
						disabled: false,
						handler: this.onReset,
						scope: this
					}
		]
    },

    onReset: function () {
        this.form.getForm().reset();
        this.fireEvent('Cancel', this);
    },

    onSave: function () {
      var that = this;    // consists the main/prappNameclass object  
      var endpointName = that.items.first().getForm().findField('Name').getValue();

      if (this.form.getForm().getFieldValues().Scope != this.form.getForm().getFieldValues().Name) {
        if (ifExistSibling(endpointName, that.node, that.state)) {
          showDialog(400, 100, 'Warning', 'The name \"' + endpointName + '\" already exits in this level, please choose a different name.', Ext.Msg.OK, null);
          return;
        }

        this.form.getForm().submit({
            waitMsg: 'Saving Data...',
            success: function (f, a) {
                that.fireEvent('Save', that);
            },
            failure: function (f, a) {
                //Ext.Msg.alert('Warning', 'Error saving changes!')
                var message = 'Error saving changes!';
                showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
            }
        });
      }
      else {
          var message = 'Scope & Application name cannot be same!';
          showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
      }
    }

});

var ifExistSibling = function (str, node, state) {
  var ifExist = false;
  if (node.childNodes) {
    var childNodes = node.childNodes;
    var repeatTime = 0;

    for (var i = 0; i < childNodes.length; i++) {
      if (childNodes[i].attributes.text.toLowerCase() == str.toLowerCase()) {
        if (state == 'new')
          ifExist = true;
        else {
          repeatTime++;
          if (repeatTime > 1) {
            ifExist = true;
            return ifExist;
          }
        }
      }
    }
  }

  return ifExist;
};


