/// <reference path="../ext-3.2.1/adapter/ext/ext-base.js" />
/// <reference path="../ext-3.2.1/ext-all.js" />
/// <reference path="../ext-3.2.1/ux/ux-all.js" />

// Application instance for showing user-feedback messages.
var iRINGTools = new Ext.iRINGTools({});

Ext.onReady(function () {
  Ext.QuickTips.init();

  var searchPanel = new iIRNGTools.AdapterManager.SearchPanel({
    id: 'search-panel',
    title: 'Reference Data Search',
    region: 'south',
    collapseMode: 'mini',
    height: 300,    
    collapsible: true,
    collapsed: false,
    split: true,
    layout: 'fit',
    border: true,

    searchUrl: 'Search',
    limit: 100
  });

  var defintionPanel = new iIRNGTools.ScopeEditor.ScopeMapping({
    id: 'def-panel',    
    title: 'Scope Definition',
    collapsible: false,    
    layout: 'fit',
    border: true,
    closable: true,

    loader: new Ext.tree.TreeLoader({}),

    root: new Ext.tree.AsyncTreeNode({
      expanded: true,
      children: [{
        text: 'ScopeGraph',
        leaf: false,
        children: [{
          text: 'ScopeTemplate1',
          leaf: false,
          children: [{
            text: 'ScopeTemplate2',
            leaf: true
          }]
        }]
      }]
    }),
    
    bbar: new Ext.ux.StatusBar({
      id: 'right-statusbar',
      defaultText: 'Ready',      
      statusAlign: 'right'   
    })

  });

  var contentPanel = new Ext.TabPanel({
    id: 'content-panel',
    region: 'center', // this is what makes this panel into a region within the containing layout    
    collapsible: false,    
    border: true    
  });

  var navigationPanel = new Ext.Panel({
    id: 'nav-panel',
    title: 'Registered Scopes',
    region: 'west',
    collapseMode: 'mini',
    width: 200,
    collapsible: true,
    collapsed: false,
    split: true,
    layout: 'border',
    border: true,
    items: [
      treePanel,
      detailsPanel
    ],
    tbar: [
      { 
        text: 'New', 
        handler: function(btn, ev) { 
          
          var window = new Ext.Window({
            title: 'Scope Details',
            width: 300,
            height: 300
          });

          window.show();

        },
        scope: this 
      },
      { text: 'Update' },
      { text: 'Configure' },
      { text: 'Mapping', handler: function(btn, ev) { 
          
          var node = treePanel.getSelectionModel().getSelectedNode();
          
          if (node.isLeaf()) {

            var parentNode = node.parentNode;

            var newTab = new iIRNGTools.AdapterManager.MappingPanel({
              title: 'Mapping - ' + parentNode.attributes.Scope.Name + '.' + node.attributes.Application.Name,
              closable: true
            });

            contentPanel.add(newTab);
            contentPanel.activate(newTab);
          }
        }
      }
    ]
  });

  // Load Stores
  searchPanel.load();

  // Finally, build the main layout once all the pieces are ready.  This is also a good
  // example of putting together a full-screen BorderLayout within a Viewport.
  var viewPort = new Ext.Viewport({
    layout: 'border',
    title: 'Scope Editor',
    items: [
      {
        xtype: 'box',
        region: 'north',
        applyTo: 'header',
        height: 60
      },
      navigationPanel,
      contentPanel,
      searchPanel
    ],
    listeners: {
      render: function() {
        // After the component has been rendered, disable the default browser context menu
        Ext.getBody().on("contextmenu", Ext.emptyFn, null, {preventDefault: true});
      }
    },
    renderTo: Ext.getBody()
  });
});