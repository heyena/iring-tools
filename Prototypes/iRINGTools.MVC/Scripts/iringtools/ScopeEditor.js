/// <reference path="../ext-3.2.1/adapter/ext/ext-base.js" />
/// <reference path="../ext-3.2.1/ext-all.js" />
/// <reference path="../ext-3.2.1/ux/ux-all.js" />

/*!
* Ext JS Library 3.2.1
* Copyright(c) 2006-2010 Ext JS, Inc.
* licensing@extjs.com
* http://www.extjs.com/license
*/

// Application instance for showing user-feedback messages.
var iRINGTools = new Ext.iRINGTools({});

Ext.onReady(function () {
  Ext.QuickTips.init();

  var searchStore = new Ext.data.Store({
    proxy: new Ext.data.HttpProxy({
      url: 'Search'
    }),
    reader: new Ext.data.JsonReader({
      root: 'Entities',
      totalProperty: 'Total',
      id: 'label'
    }, 
    [
      { name: 'uri', allowBlank: false },
      { name: 'label', allowBlank: false },
      { name: 'repository', allowBlank: false }
    ]),
    baseParams: { limit: 100 }
  });

  var searchExpander = new Ext.ux.grid.RowExpander({
    tpl: new Ext.Template(
      '<p><b>Uri:</b> {uri}</p><br>',
      '<p><b>Description:</b> {desc}</p>'
    )
  });

  var searchPanel = new Ext.Panel({
    id: 'search-panel',
    region: 'south',
    collapseMode: 'mini',
    height: 300,
    title: 'Reference Data Search',
    collapsible: true,
    collapsed: false,
    split: true,
    layout: 'fit',
    border: true,

    items: new Ext.grid.GridPanel({
      store: searchStore,
      plugins: searchExpander,
      cm: new Ext.grid.ColumnModel([
        searchExpander,
        { header: "Label", width: 400, sortable: true, dataIndex: 'label' },
        { header: "Repository", width: 150, sortable: true, dataIndex: 'repository' },
        { header: "Uri", width: 400, sortable: true, dataIndex: 'uri', hidden: true }
      ]),
      listeners:{        
		    contextmenu: function(e){
          this.contextMenu = new Ext.menu.Menu({
			      id: 'gridCtxMenu',
			      items: [
              {
				        text: 'Add'
			        },
              {
				        text: 'Edit'
			        }
            ]
		      });		      
	        xy = e.getXY();
	        this.contextMenu.showAt(xy);
        }		    			
	    }
    }),

    tbar: [
      'Search: ', ' ',
      new Ext.ux.form.SearchField({
        store: searchStore,
        width: 320
      }, ' ', { text: '' })
    ],

    bbar: new Ext.PagingToolbar({
      store: searchStore,
      pageSize: 100,
      displayInfo: true,
      displayMsg: 'Results {0} - {1} of {2}',
      emptyMsg: "No results to display"
    })    
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
    border: true,
    activeTab: 0,
    items: [
      defintionPanel
    ]
  });

  var treePanel = new Ext.tree.TreePanel({
    id: 'tree-panel',
    region: 'center',
    collapseMode: 'mini',
    height: 200,
    collapsible: false,
    collapsed: false,
    split: true,
    layout: 'fit',
    border: true,

    rootVisible: false,
    lines: false,
    singleExpand: true,
    useArrows: true,

    loader: new Ext.tree.TreeLoader({
      dataUrl: 'Scopes/Navigation'
    }),

    root: new Ext.tree.AsyncTreeNode({}),

    listeners: {
      click: function (n) {
        var sn = this.selModel.selNode || {}; // selNode is null on initial selection              
        if (n.id != sn.id) {  // ignore clicks on folders and currently selected node                     
          if (n.leaf) {
            detailsPanel.loadRecord(n.attributes.Application);
          } else {
            detailsPanel.loadRecord(n.attributes.Scope);
          }
        }
      }
    }
  });

  // This is the Details panel that contains the description for each example layout.
  var detailsPanel = new iIRNGTools.ScopeEditor.ScopeDetails({
    id: 'details-panel',
    title: 'Details',
    region: 'south',
    collapseMode: 'mini',
    height: 150,
    collapsible: true,
    collapsed: false,
    split: true,
    layout: 'fit',
    border: true,
    frame: false,

    propertyNames: {
      name: 'Name',
      description: 'Description'
    },

    listeners: {
      create: function (fpanel, data) {   // <-- custom "create" event defined in App.user.Form class
        //var rec = new userGrid.store.recordType(data);
        //userGrid.store.insert(0, rec);
      }
    }
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

            var newTab = new Ext.tree.TreePanel({
              title: 'Mapping - ' + parentNode.attributes.Scope.Name + '.' + node.attributes.Application.Name,
              closable: true,
    
              rootVisible: false,
              lines: false,
              singleExpand: true,
              useArrows: true,

              loader: new Ext.tree.TreeLoader({}),

              root: new Ext.tree.AsyncTreeNode({
                expanded: true,
                children: [{
                  text: 'MappingGraph',
                  expanded: true,
                  leaf: false,
                  children: [
                    {
                      text: 'MapTemplate1',
                      leaf: true                    
                    },
                    {
                      text: 'MapTemplate2',
                      leaf: true
                    }
                  ]
                }]
              }),

              tbar: [{ text: 'Map'},{ text: 'Del'},{ text: '...'}]

            });

            contentPanel.add(newTab);

            contentPanel.activate(newTab);
          }
        }
      }
    ]
  });

  // Load Stores
  searchStore.load({ params: { start: 0, limit: 100} });

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
          height: 50
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