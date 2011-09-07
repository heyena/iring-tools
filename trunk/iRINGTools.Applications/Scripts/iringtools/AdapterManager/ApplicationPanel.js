/**
* @class AdapterManager.ApplicationPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
Ext.define('AdapterManager.ApplicationPanel', {
  extend: 'Ext.panel.Panel',
  alias: 'widget.AdapterManager.ApplicationPanel',
  layout: 'fit',
  border: false,
  frame: false,
  split: false,
  store: null,
  proxy: null,
  scope: null,
  record: null,
  form: null,
  url: null,

  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

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

    if (this.scope != null) {
      scope = this.scope.Name;
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
    }
    else {
      showconfigure = true;
    }

    this.proxy = Ext.create('Ext.data.proxy.Ajax', {
      timeout: 120000,
      extraParams: {},
      url: 'directory/datalayers',
      reader: {
        type: 'json',
        root: 'items'
      }
    });

    Ext.define('dlModel', {
      extend: 'Ext.data.Model',
      fields: [
          { name: 'Assembly', type: 'string' },
          { name: 'Name', type: 'string' },
          { name: 'Configurable', type: 'string' }
       ]
    });

    this.store = new Ext.data.Store({
      id: 'dataLayer',
      model: 'dlModel',
      proxy: this.proxy
    });

    var cmbDataLayers = Ext.create('Ext.form.ComboBox', {
      fieldLabel: 'Data Layer',
      //boxMaxWidth: 250,
      width: 250,
      store: this.store,
      displayField: 'Name',
      valueField: 'Assembly',
      hiddenName: 'Assembly',
      value: assembly,
      queryMode: 'local'
    });

    cmbDataLayers.on('select', function (combo, record, index) {
      if (record != null && this.record != null) {
        this.record.DataLayer = record[0].data.name;
        this.record.Assembly = record[0].data.assembly;
      }
    }, this);

    that = this;

    this.form = new Ext.form.Panel({
      labelWidth: 70, // label settings here cascade unless
      url: this.url,
      method: 'POST',
      bodyStyle: 'padding:10px 5px 0',
      layout: 'anchor',
      border: false, // removing the border of the form

      frame: false,
      
      defaults: {
        width: 310,
        msgTarget: 'side'
      },
      defaultType: 'textfield',

      items: [
              { fieldLabel: 'Scope', name: 'Scope', xtype: 'hidden', width: 300, value: scope, allowBlank: false },
              { fieldLabel: 'Application', name: 'Application', xtype: 'hidden', width: 300, value: name, allowBlank: false },
              { fieldLabel: 'Name', name: 'Name', xtype: 'textfield', width: 300, value: name, allowBlank: false },
              { fieldLabel: 'Description', name: 'Description', allowBlank: true, xtype: 'textarea', width: 300, value: description },
              cmbDataLayers
          ],
      buttonAlign: 'left', // buttons aligned to the left            
      autoDestroy: false
    });

    this.items = [
  		    this.form
  	    ];



    this.bbar = this.buildToolbar(showconfigure);

    // super
    this.callParent(arguments);

    this.store.load();
    

  },

  buildToolbar: function (showconfigure) {
    return [{
      xtype: 'tbfill'
    }, {
      xtype: "button",
      text: 'Ok',
      //icon: 'Content/img/16x16/document-save.png',
      //tooltip: 'Save',
      disabled: false,
      handler: this.onSave,
      scope: this
    }, {
      xtype: "button",
      text: 'Canel',
      //icon: 'Content/img/16x16/edit-clear.png',
      //tooltip: 'Clear',
      disabled: false,
      handler: this.onReset,
      scope: this
    }]
  },

  onReset: function () {
    this.form.getForm().reset();
    this.fireEvent('Cancel', this);
  },

  onSave: function () {
    var that = this;    // consists the main/prappNameclass object
    if (this.form.getForm().getFieldValues().Scope != this.form.getForm().getFieldValues().Name) {
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
