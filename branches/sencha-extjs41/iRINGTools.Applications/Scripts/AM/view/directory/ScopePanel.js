Ext.define('AM.view.directory.ScopePanel', {
  extend: 'Ext.window.Window',
  alias: 'widget.scopeform',
  layout: 'fit',
  border: false,
  frame: false,
  from: null,
  split: true,
  record: null,
  url: null,
  height: 240,
  width: 460,
  closable: true,
  bodyPadding: 1,
  autoload: true,
  path: null,
  state: null,
  node: null,

  initComponent: function () {
    var me = this;
    this.addEvents({
      close: true,
      save: true,
      reset: true,
      validate: true,
      tabChange: true,
      refresh: true,
      selectionchange: true
    });

    this.bbar = new Ext.toolbar.Toolbar();
    this.bbar.add(this.buildToolbar());

    var name = '';
    var path = '';
    var description = '';

    if (this.node.parentNode)
      path = this.path;

    var state = this.state;
    var context = this.record.context;

    if (this.state == 'edit' && this.record != null) {
      name = this.record.Name;
      description = this.record.Description;
    }

    var contextCombo = Ext.create('Ext.form.ComboBox', {
      loadMask: false,
      fieldLabel: 'Context name',
      name: 'contextCombo',      
      editable: true,
      triggerAction: 'all',
      forceSelection: false,
      typeAhead: false,
      selectOnFocus: false,
      minChars: 100000,
      store: Ext.create('Ext.data.Store', {
        model: 'AM.model.ContextModel',
        autoLoad: false,
        clearOnLoad: true,
        listeners: {
          load: function () {
            if (context == '') {
              if (contextCombo.store)
                if (contextCombo.store.data.items[0])
                  context = contextCombo.store.data.items[0].data.context;
            }

            if (context != '' && context != undefined && contextCombo.store.data.length == 1)
              contextCombo.setValue(context);

            if (contextCombo.store.data.length == 1)
              me.record.Context = context;
          }
        }
      }),
      displayField: 'context',
      valueField: 'context',
      hiddenName: 'Context',
      value: context,
      allowBlank: true,
      listeners: {
        'select': function (combo, rec, index) {
          if (rec != null && me.record != null) {
            me.record.Context = rec[0].data.context;
          }
        },
        change: function (combo, newValue, oldValue) {
          if (newValue != undefined && newValue != '' && me.record != null) {
            me.record.Context = newValue;
          }
        }
      }
    });

    this.items = [{
      xtype: 'form',
      labelWidth: 100,
      url: this.url,
      method: 'POST',
      bodyStyle: 'padding:10px 5px 0',
      border: false,
      frame: false,
      defaults: {
        anchor: '100%',
        msgTarget: 'side'
      },
      defaultType: 'textfield',
      items: [
              { name: 'path', xtype: 'hidden', value: path, allowBlank: false },
              { name: 'state', xtype: 'hidden', value: state, allowBlank: false },
              { name: 'oldContext', xtype: 'hidden', value: context, allowBlank: false },
              { fieldLabel: 'Folder name', name: 'folderName', xtype: 'textfield', value: name, allowBlank: false },
              { name: 'contextName', xtype: 'hidden', value: me.record.Context, allowBlank: true },
              contextCombo,
              { fieldLabel: 'Description', name: 'Description', allowBlank: true, xtype: 'textarea', value: description }
           ]
    }];
    /*
     
    */


    // super
    this.callParent(arguments);
  },

  buildToolbar: function () {
    return [{
      xtype: 'tbfill'
    }, {
      xtype: "button",
      text: 'Ok',
      disabled: false,
      handler: this.onSave,
      scope: this
    }, {
      xtype: "button",
      text: 'Cancel',
      //icon: 'Content/img/16x16/edit-clear.png',      
      disabled: false,
      handler: this.onReset,
      scope: this
    }]
  },

  onReset: function () {
    this.items.first().getForm().reset();
    this.fireEvent('Cancel', this);
  },

  onSave: function () {
    var me = this;

    var thisForm = me.items.items[0].getForm();
    var folderName = thisForm.findField('folderName').getValue();
    var contextNameField = thisForm.findField('contextName');

    if (me.record.Context != undefined)
      contextNameField.setValue(me.record.Context);
    else {
      var context = thisForm.findField('contextCombo').getValue();
      contextNameField.setValue(context);
    }


    if (ifExistSibling(folderName, this.node, this.state)) {
      showDialog(400, 100, 'Warning', 'The name \"' + folderName + '\" already exits in this level, please choose a different name.', Ext.Msg.OK, null);
      return;
    }

    this.items.first().getForm().submit({
      waitMsg: 'Saving Data...',
      success: function (response, request) {
        me.fireEvent('Save', me);
      },
      failure: function (response, request) {
        if (response.items[3].value != undefined) {
          var rtext = response.items[3].value;
          showDialog(400, 100, 'Error saving folder changes', 'Changes of ' + rtext + ' are not saved.', Ext.Msg.OK, null);
          return;
        }
        var message = 'Error saving changes!';
        showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
      }
    });
  }
});



