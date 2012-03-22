
Ext.define('AM.view.nhibernate.NHibernateTree', {
  extend: 'Ext.tree.Panel',
  alias: 'widget.nhibernatetreepanel',
  bodyStyle: 'padding:0.5px 1px 1px 1px',
  autoScroll: true,
  animate: true,
  expandAll: true,
  lines: true,
  frame: false,
  enableDD: false,
  containerScroll: true,
  rootVisible: true,
  dbDict: null,
  dbInfo: null,
  dataTypes: null,
  contextName: null,
  endpoint: null,
  baseUrl: null,
 // store: null,
  width: 300,
  root: {
    expanded: false,
    type: 'DATAOBJECTS',
    text: 'Data Objects',
    iconCls: 'folder'
  },
  initComponent: function () {
    var me = this;
    Ext.apply(this, {
      store: Ext.create('Ext.data.TreeStore', {
        model: 'AM.model.NHibernateTreeModel',
        clearOnLoad: true,
        root: {
          expanded: true,
          type: 'DATAOBJECTS',
          text: 'Data Objects',
          iconCls: 'folder'
        },
        proxy: {
          type: 'ajax',
          timeout: 600000,
          url: 'NHibernate/getnode',
          actionMethods: { read: 'POST' },
          extraParams: {
            contextName: me.contextName,
            endpoint: me.endpoint,
            baseUrl: me.baseUrl,
            type: 'DATAOBJECTS'
          },
          reader: { type: 'json' }
        }
      })
    });

    var wizard = this;
    var scopeName = this.contextName;
    var appName = this.endpoint;

    this.tbar = new Ext.Toolbar({
      items: [{
        xtype: 'tbspacer',
        width: 4
      }, {
        xtype: 'button',
        icon: 'Content/img/16x16/view-refresh.png',
        text: 'Reload',
        tooltip: 'Reload Data Objects',
        action: 'reloaddataobjects'//,
      }, {
        xtype: 'tbspacer',
        width: 4
      }, {
        xtype: 'button',
        icon: 'Content/img/16x16/document-properties.png',
        text: 'Edit Connection',
        tooltip: 'Edit database connection',
        action: 'editdbconnection'
      }, {
        xtype: 'tbspacer',
        width: 4
      }, {
        xtype: 'button',
        icon: 'Content/img/16x16/document-save.png',
        text: 'Save',
        tooltip: 'Save the data objects tree to the back-end server',
        formBind: true,
        action: 'savedbobjectstree'
      }]
    });

    this.callParent(arguments);
  },

  onReload: function () {
    this.getStore().load();
  }
});

