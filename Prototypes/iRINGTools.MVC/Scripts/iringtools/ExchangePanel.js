﻿/// <reference path="../ext-3.2.1/adapter/ext/ext-base.js" />

Ext.ns('iIRNGTools', 'iIRNGTools.AdapterManager');
/**
* @class iIRNGTools.AdapterManager.ExchangePanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
iIRNGTools.AdapterManager.ExchangePanel = Ext.extend(Ext.FormPanel, {
  labelWidth: 110, // label settings here cascade unless overridden  
  frame: true,
  bodyStyle: 'padding:5px 5px 0',
  width: 390,
  height: 290,
  defaults: { width: 230 },
  defaultType: 'textfield',

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
          method: 'GET'
        })
      }//,
      //      listeners: {
      //        beforeload: function (proxy, params) {
      //          iRINGTools.setAlert(true, params);
      //        }
      //      }
    });

    var scopesStore = new Ext.data.JsonStore({
      // store config
      autoDestroy: true,
      proxy: new Ext.data.HttpProxy({
        api: {
          read: new Ext.data.Connection({
            url: 'Scopes',
            method: 'GET'
          })
        }
      }),
      // url: 'scopes',
      baseParams: {
        'remote': null
      },
      // reader configs
      root: 'Items',
      idProperty: 'Name',
      fields: ['Name', 'Description', 'Applications']//,
      //      listeners: {
      //        beforeload: function (store, options) {
      //          iRINGTools.setAlert(true, options);
      //        }
      //      }
    });

    var applicationStore = new Ext.data.JsonStore({
      // store configs
      autoDestroy: true,

      proxy: new Ext.data.HttpProxy({
        api: {
          read: new Ext.data.Connection({
            url: 'Scopes/Applications',
            method: 'GET'
          })
        }
      }),
      root: 'Items',
      idProperty: 'Name',
      fields: ['Name', 'Description']
    });

    var graphStore = new Ext.data.JsonStore({
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
      root: 'Items',
      idProperty: 'dataObjectMap',
      fields: [{ name: 'dataObjectMap'}],
      listeners: {
        load: function (store, records, options) {
          iRINGTools.setAlert(true, records);
        }
      }
    });

    // create the combo instance
    var cmbMethod = new Ext.form.ComboBox({
      fieldLabel: 'Exchange Method',
      name: 'cmbExchangeMethod',
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

          var uri = servicesURI.getValue();

          if (uri.substr(uri.length, 1) != '/') uri += '/';

          endPointURI.setValue(uri + record.data.Uri);
        }
      }
    });

    // create the combo instance
    var cmbScope = new Ext.form.ComboBox({
      fieldLabel: 'Scope',
      name: 'cmbScope',
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
          applicationStore.load({
            params: {
              'scope': record.data.Name,
              'remote': txt.value
            }
          });
        }
      }
    });

    // create the combo instance
    var cmbApplication = new Ext.form.ComboBox({
      fieldLabel: 'Application',
      name: 'cmbApplication',
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

          graphStore.load({
            params: {
              remote: txtServicesURI.value,
              scope: cmbScope.value,
              application: cmbApplication.value
            }
          });
        }
      }
    });

    // create the combo instance
    var cmbGraph = new Ext.form.ComboBox({
      fieldLabel: 'Graph',
      name: 'cmbGraph',
      typeAhead: true,
      triggerAction: 'all',
      lazyRender: true,
      mode: 'local',
      store: graphStore,
      valueField: 'Name',
      displayField: 'Name',
      listeners: {
        select: function (combo, record, index) {

        }
      }
    });

    this.items = [
      {
        id: 'txtServicesURI',
        fieldLabel: 'iRING Services URI',
        name: 'iRINGServicesUri',
        value: 'http://adcrdlweb/services',
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
        fieldLabel: 'Graph Base URI',
        name: 'targetGraphBaseUri',
        allowBlank: false
      }, {
        xtype: 'fieldset',
        title: 'Credentials',
        checkboxToggle: true,
        autoHeight: true,
        autoWidth: true,
        defaults: { width: 210 },
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
      {
        text: 'Fetch',
        handler: function (btn, ev) {
          var txt = Ext.getCmp('txtServicesURI');

          scopesStore.load({
            params: {
              'remote': txt.value
            }
          });
        },
        scope: this
      }, {
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
    this.fireEvent('exchange', this, this.getForm());
  },

  onCancel: function (btn, ev) {
    this.fireEvent('cancel', this, this.getForm());
  }

});