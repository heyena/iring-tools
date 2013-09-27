Ext.ns('AdapterManager');

AdapterManager.ScopePanel = Ext.extend(Ext.Window, {
  layout: 'fit',
  title: 'Edit Scope',
  width: 460,
  height: 240,
  iconCls: 'tabsScope',
  closable: true,
  modal: true,
  resizable: false,

  initComponent: function () {
    this.bbar = this.buildToolbar();

    var name = "";
    var displayName = "";
    var description = "";
    var cacheDBConnStr = "Data Source={hostname\\dbInstance};Initial Catalog={dbName};User ID={userId};Password={password}";

    if (this.record != null) {
      name = this.record.Name;
      displayName = this.record.DisplayName;
      description = this.record.Description;

      if (this.record.Configuration != null && this.record.Configuration.AppSettings != null &&
            this.record.Configuration.AppSettings.Settings != null) {
        Ext.each(this.record.Configuration.AppSettings.Settings, function (settings, index) {
          if (settings.Key == "iRINGCacheConnStr")
            cacheDBConnStr = settings.Value;
        });
      }
    }

    this.form = new Ext.FormPanel({
      url: this.url,
      method: 'POST',
      frame: false,
      border: false,
      autoDestroy: false,
      bodyStyle: 'padding:15px',
      labelWidth: 100,
      defaults: {
        anchor: '100%',
        allowBlank: true,
        msgTarget: 'side'
      },
      defaultType: 'textfield',
      items: [
        { name: 'name', xtype: 'hidden', value: name },
        { fieldLabel: 'Name', name: 'displayName', value: displayName, allowBlank: false, msgTarget: 'Required field' },
        { fieldLabel: 'Description', name: 'description', xtype: 'textarea', value: description },
        { fieldLabel: 'Cache ConnStr', name: 'cacheDBConnStr', xtype: 'textarea', value: cacheDBConnStr }
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
      disabled: false,
      handler: this.onSave,
      scope: this
    }, {
      xtype: 'tbspacer',
      width: 5
    }, {
      xtype: "tbbutton",
      text: 'Cancel',
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
    var that = this;
    var returnVal = this.checkidNodeExists()

    if (returnVal == true) {
      this.form.getForm().submit({
        waitMsg: 'Saving Data...',
        success: function (f, a) {
          that.fireEvent('Save', that);
        },
        failure: function (f, a) {
          var message = 'Error saving changes!';
          showDialog(400, 100, 'Error', message, Ext.Msg.OK, null);
        }
      });
    }
    else {
      var message = 'Scope already exists!';
      showDialog(400, 100, 'Error', message, Ext.Msg.OK, null);
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
