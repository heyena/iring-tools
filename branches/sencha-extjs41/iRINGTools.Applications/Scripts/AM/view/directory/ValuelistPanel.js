Ext.define('AM.view.directory.ValuelistPanel', {
  extend: 'Ext.window.Window',
  alias: 'widget.valuelistpanel',
  layout: 'fit',
  height: 110,
  width: 430,
  border: false,
  frame: false,
  split: true,
  record: null,
  url: null,
  nodeId: null,
  closable: true,
  iconCls: 'tabsValueList',
  node: null,
  state: null,

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

    this.bbar = new Ext.toolbar.Toolbar();
    this.bbar.add(this.buildToolbar());

    var name = "";
    var nodeId = "";
    var contextName = '';
    var endpoint = '';
    var oldValueListName = '';
    var baseUrl = '';

    if (this.node != null) {
      nodeId = this.node.id;
      contextName = this.node.data.property.context;
      endpoint = this.node.data.property.endpoint;
      baseUrl = this.node.data.property.baseUrl;
    }

    if (this.state == 'edit' && this.record != null) {
      name = this.record.record.name;
      oldValueListName = name;
      this.node = this.node.parentNode;
    }

    this.items = [{
      xtype: 'form',
      labelWidth: 100,
      //      layout: 'fit',
      url: 'mapping/valueList',
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
         { name: 'oldValueList', xtype: 'hidden', value: oldValueListName, allowBlank: false },
         { name: 'contextName', xtype: 'hidden', value: contextName, allowBlank: false },
         { name: 'endpoint', xtype: 'hidden', value: endpoint, allowBlank: false },
         { name: 'baseUrl', xtype: 'hidden', value: baseUrl, allowBlank: false },
         { fieldLabel: 'Value List Name', name: 'valueList', xtype: 'textfield', value: name, allowBlank: false }
      ],
      autoDestroy: false
    }];
    // super
    this.callParent(arguments);
  },

  buildToolbar: function () {
    return [{
      xtype: 'tbfill'
    }, {
      xtype: "button",
      text: 'Ok',
      //icon: 'Content/img/16x16/document-save.png',      
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
    var me = this;    // consists the main/previous class object
    var thisForm = this.items.first().getForm();
    if (thisForm.findField('valueList').getValue() == '') {
      showDialog(400, 100, 'Warning', 'Please type in a value list name before saving.', Ext.Msg.OK, null);
      return;
    }
    thisForm.submit({
      waitMsg: 'Saving Data...',
      success: function (f, a) {
        //Ext.Msg.alert('Success', 'Changes saved successfully!');
        me.fireEvent('Save', me);
      },
      failure: function (f, a) {
        //Ext.Msg.alert('Warning', 'Error saving changes!')
        var message = 'Error saving changes!';
        showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
      }
    });
  }
});
