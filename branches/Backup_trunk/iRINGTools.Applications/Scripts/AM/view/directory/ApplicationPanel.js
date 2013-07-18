Ext.define('AM.view.directory.ApplicationPanel', {
  extend: 'Ext.window.Window',
  alias: 'widget.applicationform',
  layout: 'fit',
  border: false,
  frame: false,
  from: null,
  record: null,
  node: null,
  state: null,
  height: 291,
  width: 460,
  bodyPadding: 1,
  closable: true,
  autoload: true,

  initComponent: function () {
    var me = this;
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

    var source = "";
    var showconfigure = "";
    var path = this.path;
    var state = this.state;

    var name = "";
    var description = "";
    var dataLayer = "";
    var assembly = '';
    var baseurl = '';
    var context = this.record.context;

    if (this.state == 'edit' && this.record != null) {
      name = this.record.Name;
      description = this.record.Description;
      dataLayer = this.record.DataLayer;
      assembly = this.record.Assembly;
      baseurl = this.record.BaseUrl;
      showconfigure = false;
    }
    else
      showconfigure = true;
    var me = this;
    var cmbDataLayers = Ext.create('Ext.form.ComboBox', {
      fieldLabel: 'Data Layer',
      editable: false,
      triggerAction: 'all',
      store: Ext.create('Ext.data.Store', {
        model: 'AM.model.DataLayerModel',
        listeners: {
          load: function () {
            if (assembly != '') {
              cmbDataLayers.setValue(assembly);
            }
          },
          beforeLoad: function (store, action) {
            var useNodeBaseUrl = false;

            if (me.items.first().items.last().items.items[0] != undefined)
              if (me.items.first().items.last().items.items[0].items.items[0].rawValue != undefined && me.items.first().items.last().items.items[0].items.items[0].rawValue != '')
                store.proxy.extraParams.baseUrl = me.items.first().items.last().items.items[0].items.items[0].rawValue;
              else
                useNodeBaseUrl = true;
            else
              useNodeBaseUrl = true;

            if (baseurl == '' || baseurl == "")
              return;

            if (useNodeBaseUrl)
              store.proxy.extraParams.baseUrl = baseurl;
          }
        }
      }),
      displayField: 'name',
      valueField: 'assembly',
      hiddenName: 'Assembly',
      value: assembly,
      allowBlank: false,
      listeners: {
        'select': function (combo, rec, index) {
          if (rec != null && me.record != null) {
            me.record.DataLayer = rec[0].data.name;
            me.record.Assembly = rec[0].data.assembly;
            //  this.getForm().findField('assembly').setValue(record.data.assmply);
          }
        }
      }
    });

    var availableBaseUris = Ext.create('Ext.form.ComboBox', {
      loadMask: false,
      fieldLabel: 'Base Url',
      editable: true,
      triggerAction: 'all',
      forceSelection: false,
      typeAhead: false,
      selectOnFocus: false,
      minChars: 100000,
      store: Ext.create('Ext.data.Store', {
        model: 'AM.model.BaseUrlModel',
        listeners: {
          load: function () {
            if (baseurl == '') {
              if (availableBaseUris.store)
                baseurl = availableBaseUris.store.data.items[0].data.baseurl;
            }

            if (baseurl != '' && baseurl != undefined && availableBaseUris.store.data.length == 1)
              availableBaseUris.setValue(baseurl);

            if (availableBaseUris.store.data.length == 1)
              me.record.BaseUrl = baseurl;
          }
        }
      }),
      displayField: 'baseurl',
      valueField: 'baseurl',
      editable: true,
      hiddenName: 'Urlocator',
      value: baseurl,
      listeners: {
        'select': function (combo, rec, index) {
          if (rec != null && me.record != null) {
            me.record.BaseUrl = rec[0].data.baseurl;
          }
        }
      }
    });

    this.items = [{
      xtype: 'form',
      labelWidth: 100,
      url: 'directory/endpoint',
      method: 'POST',
      bodyStyle: 'padding:10px 5px 0',
      border: false,
      frame: false,
      defaults: {
        msgTarget: 'side',
        anchor: '100%'
      },
      defaultType: 'textfield',
      items: [{
        name: 'path',
        xtype: 'hidden',
        value: path,
        allowBlank: false
      }, {
        name: 'state',
        xtype: 'hidden',
        value: state,
        allowBlank: false
      }, {
        name: 'contextValue',
        xtype: 'hidden',
        value: context,
        allowBlank: false
      }, {
        name: 'oldBaseUrl',
        xtype: 'hidden',
        value: baseurl,
        allowBlank: false
      }, {
        name: 'oldAssembly',
        xtype: 'hidden',
        value: assembly,
        allowBlank: false
      }, {
        name: 'assembly',
        xtype: 'hidden',
        value: me.record.Assembly,
        allowBlank: false
      }, {
        name: 'baseUrl',
        xtype: 'hidden',
        value: me.record.BaseUrl,
        allowBlank: false
      }, {
        fieldLabel: 'Endpoint name',
        name: 'endpoint',
        xtype: 'textfield',
        value: name,
        allowBlank: false
      }, {
        fieldLabel: 'Description',
        name: 'Description',
        allowBlank: true,
        xtype: 'textarea',
        value: description
      }, {
        fieldLabel: 'Context name',
        name: 'context',
        xtype: 'textfield',
        value: context,
        disabled: true
      }, cmbDataLayers
       , {
         xtype: 'form',
         layout: 'column',
         border: false,
         frame: false,
         defaults: {
           border: false,
           frame: false
         },
         items: [{
           columnWidth: .87,
           layout: 'fit',
           items: availableBaseUris
         }, {
           columnWidth: .13,
           items: [{
             xtype: 'button',
             style: 'float: right;',
             text: 'Test Url',
             tooltip: 'Test the entered Url',
             handler: function () {
               var baseUrl = me.items.first().items.last().items.items[0].items.items[0].rawValue;
               Ext.Ajax.request({
                 url: 'directory/testBaseUrl',
                 timeout: 600000,
                 method: 'POST',
                 params: {
                   baseUrl: baseUrl
                 },
                 success: function (response, request) {
                   if (response.responseText.indexOf('error') == -1)
                     showDialog(400, 100, 'Testing Result', 'The url is valid and the server is connected.', Ext.Msg.OK, null);
                   else
                     showDialog(400, 100, 'Testing Result', 'Connection failed. Please enter/select a valid url.', Ext.Msg.OK, null);
                 },
                 failure: function (response, request) {
                   showDialog(400, 100, 'Testing Result', 'Connection failed. Please enter/select a valid url.', Ext.Msg.OK, null);
                 }
               });
             }
           }]
         }]
       }]
    }];

    this.bbar = this.buildToolbar(showconfigure);
    // super
    this.callParent(arguments);
    this.items.first().items.items[this.items.first().items.length - 2].store.load();
    this.items.first().items.last().items.items[0].items.items[0].store.load();
  },

  buildToolbar: function (showconfigure) {
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
      text: 'Canel',
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
    var thisForm = this.items.first().getForm();
    var endpointName = thisForm.findField('endpoint').getValue();

    thisForm.findField('assembly').setValue(this.record.Assembly);
    var baseUrl;

    if (me.items.first().items.last().items.items[0].items.items[0].rawValue != '' && me.items.first().items.last().items.items[0].items.items[0].rawValue != undefined) {
      thisForm.findField('baseUrl').setValue(me.items.first().items.last().items.items[0].items.items[0].rawValue);
      baseUrl = me.items.first().items.last().items.items[0].items.items[0].rawValue;
    }
    else {
      thisForm.findField('baseUrl').setValue(this.record.BaseUrl);
      baseUrl = this.record.BaseUrl;
    }

    Ext.Ajax.request({
      url: 'directory/testBaseUrl',
      timeout: 600000,
      method: 'POST',
      params: {
        baseUrl: baseUrl
      },
      success: function (response, request) {
        if (response.responseText.indexOf('error') > -1) {
          showDialog(400, 100, 'Testing Result', 'Connection failed. Please enter/select a valid base url.', Ext.Msg.OK, null);
          return;
        }
        else {
          if (ifExistSibling(endpointName, me.node, me.state)) {
            showDialog(400, 100, 'Warning', 'The name \"' + endpointName + '\" already exits in this level, please choose a different name.', Ext.Msg.OK, null);
            return;
          }

          thisForm.submit({
            waitMsg: 'Saving Data...',
            success: function (response, request) {
              me.fireEvent('Save', me);
            },
            failure: function (response, request) {
              var rtext = request.result;
              if (rtext.toUpperCase().indexOf('FALSE') > 0) {
                var ind = rtext.indexOf('}');
                var len = rtext.length - ind - 1;
                var msg = rtext.substring(ind + 1, rtext.length - 1);
                showDialog(400, 100, 'Error saving endpoint changes', msg, Ext.Msg.OK, null);
                return;
              }
              var message = 'Error saving changes!';
              showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
            }
          });
        }
      },
      failure: function (response, request) {
        showDialog(400, 100, 'Testing Result', 'Connection failed. Please enter/select a valid base url.', Ext.Msg.OK, null);
        return;
      }
    });
  }
});
