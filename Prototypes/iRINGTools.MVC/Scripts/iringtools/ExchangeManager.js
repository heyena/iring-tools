/// <reference path="../ext-3.2.1/adapter/ext/ext-base.js" />
/// <reference path="../ext-3.2.1/ext-all.js" />
/// <reference path="../ext-3.2.1/ux/ux-all.js" />

// Application instance for showing user-feedback messages.
var iRINGTools = new Ext.iRINGTools({});

Ext.onReady(function () {
  Ext.QuickTips.init();

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

  var scopesStore = new Ext.data.JsonStore({
    // store configs
    autoDestroy: true,
    url: 'Scopes',
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
    url: 'Scopes/Applications',
    baseParams: {
      'scope': '12345_000'
    },
    // reader configs      
    root: 'Items',
    idProperty: 'Name',
    fields: ['Name', 'Description']

  });

  var graphStore = new Ext.data.JsonStore({
    // store configs
    autoDestroy: true,
    url: 'Scopes/Manifest',
    // reader configs
    root: 'Items',
    idProperty: 'Name',
    fields: ['Name']
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
        applicationStore.load({
          params: {
            'scope': '12345_000'
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

  var exhangePanel = new Ext.FormPanel({
    region: 'center',
    labelWidth: 110, // label settings here cascade unless overridden  
    frame: true,
    bodyStyle: 'padding:5px 5px 0',
    width: 390,
    height: 290,
    defaults: { width: 230 },
    defaultType: 'textfield',
    items: [
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
    ],

    buttons: [
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
    ]
  });



  // Finally, build the main layout once all the pieces are ready.  This is also a good
  // example of putting together a full-screen BorderLayout within a Viewport.
  var viewPort = new Ext.Viewport({
    layout: 'border',
    title: 'Scope Editor',
    border: false,
    items: [
      exhangePanel
    ],
    renderTo: Ext.getBody()
  });

});