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
            url: 'service/search'
        }),
        reader: new Ext.data.JsonReader({
            root: 'RefDataEntities',
            totalProperty: 'Count',
            id: 'label'
        }, [
            { name: 'label', allowBlank: false },
            { name: 'uri', allowBlank: false },
            { name: 'repository', allowBlank: false }
        ]),

        baseParams: { limit: 20 }
    });

    // Custom rendering Template for the View
    var resultTpl = new Ext.XTemplate(
        '<tpl for=".">',
        '<div class="search-item">',
            '<h3><span>{label} {uri} {repository}</span></h3>',            
        '</div></tpl>'
    );

    var searchPanel = new Ext.Panel({
        id: 'search-panel',
        region: 'south', // this is what makes this panel into a region within the containing layout
        title: 'Reference Data Search',
        layout: 'fit',
        split: true,
        //margins: '2 5 5 0',
        border: true,

        items: new Ext.DataView({
            tpl: resultTpl,
            store: searchStore,
            itemSelector: 'div.search-item'
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
            pageSize: 20,
            displayInfo: true,
            displayMsg: 'Topics {0} - {1} of {2}',
            emptyMsg: "No topics to display"
        })
    });

    searchStore.load({ params: { start: 0, limit: 20 } });

    var mappingPanel = new iIRNGTools.ScopeEditor.ScopeMapping({
        id: 'mapping-panel',
        region: 'center', // this is what makes this panel into a region within the containing layout
        title: 'Scope Mapping',
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

    // This is the main content center region that will contain each example layout panel.
    // It will be implemented as a CardLayout since it will contain multiple panels with
    // only one being visible at any given time.
    var contentPanel = {
        id: 'content-panel',
        region: 'center', // this is what makes this panel into a region within the containing layout
        layout: 'card',
        margins: '2 5 5 0',
        activeItem: 0,
        border: false,
        items: [{
            id: 'border-panel',
            region: 'center', // this is what makes this panel into a region within the containing layout
            layout: 'border',
            border: false,
            items: [mappingPanel, searchPanel]
        }]
    };

    // Go ahead and create the TreePanel now so that we can use it below
    var treePanel = new Ext.tree.TreePanel({
        id: 'tree-panel',
        title: 'Registered Scopes',
        region: 'north',
        split: true,
        height: 300,
        minSize: 150,
        autoScroll: true,

        // tree-specific configs:
        rootVisible: false,
        lines: false,
        singleExpand: true,
        useArrows: true,

        loader: new Ext.tree.TreeLoader({
            dataUrl: 'service/scopes?format=tree'
        }),

        root: new Ext.tree.AsyncTreeNode({})
    });

    // Assign the changeLayout function to be called on tree node click.
    treePanel.on('click', function (n) {
        var sn = this.selModel.selNode || {}; // selNode is null on initial selection              
        if (n.leaf && n.id != sn.id) {  // ignore clicks on folders and currently selected node                     

            var ScopeDetailRecord = Ext.data.Record.create([ // creates a subclass of Ext.data.Record
                        {name: 'scopeName', allowBlank: false },
                        { name: 'scopeDescription' },
                        { name: 'applicationName', allowBlank: false },
                        { name: 'applicationDescription' },
                    ]);

            // create Record instance
            var rec = new ScopeDetailRecord(
                        {
                            scopeName: n.attributes.scopeName,
                            scopeDescription: n.attributes.scopeDescription,
                            applicationName: n.attributes.applicationName,
                            applicationDescription: n.attributes.applicationDescription
                        }
                    );

            detailsPanel.loadRecord(rec);

            //Ext.getCmp('content-panel').layout.setActiveItem(n.id + '-panel');
            //if (!detailEl) {
            //    var bd = Ext.getCmp('details-panel').body;
            //    bd.update('').setStyle('background', '#fff');
            //    detailEl = bd.createChild(); //create default empty div
            //}
            //detailEl.hide().update(Ext.getDom(n.id + '-details').innerHTML).slideIn('l', { stopFx: true, duration: .2 });

        }
    });

    // This is the Details panel that contains the description for each example layout.
    var detailsPanel = new iIRNGTools.ScopeEditor.ScopeDetails({
        id: 'details-panel',
        title: 'Scope Details',
        region: 'center',
        bodyStyle: 'padding-bottom:15px;background:#eee;',
        autoScroll: true,
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
        items: [{
            xtype: 'box',
            region: 'north',
            applyTo: 'header',
            height: 30
        }],
        items: [{
            xtype: 'box',
            region: 'north',
            applyTo: 'header',
            height: 30
        }, {
            layout: 'border',
            id: 'layout-browser',
            region: 'west',
            border: false,
            split: true,
            margins: '2 0 5 5',
            width: 275,
            minSize: 100,
            maxSize: 500,
            items: [treePanel, detailsPanel]
        },
                    contentPanel
                ],
        renderTo: Ext.getBody()
    });
}); 