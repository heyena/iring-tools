/// <reference path="../ext-3.2.1/adapter/ext/ext-base.js" />

Ext.ns('iIRNGTools', 'iIRNGTools.AdapterManager');
/**
* @class iIRNGTools.AdapterManager.ExchangePanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
iIRNGTools.AdapterManager.ExchangePanel = Ext.extend(Ext.FormPanel, {
  labelWidth: 130, // label settings here cascade unless overridden  
  frame: true,
  bodyStyle: 'padding:5px 5px 0',
  width: 490,
  height: 390,
  defaults: { width: 290 },
  defaultType: 'textfield',
  waitMsgTarget: true,
  scope: null,
  application: null,

  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

    this.addEvents({
      pull: true,
      cancel: true
    });

    var methodStore = new Ext.data.JsonStore({
      // store configs
      autoDestroy: true,
      url: 'Exchange/Methods',
      // reader configs
      root: 'Items',
      idProperty: 'Name',
      fields: ['Name', 'Uri']
    });

    methodStore.load();

    var scopesProxy = new Ext.data.HttpProxy({
      api: {
        read: new Ext.data.Connection({
          url: 'scopes',
          method: 'GET',
          timeout: 12000

        })
      }
    });

    var scopesStore = new Ext.data.JsonStore({
      // store config
      autoDestroy: true,
      proxy: new Ext.data.HttpProxy({
        api: {
          read: new Ext.data.Connection({
            url: 'Scopes',
            method: 'GET',
            timeout: 12000
          })
        }
      }),
      baseParams: {
        'remote': null
      },
      // reader configs
      root: 'Items',
      idProperty: 'Name',
      fields: ['Name', 'Description', 'Applications']
    });

    var applicationStore = new Ext.data.JsonStore({
      // store configs
      autoDestroy: true,

      proxy: new Ext.data.HttpProxy({
        api: {
          read: new Ext.data.Connection({
            url: 'Scopes/Applications',
            method: 'GET',
            timeout: 12000
          })
        }
      }),
      root: 'Items',
      idProperty: 'Name',
      fields: ['Name', 'Description']
    });

    var graphStore = new Ext.data.XmlStore({
      // store configs
      autoDestroy: true,

      proxy: new Ext.data.HttpProxy({
        api: {
          read: new Ext.data.Connection({
            url: 'Scopes/Mapping',
            method: 'GET'
          })
        }
      }),
      // reader configs
      record: 'GraphMap',
      idPath: 'name',
      fields: [{ name: 'Name', mapping: 'name'}],
      listeners: {
        load: function (store, records, options) {
          iRINGTools.setAlert(true, records);
        }
      }
    });

    // create the combo instance
    var cmbMethod = new Ext.form.ComboBox({
      fieldLabel: 'Exchange Method',
      name: 'exchangemethod',
      typeAhead: true,
      triggerAction: 'all',
      lazyRender: true,
      mode: 'local',
      store: methodStore,
      valueField: 'Uri',
      displayField: 'Name',
      listeners: {

        select: function (combo, record, index) {
          var servicesURI = Ext.getCmp('txtServicesURI');
          var endPointURI = Ext.getCmp('txtEndPointURI');
          var txtGraphBaseURI = Ext.getCmp('txtGraphBaseURI');
          var uri = servicesURI.getValue();

          if (uri.substr(uri.length, 1) != '/') uri += '/';

          endPointURI.setValue(uri + record.data.Uri);



          scopesStore.load({
            params: {
              'remote': servicesURI.value
            }
          });
          applicationStore.removeAll(true);
          graphStore.removeAll(true);
          cmbApplication.setValue("");
          cmbGraph.setValue("");
          txtGraphBaseURI.setValue("");
        }
      }
    });

    // create the combo instance
    var cmbScope = new Ext.form.ComboBox({
      fieldLabel: 'Scope',
      name: 'targetScope',
      typeAhead: true,
      triggerAction: 'all',
      lazyRender: true,
      mode: 'local',
      store: scopesStore,
      valueField: 'Name',
      displayField: 'Name',
      listeners: {
        select: function (combo, record, index) {
          var txt = Ext.getCmp('txtServicesURI');
          var txtGraphBaseURI = Ext.getCmp('txtGraphBaseURI');
          applicationStore.load({
            params: {
              'scope': record.data.Name,
              'remote': txt.value
            }
          });
          graphStore.removeAll(true);
          cmbApplication.setValue("");
          cmbGraph.setValue("");
          txtGraphBaseURI.setValue("");
        }
      }
    });

    // create the combo instance
    var cmbApplication = new Ext.form.ComboBox({
      fieldLabel: 'Application',
      name: 'targetApplication',
      typeAhead: true,
      triggerAction: 'all',
      lazyRender: true,
      mode: 'local',
      store: applicationStore,
      valueField: 'Name',
      displayField: 'Name',
      listeners: {
        select: function (combo, record, index) {
          var txt = Ext.getCmp('txtServicesURI');
          var txtGraphBaseURI = Ext.getCmp('txtGraphBaseURI');
          graphStore.load({
            params: {
              remote: txt.value,
              scope: cmbScope.value,
              application: record.data.Name
            }
          });
          txtGraphBaseURI.setValue("");
        }
      }
    });

    // create the combo instance
    var cmbGraph = new Ext.form.ComboBox({
      fieldLabel: 'Graph',
      name: 'targetGraph',
      typeAhead: true,
      triggerAction: 'all',
      lazyRender: true,
      mode: 'local',
      store: graphStore,
      valueField: 'Name',
      displayField: 'Name',
      listeners: {
        select: function (combo, record, index) {
          var txtServicesURI = Ext.getCmp('txtServicesURI');
          var txtGraphBaseURI = Ext.getCmp('txtGraphBaseURI');

          var uri = txtServicesURI.getValue();
          if (uri.substr(uri.length, 1) != '/') uri += '/';

          uri += "adapter/";
          uri += cmbScope.getValue() + "/";
          uri += cmbApplication.getValue() + "/";
          uri += cmbGraph.getValue();

          //http: //adcrdlweb/services/adapterservice/12345_000/EXCEL/Valves

          txtGraphBaseURI.setValue(uri);
        }
      }
    });

    this.items = [
      {
        id: 'txtServicesURI',
        fieldLabel: 'iRING Services URI',
        name: 'targetServicesUri',
        value: 'http://adcrdlweb.corp.hatchglobal.com/Services',
        allowBlank: false
      },
      cmbMethod,
      cmbScope,
      cmbApplication,
      cmbGraph,
      {
        id: 'txtEndPointURI',
        fieldLabel: 'Endpoint URI',
        name: 'targetEndpointUri',
        allowBlank: false
      }, {
        id: 'txtGraphBaseURI',
        fieldLabel: 'Graph Base URI',
        name: 'targetGraphBaseUri',
        allowBlank: false
      }, {
        xtype: 'fieldset',
        title: 'Credentials',
        checkboxToggle: true,
        autoHeight: true,
        autoWidth: true,
        defaults: { width: 270 },
        defaultType: 'textfield',
        collapsed: true,
        items: [{
          fieldLabel: 'Domain',
          name: 'domain'
        }, {
          fieldLabel: 'Username',
          name: 'username'
        }, {
          fieldLabel: 'Password',
          inputType: 'password',
          name: 'password'
        }]
      }
    ];

    this.buttons = [
    //      {
    //        text: 'Fetch',
    //        handler: function (btn, ev) {
    //          var txt = Ext.getCmp('txtServicesURI');

    //          scopesStore.load({
    //            params: {
    //              'remote': txt.value
    //            }
    //          });
    //        },
    //        scope: this
    //      }, 
      {
      text: 'Exchange',
      handler: this.onExchange,
      scope: this
    }, {
      text: 'Cancel',
      handler: this.onCancel,
      scope: this
    }
    ];

    // super
    iIRNGTools.AdapterManager.ExchangePanel.superclass.initComponent.call(this);
  },

  onExchange: function (btn, ev) {
    this.fireEvent('exchange', this, this.getForm().submit({ waitMsg: 'Executing...'}));
  },

  onCancel: function (btn, ev) {
    this.fireEvent('cancel', this, this.getForm());
  }

});