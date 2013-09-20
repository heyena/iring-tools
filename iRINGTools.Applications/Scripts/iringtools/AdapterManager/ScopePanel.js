﻿Ext.ns('AdapterManager');

AdapterManager.ScopePanel = Ext.extend(Ext.Panel, {
  border: false,
  frame: false,
  split: true,
  from: null,
  record: null,
  url: null,
  bodyStyle: 'padding:10px',

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
      selectionchange: true
    });

    this.bbar = this.buildToolbar();

    var name = "";
    var displayName = "";
    var description = "";
    var cacheDBConnStr = "";

    if (this.record != null) {
      name = this.record.Name;
      displayName = this.record.DisplayName;
      description = this.record.Description;
      if (this.record.Configuration != null && this.record.Configuration.AppSettings != null && this.record.Configuration.AppSettings.Settings !=null) {
          Ext.each(this.record.Configuration.AppSettings.Settings, function (settings, index) {
              if (settings.Key == "iRINGCacheConnStr")
                  cacheDBConnStr = settings.Value;
          });
      }
    }

    this.form = new Ext.FormPanel({
      labelWidth: 105,
      url: this.url,
      method: 'POST',
      frame: false,
      border: false,
      autoDestroy: false,
      bodyStyle: 'padding:10px',
      defaults: {
        width: 290,
        msgTarget: 'side'
      },
      defaultType: 'textfield',
      items: [
        { name: 'name', xtype: 'hidden', value: name, allowBlank: false },
        { fieldLabel: 'Name', name: 'displayName', xtype: 'textfield', value: displayName, allowBlank: false },
        { fieldLabel: 'Description', name: 'description', allowBlank: true, xtype: 'textarea', value: description },
        { fieldLabel: 'CacheDB ConnStr', name: 'cacheDBConnStr', xtype: 'textfield', value: cacheDBConnStr }
      ]
    });

    this.items = [
  		this.form
		];

    AdapterManager.ScopePanel.superclass.initComponent.call(this);
  },

  buildToolbar: function () {
    return [{
      xtype: 'tbfill',
      height: 24
    }, {
      xtype: "tbbutton",
      text: 'Ok',
      //icon: 'Content/img/16x16/document-save.png',      
      disabled: false,
      handler: this.onSave,
      scope: this
    }, {
      xtype: "tbbutton",
      text: 'Cancel',
      //icon: 'Content/img/16x16/edit-clear.png',      
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
    var that = this;    // consists the main/previous class object

    var returnVal = this.checkidNodeExists()

    if (returnVal == true) {
      this.form.getForm().submit({
        waitMsg: 'Saving Data...',
        success: function (f, a) {
          //Ext.Msg.alert('Success', 'Changes saved successfully!');
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
      var message = 'Scope/Application name already exists!';
      showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
    }
  },

  checkidNodeExists: function () {
    var returnVal = true;
    for (var i = 0; i < Ext.getCmp('Directory-Panel').root.childNodes.length; i++) {
      if (Ext.getCmp('Directory-Panel').root.childNodes[i].text == this.form.getForm().getFieldValues().Name) {
        if (Ext.getCmp('Directory-Panel').root.childNodes[i].attributes.record.Description == this.form.getForm().getFieldValues().Description) {
          returnVal = false;
        }
      }
    }
    return returnVal;
  }
});


