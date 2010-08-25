<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html>
<head runat="server">
    <title>Scope Editor</title>
    <link rel="stylesheet" type="text/css" href="../../scripts/ext/resources/css/ext-all.css"/>
    <script type="text/javascript" src="../../scripts/ext/adapter/ext/ext-base.js"></script>
    <script type="text/javascript" src="../../scripts/ext/adapter/ext/ext-base-debug.js"></script>    
    <script type="text/javascript" src="../../scripts/ext/ext-all.js"></script>
    <script type="text/javascript" src="../../scripts/ext/ext-all-debug.js"></script>

    <script type="text/javascript">
        Ext.onReady(function () {
            Ext.QuickTips.init();

            // This is an inner body element within the Details panel created to provide a "slide in" effect
            // on the panel body without affecting the body's box itself.  This element is created on
            // initial use and cached in this var for subsequent access.
            var detailEl;

            var searchPanel = {
                id: 'search-panel',
                region: 'south', // this is what makes this panel into a region within the containing layout
                title: 'Reference Data Search',
                layout: 'card',
                split: true,
                //margins: '2 5 5 0',
                activeItem: 0,
                border: true
            };

            var mappingPanel = {
                id: 'mapping-panel',
                region: 'center', // this is what makes this panel into a region within the containing layout
                title: 'Scope Mapping',
                layout: 'card',
                //margins: '2 5 5 0',
                activeItem: 0,
                border: true
            };

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
                    id: 'content1-panel',
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

                root: new Ext.tree.AsyncTreeNode()
            });

            //            // Assign the changeLayout function to be called on tree node click.
            //            treePanel.on('click', function (n) {                
            //                var sn = this.selModel.selNode || {}; // selNode is null on initial selection
            //                if (n.leaf && n.id != sn.id) {  // ignore clicks on folders and currently selected node 
            //                    Ext.getCmp('content-panel').layout.setActiveItem(n.id + '-panel');
            //                    if (!detailEl) {
            //                        var bd = Ext.getCmp('details-panel').body;
            //                        bd.update('').setStyle('background', '#fff');
            //                        detailEl = bd.createChild(); //create default empty div
            //                    }
            //                    detailEl.hide().update(Ext.getDom(n.id + '-details').innerHTML).slideIn('l', { stopFx: true, duration: .2 });
            //                
            //                }                                
            //            });            

            // This is the Details panel that contains the description for each example layout.
            var detailsPanel = {
                id: 'details-panel',
                title: 'Scope Details',
                region: 'center',
                bodyStyle: 'padding-bottom:15px;background:#eee;',
                autoScroll: true,
                html: '<p class="details-info">When you select a layout from the tree, additional details will display here.</p>'
            };

            // Finally, build the main layout once all the pieces are ready.  This is also a good
            // example of putting together a full-screen BorderLayout within a Viewport.
            var viewPort = new Ext.Viewport({
                layout: 'border',
                title: 'Ext Layout Browser',
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
                renderTo: document.body
            });

        });        
    </script>
</head>
<body>
    <div id="header"><h1>Ext Layout Browser</h1></div>
    <div style="display:none;">

        <!-- Start page content -->
        <div id="start-div">            
        </div>

    </div>
</body>
</html>
