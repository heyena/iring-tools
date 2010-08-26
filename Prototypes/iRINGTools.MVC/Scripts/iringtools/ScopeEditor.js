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
        }, [
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
          ])
      }),

      tbar: [
          'Search: ', ' ',
          new Ext.ux.form.SearchField({
              store: searchStore,
              width: 320
          })
      ],

      bbar: new Ext.PagingToolbar({
          store: searchStore,
          pageSize: 100,
          displayInfo: true,
          displayMsg: 'Results {0} - {1} of {2}',
          emptyMsg: "No results to display"
      })
    });

    searchStore.load({ params: { start: 0, limit: 100} });

    var mappingPanel = new iIRNGTools.ScopeEditor.ScopeMapping({
        id: 'mapping-panel',
        region: 'center', // this is what makes this panel into a region within the containing layout
        title: 'Scope Definition',
        collapsible: false,
        layout: 'fit',
        border: true,

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

        })
    });

    // Go ahead and create the TreePanel now so that we can use it below
    var treePanel = new Ext.tree.TreePanel({
      id: 'tree-panel',
      title: 'Registered Scopes',
      region: 'west',
      width: 200,
      collapsible: true,
      collapsed: false,
      split: true,
      layout: 'fit',
      border: true,

      // tree-specific configs:
      rootVisible: false,
      lines: false,
      singleExpand: true,
      useArrows: true,

      loader: new Ext.tree.TreeLoader({
          dataUrl: 'Scopes/Navigation'
      }),

      

      root: new Ext.tree.AsyncTreeNode({})
    });

    // Assign the changeLayout function to be called on tree node click.
    treePanel.on('click', function (n) {
      var sn = this.selModel.selNode || {}; // selNode is null on initial selection              
      if (n.id != sn.id) {  // ignore clicks on folders and currently selected node                     
        if (n.leaf) {
          detailsPanel.loadRecord(n.attributes.Application);
        } else {
          detailsPanel.loadRecord(n.attributes.Scope);
        }
      }
    });

    var scopeStore = new Ext.data.Store({
        proxy: new Ext.data.HttpProxy({
            url: 'Scopes'
        }),
        reader: new Ext.data.JsonReader({
            root: 'Entities',
            totalProperty: 'Total',
            id: 'label'
        }, [
            { name: 'name', allowBlank: false },
            { name: 'description', allowBlank: false },
            { name: 'connectionString', allowBlank: false }
        ]),

        baseParams: { limit: 100 }
    });

    // This is the Details panel that contains the description for each example layout.
    var detailsPanel = new iIRNGTools.ScopeEditor.ScopeDetails({
        id: 'details-panel',
        title: 'Details',
        region: 'east',
        width: 200,
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
            height: 90
          },
          mappingPanel,
          searchPanel,
          treePanel,
          detailsPanel
        ],
        renderTo: Ext.getBody()
    });    
}); 