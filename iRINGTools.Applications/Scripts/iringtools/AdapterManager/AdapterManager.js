/// <reference path="../ext-3.2.1/adapter/ext/ext-base.js" />
/// <reference path="../ext-3.2.1/ext-all.js" />
/// <reference path="../ext-3.2.1/ux/ux-all.js" />

// Application instance for showing user-feedback messages.
var App = new Ext.App({});

Ext.onReady(function () {
  Ext.QuickTips.init();

  /*
  var actionPanel = new AdapterManager.ActionPanel({
  id: 'action-panel',
  region: 'west',
  width: 200,

  collapseMode: 'mini',
  collapsible: true,
  collapsed: false
  });
  */



  var searchPanel = new AdapterManager.SearchPanel({
    id: 'search-panel',
    title: 'Reference Data Search',
    region: 'south',
    height: 300,

    collapseMode: 'mini',
    collapsible: true,
    collapsed: false,

    searchUrl: 'refdata',
    limit: 100
  });

  var contentPanel = new Ext.TabPanel({
    id: 'content-panel',
    region: 'center',

    collapsible: false,
    closable: true,
    enableTabScroll: true,
    border: true,
    split: true
  });

  var directoryPanel = new AdapterManager.DirectoryPanel({
    id: 'nav-panel',
    title: 'Directory',
    region: 'west',
    width: 250,

    collapseMode: 'mini',
    collapsible: true,
    collapsed: false,

    navigationUrl: 'directory/getnode'
  });

  directoryPanel.on('newscope', function (npanel, node) {

    if (node.attributes.type == "ScopeNode") {
      node = node.parentNode;
    }

    var newTab = new AdapterManager.ScopePanel({
      id: 'tab-' + node.id,
      title: 'Scope - (New)',
      record: node.attributes.record,
      url: 'directory/scope'
    });

    newTab.on('save', function (panel) {
      contentPanel.remove(panel);
      directoryPanel.reload();
    }, this);

    contentPanel.add(newTab);
    contentPanel.activate(newTab);

  }, this);

  directoryPanel.on('editscope', function (npanel, node) {

    var newTab = new AdapterManager.ScopePanel({
      id: 'tab-' + node.id,
      title: 'Scope - ' + node.text,
      record: node.attributes.record,
      url: 'directory/scope'
    });

    contentPanel.add(newTab);
    contentPanel.activate(newTab);

  }, this);
  
  directoryPanel.on('deletescope', function (npanel, node) {
   
    if (node.attributes.type != "ApplicationNode" && node.attributes.type != "ScopeNode") {
      Ext.MessageBox.show({
        title: '<font color=red></font>',
        msg: 'Please select a child node to delete.',
        buttons: Ext.MessageBox.OK,
        icon: Ext.MessageBox.INFO
      });
    } else if (node == null) {
      Ext.Msg.alert('Warning', 'Please select a node.');
    } 
    {
       Ext.Ajax.request({
          url: 'directory/deleteNode',
          method: 'GET',
          params: {
            'nodeId': node.attributes.id,
            'parentNodeID': node.parentNode.attributes.id
         },
         success: function (o) {
            directoryPanel.reload();
            Ext.Msg.alert('Sucess', 'Node has been deleted');
         },
         failure: function (f, a) {
           Ext.Msg.alert('Warning', 'Error!!!');
        }
      });
    } 
  }, this);


  directoryPanel.on('newapplication', function (npanel, node) {

    if (node.attributes.type == "ApplicationNode") {
      node = node.parentNode;
    }

    var newTab = new AdapterManager.ApplicationPanel({
      id: 'tab-' + node.id,
      title: 'Application - ' + node.parentNode.text + '.(new)',
      scope: node.attributes.record,
      record: null,
      url: 'directory/application',
      closable: true
    });

    newTab.on('save', function (panel) {
      contentPanel.remove(panel);
      directoryPanel.reload();
    }, this);

    contentPanel.add(newTab);
    contentPanel.activate(newTab);

  }, this);

  directoryPanel.on('editapplication', function (npanel, node) {

    var newTab = new AdapterManager.ApplicationPanel({
      id: 'tab-' + node.id,
      title: 'Application - ' + node.parentNode.text + '.' + node.text,
      scope: node.parentNode.attributes.record,
      record: node.attributes.record,
      url: 'directory/application',
      closable: true
    });

    newTab.on('configure', function (panel, scope, application) {

      if (application.DataLayer = 'ExcelLibrary') {

        var newConfig = new AdapterManager.ExcelLibraryPanel({
          id: 'tab-c.' + scope.Name + '.' + application.Name,
          title: 'Configure - ' + scope.Name + '.' + application.Name,
          scope: scope,
          application: application,

          url: 'excel/configure',
          closable: true
        });

        contentPanel.add(newConfig);
        contentPanel.activate(newConfig);

      } else if (application.DataLayer = 'NHibernateLibrary') {

      } else {

      }

    }, this);

    contentPanel.add(newTab);
    contentPanel.activate(newTab);

  }, this);

  directoryPanel.on('deleteapplication', function (npanel, node) {
  }, this);


  directoryPanel.on('openmapping', function (npanel, node) {

    var scope = node.parentNode;
    var application = node;

    var newTab = new AdapterManager.MappingPanel({
      title: 'Mapping - ' + scope.text + "." + application.text,
      scope: scope.attributes.record,
      application: application.attributes.record
    });

    contentPanel.add(newTab);
    contentPanel.activate(newTab);

  }, this);

  directoryPanel.on('remove', function (npanel, node) {
    that = this;
    if (node.hasChildNodes()) {
      Ext.MessageBox.show({
        title: '<font color=red></font>',
        msg: 'Please select a child node to delete.',
        buttons: Ext.MessageBox.OK,
        icon: Ext.MessageBox.INFO
      });
    } else if (node == null) {
      Ext.Msg.alert('Warning', 'Please select a node.')
    } else {
      Ext.Msg.show({
        msg: 'All the tabs will be closed. Do you want to delete this node?',
        buttons: Ext.Msg.YESNO,
        icon: Ext.Msg.QUESTION,
        fn: function (action) {
          if (action == 'yes') {
            //send ajax request
            Ext.Ajax.request({
              url: 'directory/deletenode',
              method: 'GET',
              params: { 'nodeId': node.id, 'parentNodeID': node.parentNode.id },
              success: function (o) {
                // remove all tabs form tabpanel
                Ext.getCmp('contentpanel').removeAll(true); // it will be removed in future
                // remove the node form tree
                //that.federationPanel.selModel.selNode.parentNode.removeChild(node);
                //Tree Reload
                that.onRefresh();
                // fire event so that the Details panel will be changed accordingly
                that.fireEvent('selectionchange', this)
                Ext.Msg.alert('Sucess', 'Node has been deleted')
              },
              failure: function (f, a) {
                Ext.Msg.alert('Warning', 'Error!!!')
              }
            });
          } else if (action == 'no') {
            Ext.Msg.alert('Info', 'Not now');
          }
        }
      });
    }
  });

  // Load Stores
  searchPanel.load();

  // Finally, build the main layout once all the pieces are ready.  This is also a good
  // example of putting together a full-screen BorderLayout within a Viewport.
  var viewPort = new Ext.Viewport({
    layout: 'border',
    title: 'Scope Editor',
    border: false,
    items: [
      {
        xtype: 'box',
        region: 'north',
        applyTo: 'header',
        border: false,
        height: 60
      },
      directoryPanel,
      contentPanel,
      searchPanel
    ],
    listeners: {
      render: function () {
        // After the component has been rendered, disable the default browser context menu
        Ext.getBody().on("contextmenu", Ext.emptyFn, null, { preventDefault: true });
      }
    },
    renderTo: Ext.getBody()
  });

});