/// <reference path="../ext-3.2.1/adapter/ext/ext-base.js" />

Ext.ns('iIRNGTools', 'iIRNGTools.AdapterManager');
/**
* @class iIRNGTools.AdapterManager.ExchangePanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
iIRNGTools.AdapterManager.ExchangePanel = Ext.extend(Ext.FormPanel, {
  labelWidth: 90, // label settings here cascade unless overridden  
  frame: true,
  bodyStyle: 'padding:5px 5px 0',
  width: 350,
  height: 260,
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

    this.items = [
      {
        fieldLabel: 'Graph',
        name: 'graphName',
        allowBlank: false
      }, {
        fieldLabel: 'Enpoint URI',
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
          name: 'password'
        }]
      }
    ];

    this.buttons = [
      {
        text: 'Transfer',
        handler: this.onPull,
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

  onPull: function (btn, ev) {
    this.fireEvent('pull', this, this.getForm());   
  },

  onCancel: function (btn, ev) {
    this.fireEvent('cancel', this, this.getForm());       
  }

});