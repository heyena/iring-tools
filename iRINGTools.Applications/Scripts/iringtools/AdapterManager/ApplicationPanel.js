Ext.ns('AdapterManager');

/**
* @class AdapterManager.ApplicationPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/

AdapterManager.ApplicationPanel = Ext.extend(Ext.Panel, {
  height: 360,
  border: false,
  frame: false,
  split: false,
  node: null,
  state: null,
  scope: null,
  record: null,
  id: null,
  form: null,
  url: null,
  key: "",
  value: "",

  /**
  * initComponent
  * @protected
  */
  addSettings: function (key, value, nameID, valueID) {
    return [{
      xtype: 'container',
      style: 'margin:10 20 0 95;',
      //bodyStyle: 'padding:10px 20px 0 70px',
      layout: 'column',
      items: [
				{
				  xtype: 'textfield',
				  name: nameID,
				  value: key,
				  columnWidth: '0.30',
				  //width:164,
				  allowBlank: true
				},
			{
			  xtype: 'textarea',
			  name: valueID,
			  value: value,
			  columnWidth: '0.60',
			  grow: false,
			  //width:270,
			  height: 40,
			  style: 'margin:0 0 0 3;'
			  //margin:'0 0 0 3'
			},
			{
			  xtype: 'button',
			  //flex: 1,
			  text: 'Delete',
			  columnWidth: '0.10',
			  style: 'margin:0 0 0 3;',
			  //width:48,
			  //margin:'0 0 0 3',
			  //action:'DeleteMe',
			  //icon :'../../ux/css/images/right2.gif',//'remove-button',
			  //columnWidth: 0.10,
			  //style: 'margin:0 0 0 49;',
			  //style: 'float: right;',
			  tooltip: 'Click to Delete settings',
			  handler: function () {
			    this.findParentByType('container').destroy();

			  }
			}]
    }]
  },
  listeners: {
    afterrender: function (me, eOpts) {
      if (this.record != null) {
        if (this.record.Configuration != null) {
          if (this.record.Configuration.AppSettings != null) {
            if (this.record.Configuration.AppSettings.Settings != null) {
              for (var i = 0; i < this.record.Configuration.AppSettings.Settings.length; i++) {
                this.key = this.record.Configuration.AppSettings.Settings[i].Key;
                this.value = this.record.Configuration.AppSettings.Settings[i].Value;
                var newSetting = this.addSettings(this.key, this.value, ('key' + i), ('value' + i));
                newSetting[0].items[0].allowBlank = false;
                me.findById('settingfieldset').add(newSetting);
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
    var displayName = "";
    var description = "";
    var dataLayer = "";
    var assembly = "";
    var cacheImportURI = "";
    var cacheTimeout = "";

    if (this.record != null) {
      name = this.record.Name;
      displayName = this.record.DisplayName;
      description = this.record.Description;
      dataLayer = this.record.DataLayer;
      assembly = this.record.Assembly;
      showconfigure = false;
      cacheImportURI = this.record.CacheImportURI;
      cacheTimeout = this.record.CacheTimeout;
    }
    else {
      showconfigure = true;
    }

    var dataLayersStore = new Ext.data.JsonStore({
      // store configs            
      autoDestroy: true,
      autoLoad: true,
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
      //alert('hi...');
      panel.body.unmask();
    }, this);

    var cmbDataLayers = new Ext.form.ComboBox({
      fieldLabel: 'Data Layer',
      //bodyStyle: 'width:500px',
      boxMaxWidth: 531, //250,
      width: 531, //250,
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

    this.form = new Ext.FormPanel({
      labelWidth: 100, // label settings here cascade unless
      url: this.url,
      method: 'POST',
      bodyStyle: 'padding:10px 5px 0',
      border: false, // removing the border of the form
      frame: false,
      closable: true,
      defaults: {
        //width: 310,
        //msgTarget: 'side'
      },
      defaultType: 'textfield',
      layout: 'form',
      items: [
					  { fieldLabel: 'Scope', name: 'Scope', xtype: 'hidden', width: 530, value: scope, allowBlank: false },
					  { name: 'Application', xtype: 'hidden', width: 530, value: name, allowBlank: false },
					  { fieldLabel: 'Name', name: 'Name', xtype: 'textfield', width: 530, value: displayName, allowBlank: false },
					  { fieldLabel: 'Description', name: 'Description', allowBlank: true, xtype: 'textarea', width: 530, height: 62, value: description },
					  cmbDataLayers,
                      { fieldLabel: 'Cache ImportURI', name: 'cacheImportURI', xtype: 'textfield', width: 530, value: cacheImportURI, allowBlank: false },
					  { fieldLabel: 'Cache Timeout', name: 'cacheTimeout', xtype: 'textfield', width: 530, value: cacheTimeout, allowBlank: false },
					  {
					    xtype: 'container',
					    layout: {
					      type: 'hbox'
					    },
					    items: [
							  {
								  xtype: 'label',
								  text: 'Settings:',
								  style: 'font:normal 12px tahoma, arial, helvetica, sans-serif;'
							  },
							  {
								  xtype: 'label',
								  text: 'Name',
								  style: 'margin:0 0 0 90;font:normal 12px tahoma, arial, helvetica, sans-serif;'
							  },
							  {
								  xtype: 'label',
								  text: 'Value',
								  style: 'margin:0 0 0 250;font:normal 12px tahoma, arial, helvetica, sans-serif;'
							  },
							  {
								  xtype: 'button',
								  text: 'Add',
								  width: 50,
								  style: 'margin:0 0 0 446;',
								  tooltip: 'Click to Add settings',
								  handler: function () {

									  var nameID;
									  var valueID;
									  var myFieldSet = Ext.getCmp('settingfieldset');
									  if (myFieldSet.items.items.length >= 1) {
										  var nameID = 'key' + (myFieldSet.items.items.length + 1);
										  var valueID = 'value' + (myFieldSet.items.items.length + 1);
									  } else {
										  var nameID = 'key1';
										  var valueID = 'value1';
									  }
									  var abc = myThis.addSettings("", "", nameID, valueID);
									  myFieldSet.add(abc);
									  myFieldSet.doLayout();
									  myFieldSet.items.items[myFieldSet.items.length - 1].items.items[0].allowBlank = false;
                  }
						  }]
					  },
						{
						  xtype: 'fieldset',
						  border: false,
						  collapsible: false,
						  id: 'settingfieldset',
						  height: 200,
						  autoScroll: true,
						  items: [
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

  },

  buildToolbar: function (showconfigure) {
    return [
					{
					  xtype: 'tbfill'
					},
					{
					  xtype: "tbbutton",
					  text: 'Ok',
					  disabled: false,
					  handler: this.onSave,
					  scope: this
					},
					 {
					   xtype: "tbbutton",
					   text: 'Cancel',					   
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
    var flag = false;
    
    for (var i = 0; i < that.items.items[0].items.items[8].items.items.length; i++) {
      if (that.items.items[0].items.items[8].items.items[i].items.items[0].name.toLowerCase().substring(0, 3) == 'key') {
        if (that.items.items[0].items.items[8].items.items[i].items.items[0].getValue().trim() == ' ' || that.items.items[0].items.items[8].items.items[i].items.items[0].getValue().trim() == "") {
          flag = true;
        }
      }

    }
    if (flag) {
      showDialog(400, 100, 'Warning', 'Please enter a valid setting name', Ext.Msg.OK, null);
      return;
    }
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
