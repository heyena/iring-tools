/// <reference path="../ext-3.2.1/adapter/ext/ext-base.js" />

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

    var cmbServiceURI = new Ext.form.ComboBox({
      store: new Ext.data.JsonStore({
        
      }),
      listeners: {        
        'select': onSelect_ServiceURI
      }
    });

    this.items = [
      {
        fieldLabel: 'iRING Services URI',
        name: 'iRINGServicesUri',
        allowBlank: false
      }, {
        fieldLabel: 'Exchange Method',
        name: 'exchangeMethod',
        allowBlank: false
      }, {
        fieldLabel: 'Endpoint URI',
        name: 'targetEndpointUri',
        allowBlank: false
      }, {
        fieldLabel: 'Graph',
        name: 'graphName',
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
  },

  onSelect_ServiceURI: function (btn, ev) {
    
  }

});