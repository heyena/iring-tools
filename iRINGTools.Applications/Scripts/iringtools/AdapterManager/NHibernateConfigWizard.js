Ext.ns('AdapterManager');

AdapterManager.NHibernateConfigWizard = Ext.extend(Ext.Container, {
  constructor: function(config) {
    config = config || {};
    var wizard = this;

    var navBtns = [{
      text: 'Prev',
      handler: function() {
        var activeForm = wizard.getLayout().activeItem;

        // TODO: validate form and boundary

        var activeFormIndex = wizard.items.indexOf(activeForm);
        wizard.getLayout().setActiveItem(activeFormIndex - 1);
      }
    },{
      text: 'Next',
      handler: function() {
        var activeForm = wizard.getLayout().activeItem;

        // TODO: validate form and boundary

        var activeFormIndex = wizard.items.indexOf(activeForm);
        wizard.getLayout().setActiveItem(activeFormIndex + 1);
      }
    },{
      text: 'Cancel',
      handler: function() {
        wizard.destroy();
      }
    }];
    
    var dataSourceForm = new Ext.FormPanel({
      id: 'ds-form',
      labelWidth: 160,
      bodyStyle: 'padding:15px',
      border: false,
      frame: true,
      items: [{
        xtype: 'label',
        fieldLabel: 'Configure Data Source',
        labelSeparator: '',
        style: {
          marginBottom: '50px'
        },
        anchor: '100% -100'
      },{
        xtype: 'combo',
        hiddenName: 'Provider',
        fieldLabel: 'Database Provider',
        triggerAction: 'all',
        mode: 'local',
        editable: false,
        store: new Ext.data.JsonStore({
          autoLoad: true,
          url: 'AdapterManager/DataProviders',
          root: 'items',
          idProperty: 'Provider',
          fields: [{ 
            name: 'Provider'
          }]
        }),
        displayField: 'Provider',
        valueField: 'Provider',
        anchor: '40%'
      },{
        xtype: 'textfield',
        name: 'dataSourceFormDs',
        fieldLabel: 'Database Source',
        anchor: '40%'
      },{
        xtype: 'textfield',
        name: 'dataSourceFormSchema',
        fieldLabel: 'Schema Name',
        anchor: '40%'
      },{
        xtype: 'textfield',
        name: 'dataSourceFormUserId',
        fieldLabel: 'User ID',
        anchor: '40%'
      },{
        xtype: 'textfield',
        name: 'dataSourceFormPassword',
        fieldLabel: 'Password',
        anchor: '40%'
      },{
        buttons: navBtns        
      }]
    });

    var importForm = new Ext.FormPanel({
      id: 'import-form',
      labelWidth: 160,
      bodyStyle: 'padding:15px',
      border: false,
      frame: true,
      items: [{
        xtype: 'label',
        fieldLabel: 'Select tables to import',
        labelSeparator: '',
        style: {
          marginBottom: '50'
        },
        anchor: '100% -100'
      },{
        xtype: 'textfield',
        name: 'table',
        fieldLabel: 'table',
        anchor: '40%'
      },{
        buttons: navBtns
      }]
    });
    
    var dictionaryForm = new Ext.FormPanel({
      id: 'dictionary-form',
      labelWidth: 160,
      bodyStyle: 'padding:15px',
      border: false,
      frame: true,
      items: [{
        xtype: 'label',
        fieldLabel: 'Configure Database Dictionary',
        labelSeparator: '',
        style: {
          marginBottom: '50'
        },
        anchor: '100% -100'
      },{
        xtype: 'textfield',
        name: 'table',
        fieldLabel: 'table',
        anchor: '40%'
      },{
        buttons: navBtns
      }]
    });
    
    Ext.apply(this, {
      title: 'NHibernate Configuration Wizard',
      closable: true,
      layout: 'card',
      activeItem: 0,
      items: [dataSourceForm, importForm, dictionaryForm]
    });

    AdapterManager.NHibernateConfigWizard.superclass.constructor.apply(this, arguments);
  }
});
