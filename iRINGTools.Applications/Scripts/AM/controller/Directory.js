Ext.define('AM.controller.Directory', {
    extend: 'Ext.app.Controller',

    models: [
        'DirectoryModel',
        'DataLayerModel',
        'DynamicModel',
        'FileDownloadModel',
        'VirtualPropertyModel',
        'PermissionsM',
        'ResourceGroupModel'
    ],

    stores: [
        'DirectoryTreeStore',
        'DataLayerStore',
        'FileDownloadStore',
        'VirtualPropertyStore',
        'PermissionsS',
        'ResourceGroupStore'


    ],

    views: [
        'directory.DirectoryPanel',
        'common.PropertyPanel',
        'common.ContentPanel',
        'common.CenterPanel',
        'directory.DirectoryTree',
        'directory.ApplicationWindow',
        'directory.ScopeWindow',
        'directory.DataLayerForm',
        'directory.DataGridPanel',
        'directory.GraphMapForm',
        'directory.GraphMapWindow',
        'directory.ScopeForm',
        'directory.DataLayerWindow',
        'directory.DataLayerCombo',
        'directory.ApplicationForm',
        'menus.ScopesMenu',
        'menus.AppDataMenu',
        'menus.ApplicationMenu',
        'menus.ValueListsMenu',
        'menus.ValueListMenu',
        'menus.GraphsMenu',
        'menus.GraphMenu',
        'menus.TemplatemapMenu',
        'menus.RolemapMenu',
        'menus.ClassmapMenu',
        'menus.ValueListMapMenu',
        'menus.AppDataRefreshMenu',
        'directory.FileUpoadForm',
        'directory.FileUploadWindow',
        'directory.DownloadGrid',
        'directory.FileDownloadWindow',
        'directory.ImportCacheForm',
        'directory.ImportCacheWindow',
        'directory.DownloadForm',
        'directory.VirtualPropertyForm',
        'directory.VirtualPropertyGrid',
        'directory.VirtualPropertyWindow',
        'menus.VirtualPropertyMenu',
		'common.ExceptionPanel',
        'menus.SiteMenu',
        'directory.FolderForm',
        'directory.FolderWindow',
        'menus.FolderMenu',
        'menus.ContextMenu',
        'directory.ContextForm',
        'directory.ContextWindow',
        'directory.NewJobForm',
        'directory.SchedulerCacheWindow',
        'directory.ViewJobsForm',
        'menus.JobsMenu'


    ],

    refs: [
    {
        ref: 'dirTree',
        selector: 'viewport > directorypanel > directorytree'
    },
    {
        ref: 'dirProperties',
        selector: 'viewport > directorypanel > propertypanel'
    },
    {
        ref: 'mainContent',
        selector: 'viewport > centerpanel > contentpanel'
    },
    {
        ref: 'searchPanel',
        selector: 'viewport > centerpanel > searchpanel'
    },
    {
        ref: 'datalayerCombo',
        selector: 'datalayercombo'
    }],

    handleMetachange: function () {
        var me = this,
      store = grid.getStore(),
      columns = meta.columns;

        grid.metachange = true;
        grid.reconfigure(store, columns);
    },

    onBeforeLoad: function (store, operation, eOpts) {
        var me = this;
        if (operation.node !== null) {
            var operationNode = operation.node.data;
            var params = store.proxy.extraParams;
            if (operationNode.type !== null)
                params.type = operationNode.type;

            if (operationNode.record !== null && operationNode.record.Related !== null)
                params.related = operationNode.record.Related;

            if (operationNode.record !== null) {
                operationNode.leaf = false;

                if (operationNode.record.context)
                    params.contextName = operationNode.record.context;

                if (operationNode.record.BaseUrl)
                    params.baseUrl = operationNode.record.BaseUrl;

                if (operationNode.record.endpoint)
                    params.endpoint = operationNode.record.endpoint;

                if (operationNode.record.securityRole)
                    params.security = operationNode.record.securityRole;

                if (operationNode.text !== null)
                    params.text = operationNode.text;
            }
            else if (operationNode.property !== null) {
                operationNode.leaf = false;

                if (operationNode.property.context)
                    params.contextName = operationNode.property.context;

                if (operationNode.property.endpoint)
                    params.endpoint = operationNode.property.endpoint;

                if (operationNode.property.baseUrl)
                    params.baseUrl = operationNode.property.baseUrl;

                if (operationNode.text !== null)
                    params.text = operationNode.text;
            }
        }
    },

    //Site
    onNewOrEditFolder: function (item, e, eOpts) {
        var me = this;
        var state;
        var tree = me.getDirTree();
        var node = tree.getSelectedNode();

        var win = Ext.widget('folderWindow');
        var form = win.down('form');
        form.node = node;

        if (node.parentNode.data.record != '') {
            var selectedGroups = Ext.decode(node.data.record).groups;

            var storeObject = Ext.create('Ext.data.Store', { fields: ['groupId', 'groupName'] });

            Ext.each(selectedGroups, function (aRecord) {
                storeObject.add({ groupId: aRecord['groupId'], groupName: aRecord['groupName'] });
            }, this);

            form.getForm().findField('ResourceGroups').bindStore(storeObject);
        }

        if (item.itemId == 'editFolder' && node.data.record !== undefined) {
            win.title = 'Edit Folder';
            var state = 'edit';

            var record = Ext.decode(node.data.record);
            form.getForm().findField('displayName').setValue(record.folderName);
            //form.getForm().findField('ResourceGroups').setValue(record.groups);
        }
        else {
            var state = 'new';
            win.title = 'Add Folder';
        }

        win.on('save', function () {
            win.destroy();
            tree.view.refresh();
            tree.expandPath(tree.getRootNode().getPath());
            var detailGrid = tree.up('panel').down('propertypanel');
            detailGrid.setSource({});
        }, me);

        win.on('cancel', function () {
            win.destroy();
        }, me);

        form.getForm().findField('state').setValue(state);
        win.show();
    },

    //Delete folder
    onDeleteFolder: function (item, e, eOpts) {
        var me = this;
        var tree = this.getDirTree();
        var parent, path;
        var node = tree.getSelectedNode();

        Ext.Ajax.request({
            url: 'directory/Folder',
            form: me.form,
            method: 'POST',
            params: {
                record: node.data.record,
                state: 'delete'
            },
            success: function (response, request) {
                var objResponseText = Ext.decode(response.responseText);
                var parentNode = node.parentNode;
                parentNode.removeChild(node);
                tree.getSelectionModel().select(parentNode);
                tree.view.refresh();
                me.getDirTree().onReload();

                var currentNode;

                showDialog(400, 50, 'Alert', "Folder deleted successfully!", Ext.Msg.OK, null);
            },
            failure: function (response, request) {
                var objResponseText = Ext.decode(response.responseText);
                var userMsg = objResponseText['message'];
                var detailMsg = objResponseText['stackTraceDescription'];
                var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification' });
                Ext.ComponentQuery.query('#expValue', expPanel)[0].setValue(userMsg);
                Ext.ComponentQuery.query('#expValue2', expPanel)[0].setValue(detailMsg);
            }
        });
    },


    // onNewOrEditContext
    onNewOrEditContext: function (item, e, eOpts) {
        var me = this;
        var state;
        var tree = me.getDirTree();
        var node = tree.getSelectedNode();

        var win = Ext.widget('contextwindow');
        var form = win.down('form');
        form.node = node;

        var selectedGroups = Ext.decode(node.data.record).groups;

        var storeObject = Ext.create('Ext.data.Store', { fields: ['groupId', 'groupName'] });

        Ext.each(selectedGroups, function (aRecord) {
            storeObject.add({ groupId: aRecord['groupId'], groupName: aRecord['groupName'] });
        }, this);

        form.getForm().findField('ResourceGroups').bindStore(storeObject);

        if (item.itemId == 'editContext' && node.data.record !== undefined) {
            win.title = 'Edit Context';
            var state = 'edit';

            var record = Ext.decode(node.data.record);
            form.getForm().findField('displayName').setValue(record.displayName);
            form.getForm().findField('internalName').setValue(record.internalName);
            form.getForm().findField('internalName').readOnly = true;
            form.getForm().findField('description').setValue(record.description);
            form.getForm().findField('cacheDBConnStr').setValue(record.cacheConnStr);
        } else {
            var state = 'new';
            win.title = 'Add Context';
        }

        win.on('save', function () {
            win.destroy();
            tree.view.refresh();
            tree.expandPath(tree.getRootNode().getPath());
            var detailGrid = tree.up('panel').down('propertypanel');
            detailGrid.setSource({});
        }, me);

        win.on('cancel', function () {
            win.destroy();
        }, me);

        form.getForm().findField('state').setValue(state);
        win.show();
    },
    // end onNewOrEditContext

    //Delete context
    onDeleteContext: function (item, e, eOpts) {
        var me = this;
        var tree = this.getDirTree();
        var parent, path;
        var node = tree.getSelectedNode();

        Ext.Ajax.request({
            url: 'directory/Context',
            form: me.form,
            method: 'POST',
            params: {
                record: node.data.record,
                state: 'delete'
            },
            success: function (response, request) {
                var objResponseText = Ext.decode(response.responseText);
                var parentNode = node.parentNode;
                parentNode.removeChild(node);
                tree.getSelectionModel().select(parentNode);
                tree.view.refresh();
                me.getDirTree().onReload();
                showDialog(400, 50, 'Alert', "Context deleted successfully!", Ext.Msg.OK, null);
            },
            failure: function (response, request) {
                var objResponseText = Ext.decode(response.responseText);
                var userMsg = objResponseText['message'];
                var detailMsg = objResponseText['stackTraceDescription'];
                var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification' });
                Ext.ComponentQuery.query('#expValue', expPanel)[0].setValue(userMsg);
                Ext.ComponentQuery.query('#expValue2', expPanel)[0].setValue(detailMsg);
            }
        });
    },

    // Start onCacheUpdate
    onCacheUpdate: function (item, e, eOpts) {
        var me = this;
        var context, displayName, apps;
        var tree = me.getDirTree();
        var node = tree.getSelectedNode();
        var objResponseText;
        Ext.Ajax.request({
            url: 'directory/getnode',
            form: me.form,
            method: 'POST',
            params: {
                nodedetails: node.data,
                jsonData: node.data,
                contentType: 'application/json',
                type: 'ContextNode',
                'node': node.internalId,
                'parentnodeid': node.parentNode.internalId,
                'nodename': node.data.record.FolderName
            },
            success: function (response, request) {
                objResponseText = Ext.decode(response.responseText);
                var parentNode = node.parentNode;
                // parentNode.removeChild(node);
                // tree.getSelectionModel().select(parentNode);
                //tree.view.refresh();
                // me.getDirTree().onReload();

            },
            failure: function (response, request) {
                objResponseText = Ext.decode(response.responseText);
                var userMsg = objResponseText['message'];
                var detailMsg = objResponseText['stackTraceDescription'];
                var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification' });
                Ext.ComponentQuery.query('#expValue', expPanel)[0].setValue(userMsg);
                Ext.ComponentQuery.query('#expValue2', expPanel)[0].setValue(detailMsg);
            }
        })

        var cacheDBConnStr = 'Data Source={hostname\\dbInstance};Initial Catalog={dbName};User ID={userId};Password={password}';
        context = node.data.record.context;
        //apps = node.childNodes;

        var tempStore = Ext.create('Ext.data.Store', {
            fields: ['display'],
            storeId: 'apptempStore',
            //autoLoad:true,
            listeners: {
                'load': function () {
                    var arr = objResponseText;
                    Ext.each(arr, function (temp, index) {
                        tempStore.insert(index, { display: temp.record.DisplayName });


                    })
                }

            }


        });

        if (item.itemId == 'cacheupscreen' && node.data.record !== undefined) {

            displayNameCont = node.data.property["Display Name"];   //node.data.record.DisplayName


        }

        var win = Ext.widget('cachewindow');
        var form = win.down('form');


        win.on('cancel', function () {
            win.destroy();
        }, me);

        //var displayName1 = tempStore2.getAt(0);
        // var disp = displayName1.get('display');
        form.getForm().findField('displayName').setValue(displayNameCont);
        //form.getForm().findField('applications').setValue(disp);


        win.show();
    },

    //End onCacheUpdate

    //Start onEditrow
    onEditrow: function (item, e, eOpts) {
        var me = this;
        var context, displayName, apps;
        var tree = me.getDirTree();
        var node = tree.getSelectedNode();


        var win = Ext.widget('cachewindow');
        var form = win.down('form');

        win.on('cancel', function () {
            win.destroy();
        }, me);

        win.show();
    },
    //End onEditrow

    newOrEditScope: function (item, e, eOpts) {

        var me = this;
        var path, state, context, description, wintitle, displayName;
        var tree = me.getDirTree();
        var node = tree.getSelectedNode();
        var cacheDBConnStr = 'Data Source={hostname\\dbInstance};Initial Catalog={dbName};User ID={userId};Password={password}';
        context = node.data.record.context;

        if (node.parentNode) {
            path = node.internalId;
        } else {
            path = '';
        }

        var conf = {
            id: 'tab-' + node.data.id,
            title: wintitle,
            iconCls: 'tabsScope'
        };

        var win = Ext.widget('scopewindow', conf);
        var form = win.down('form');
        form.node = node;

        if (item.itemId == 'editfolder' && node.data.record !== undefined) {
            var name = node.data.record.Name;
            var displayName = node.data.record.DisplayName;
            var description = node.data.record.Description;
            var internalName = node.data.record.Name;
            win.title = 'Edit Scope';
            var state = 'edit';

            if (node.data.record.Configuration != null && node.data.record.Configuration.AppSettings != null &&
				node.data.record.Configuration.AppSettings.Settings != null) {
                Ext.each(node.data.record.Configuration.AppSettings.Settings, function (settings, index) {
                    if (settings.Key == "iRINGCacheConnStr") {
                        form.getForm().findField('cacheDBConnStr').setValue(settings.Value);
                    }
                });
            }
            form.getForm().findField('internalName').setReadOnly(true);

        } else {
            var name = '';
            var state = 'new';
            win.title = 'Add Scope';
        }

        win.on('save', function () {
            win.destroy();
            tree.view.refresh();
            tree.expandPath(tree.getRootNode().getPath());
            var detailGrid = tree.up('panel').down('propertypanel'); //.down('gridview');
            detailGrid.setSource({});
        }, me);

        win.on('cancel', function () {
            win.destroy();
        }, me);
        if (utilsObj.isSecEnable == "False") {
            form.getForm().findField('permissions').hide();
        }
        form.getForm().findField('path').setValue(path);
        form.getForm().findField('state').setValue(state);
        form.getForm().findField('oldContext').setValue(context);
        form.getForm().findField('description').setValue(description);
        form.getForm().findField('name').setValue(name);
        form.getForm().findField('internalName').setValue(internalName);
        form.getForm().findField('displayName').setValue(displayName);
        form.getForm().findField('contextName').setValue(name);
        form.getForm().findField('permissions').setValue(node.data.record.PermissionGroup);

        win.show();
    },

    deleteScope: function (item, e, eOpts) {
        var me = this;
        var tree = this.getDirTree();
        var parent, path;
        var node = tree.getSelectedNode();

        Ext.Ajax.request({
            url: 'directory/DeleteScope', //'directory/deleteEntry', 
            method: 'POST',
            params: {
                'nodeid': node.data.id
            },
            success: function (response, request) {
                var resp = Ext.decode(response.responseText);
                if (resp.success) {
                    var parentNode = node.parentNode;
                    parentNode.removeChild(node);
                    tree.getSelectionModel().select(parentNode);
                    tree.view.refresh();
                } else {
                    var userMsg = resp['message'];
                    var detailMsg = resp['stackTraceDescription'];
                    var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification' });
                    Ext.ComponentQuery.query('#expValue', expPanel)[0].setValue(userMsg);
                    Ext.ComponentQuery.query('#expValue2', expPanel)[0].setValue(detailMsg);
                }

                //tree.onReload();
            },
            failure: function (response, request) {
                var resp = Ext.decode(response.responseText);
                var userMsg = resp['message'];
                var detailMsg = resp['stackTraceDescription'];
                var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification' });
                Ext.ComponentQuery.query('#expValue', expPanel)[0].setValue(userMsg);
                Ext.ComponentQuery.query('#expValue2', expPanel)[0].setValue(detailMsg);
            }
        });
    },

    newOrEditEndpoint: function (item, e, eOpts) {
        var me = this;
        var name, displayName, description, datalayer, assembly, application, baseurl, showconfig, endpoint, wintitle, state, path, context;
        var tree = me.getDirTree();
        var node1 = tree.getSelectedNode();
        var node = tree.store.getNodeById(node1.internalId);
        var cacheImportURI = '';
        var cacheTimeout = '';
        var context = node.data.parentId; //node.parentNode.data.text;//node.parentNode.data.record.Name;//node.data.record.ContextName;

        var conf = {
            id: 'newwin-' + node.data.id,
            title: '',
            iconCls: 'tabsApplication',
            node: node,
            modal: true
        };

        var win = Ext.widget('applicationwindow', conf);

        var form = win.down('form');

        if (item.itemId == 'editendpoint') {
            var name = node.data.record.Name;
            var displayName = node.data.record.DisplayName;
            var description = node.data.record.Description;
            var datalayer = node.data.record.DataLayer;
            var assembly = node.data.record.Assembly;
            var application = name;
            var wintitle = 'Edit Application';
            var endpoint = node.data.record.Name;
            var state = 'edit';
            var cacheImportURI = node.data.record.CacheImportURI;
            var cacheTimeout = node.data.record.CacheTimeout;
            var permission = node.data.record.PermissionGroups;
            var internalName = node.data.record.Name;

            form.getForm().findField('internalName').setReadOnly(true);
        } else {
            var wintitle = 'Add Application';
            var state = '';
            var application = '';
            var context = node.data.text;
            var path = node.internalId;
        }

        win.setTitle(wintitle);

        win.on('save', function () {
            win.close();
            tree.view.refresh();
            var detailGrid = tree.up('panel').down('propertypanel'); //.down('gridview');
            detailGrid.setSource({});
        }, me);

        win.on('Cancel', function () {
            win.close();
        }, me);

        var dlCmb = me.getDatalayerCombo();

        dlCmb.on('select', function (combo, records, eopts) {
            if (records !== null && node.data.record !== null) {
                form.getForm().findField('assembly').setValue(records[0].data.assembly);
            }
        }, me);

        dlCmb.on('afterrender', function (combo, eopts) {
            if (assembly != undefined && assembly !== '') {
                combo.setValue(assembly.substring(assembly.indexOf(',') + 2));
            }
        }, me);

        //        if (utilsObj.isSecEnable == "False") {
        //            form.getForm().findField('permissions').hide();
        //        }
        form.node = node1;
        form.getForm().findField('path').setValue(path);
        form.getForm().findField('state').setValue(state);
        form.getForm().findField('scope').setValue(context);
        form.getForm().findField('oldAssembly').setValue(assembly);
        form.getForm().findField('name').setValue(name);
        form.getForm().findField('displayName').setValue(displayName);
        form.getForm().findField('internalName').setValue(internalName);
        form.getForm().findField('description').setValue(description);
        form.getForm().findField('context').setValue(name);
        form.getForm().findField('assembly').setValue(assembly);
        form.getForm().findField('application').setValue(application);
        form.getForm().findField('cacheImportURI').setValue(cacheImportURI);
        form.getForm().findField('cacheTimeout').setValue(cacheTimeout);
        //        form.getForm().findField('permissions').setValue(permission);
        win.show();
    },

    deleteEndpoint: function (item, e, eOpts) {
        var me = this;

        var tree = me.getDirTree();
        var node = tree.getSelectedNode();
        Ext.Ajax.request({
            url: 'directory/deleteapplication',
            method: 'POST',
            params: {
                nodeid: node.data.id
            },
            success: function (response, request) {
                var resp = Ext.decode(response.responseText);
                if (resp.success) {
                    var parentNode = node.parentNode;
                    parentNode.removeChild(node);
                    tree.getSelectionModel().select(parentNode);
                    tree.view.refresh();
                } else {
                    var userMsg = resp['message'];
                    var detailMsg = resp['stackTraceDescription'];
                    var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification' });
                    Ext.ComponentQuery.query('#expValue', expPanel)[0].setValue(userMsg);
                    Ext.ComponentQuery.query('#expValue2', expPanel)[0].setValue(detailMsg);
                }

                //tree.onReload();
            },
            failure: function (response, request) {
                var resp = Ext.decode(response.responseText);
                var userMsg = resp['message'];
                var detailMsg = resp['stackTraceDescription'];
                var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification' });
                Ext.ComponentQuery.query('#expValue', expPanel)[0].setValue(userMsg);
                Ext.ComponentQuery.query('#expValue2', expPanel)[0].setValue(detailMsg);
            }
        });
    },

    onDeleteApplication: function (item, e, eOpts) {
        var tree = this.getDirTree();
        var node = tree.getSelectedNode();

        Ext.Ajax.request({
            url: 'directory/Application',
            method: 'POST',
            params: {
                nodeId: node.data.id,
                parentNodeId: node.data.parentId,
                state: "delete"
            },
            success: function (response, request) {
                var resp = Ext.decode(response.responseText);
                if (resp.success) {
                    var parentNode = node.parentNode;
                    parentNode.removeChild(node);
                    tree.getSelectionModel().select(parentNode);
                    tree.view.refresh();
                }
            },
            failure: function (response, request) {
                var resp = Ext.decode(response.responseText);
                var userMsg = resp['message'];
                var detailMsg = resp['stackTraceDescription'];
                var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification' });
                Ext.ComponentQuery.query('#expValue', expPanel)[0].setValue(userMsg);
                Ext.ComponentQuery.query('#expValue2', expPanel)[0].setValue(detailMsg);
            }
        });
    },

    onNewDataLayer: function (item, e, eOpts) {
        var me = this;
        var tree = me.getDirTree();
        var node = tree.getSelectedNode();
        conf = {
            id: 'tab-' + node.data.id,
            title: 'Add Data Layer'
        };
        var win = Ext.widget('datalayerwindow', conf);

        var form = win.down('form');
        form.getForm().findField('state').setValue('new');

        win.on('Save', function () {
            tree.store.load();
            if (node.get('expanded') === false)
                node.expand();
        }, me);

        win.on('Cancel', function () {
            win.close();
        }, me);

        win.show();
    },

    onRegenerateAll: function (item, e, eOpts) {
        var me = this;
        Ext.Ajax.request({
            url: 'AdapterManager/RegenAll', //'directory/RegenAll',
            method: 'GET',
            success: function (result, request) {
                var responseObj = Ext.decode(result.responseText);
                var msg = '';
                for (var i = 0; i < responseObj.StatusList.length; i++) {
                    var status = responseObj.StatusList[i];
                    if (msg !== '') {
                        msg += '\r\n';
                    }
                    msg += status.Identifier + ':\r\n';
                    for (var j = 0; j < status.Messages.length; j++) {
                        msg += '    ' + status.Messages[j] + '\r\n';
                    }
                }
                Ext.widget('messagepanel', { title: 'NHibernate Regeneration Result', msg: msg });
                //showDialog(600, 340, 'NHibernate Regeneration Result', msg, Ext.Msg.OK, null);
            },
            failure: function (result, request) {
                var msg = result.responseText;
                //showDialog(500, 240, 'NHibernate Regeneration Error', msg, Ext.Msg.OK, null);
                Ext.widget('messagepanel', { title: 'NHibernate Regeneration Error', msg: msg });
            }
        });
    },

    onShowDataGrid: function (item, e, eOpts) {
        var me = this;
        var tree = this.getDirTree();
        var node = tree.getSelectedNode();
        var content = me.getMainContent();
        var contextName = node.parentNode.parentNode.parentNode.data.property['Internal Name'];
        var endpointName = node.parentNode.parentNode.data.property['Internal Name'];

        var graph = node.data.text;
        var title = contextName + '.' + endpointName + '.' + graph;
        var gridPanel = content.down('dynamicgrid[title=' + title + ']');
        var checkboxForGrid = Ext.getElementById('gridCheckbox').checked;
        if (!gridPanel) {

            content.getEl().mask("Loading...", "x-mask-loading");
            gridPanel = Ext.widget('dynamicgrid', { 'title': title });

            gridStore = gridPanel.getStore();
            var gridProxy = gridStore.getProxy();

            gridStore.on('beforeload', function (store, action) {
                var params = store.proxy.extraParams;
                if (Ext.getElementById('gridCheckbox').checked == false) {
                    params.start = (store.currentPage - 1) * store.pageSize;
                    params.limit = store.pageSize;
                }

                params.app = endpointName; //node.parentNode.parentNode.data.property.Name;
                params.scope = contextName; //node.parentNode.parentNode.parentNode.data.property.Name ;
                params.graph = graph;
                //                if (checkboxForGrid) {
                //                    params.limit = 100000;
                //                    store.pageSize = 100;
                //                    //store.buffered = true;
                //                    //store.leadingBufferZone = 300;
                //                    //gridPanel.verticalScrollerType = 'paginggridscroller';
                //                    //gridPanel.loadMask = true;
                //                    if (gridPanel.dockedItems.length >= 2)
                //                        gridPanel.dockedItems.removeAt(1);
                //                }
            }, me);

            gridProxy.on('exception', function (proxy, response, operation) {
                content.getEl().unmask();
                gridPanel.destroy();
                var responseObj = Ext.JSON.decode(response.responseText);
                var userMsg = responseObj['message'];
                var detailMsg = responseObj['stackTraceDescription'];
                var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification' });
                Ext.ComponentQuery.query('#expValue', expPanel)[0].setValue(userMsg);
                Ext.ComponentQuery.query('#expValue2', expPanel)[0].setValue(detailMsg);
                //Ext.widget('messagepanel', { title: 'Error', msg: msg });
            }, me);
            gridStore.load({
                callback: function (records, response) {
                    if (records != undefined) {
                        if (records[0]) {
                            gridPanel.reconfigure(gridStore, records[0].store.proxy.reader.metaData.columns);
                            //content.getEl().unmask();
                        } else {
                            if (response) {
                                //showDialog(200, 50, 'Warning', 'Authentication failure', Ext.Msg.OK, null);
                                //showDialog(500, 300, 'Error', response.response.responseText, Ext.Msg.OK, null);
                            }
                            return true;
                        }
                    }

                }
            });
            content.getEl().unmask();
            content.add(gridPanel);
        }
        content.setActiveTab(gridPanel);
    },

    onRefreshFacade: function (item, e, eOpts) {
        var me = this;
        var tree = this.getDirTree(),
      node = tree.getSelectedNode();

        tree.getEl().mask('Loading', 'x-mask-loading');
        Ext.Ajax.request({
            url: 'facade/refreshFacade',
            method: 'POST',
            params: {
                scope: node.data.id
            },
            success: function (response, request) {
                tree.onReload();
                tree.getEl().unmask();
            },
            failure: function (response, request) {
                tree.getEl().unmask();
                Ext.widget('messagepanel', { title: 'Warning', msg: 'Error Refreshing Facade!!!' });
                //showDialog(400, 300, 'Warning', 'Error Refreshing Facade!!!', Ext.Msg.OK, null);
            }
        });
    },

    onConfigureEndpoint: function (item, e, eOpts) {
        var me = this;
        var node = me.getDirTree().getSelectedNode();
        var datalayer = node.data.record.DataLayer;

        switch (datalayer) {
            case 'NHibernateLibrary':
                me.application.fireEvent('nhconfig', node);
                break;
            case 'SqlDataLayer':
                me.application.fireEvent('sqlconfig', node);
                break;
            default:
                //Ext.widget('messagepanel', { title: 'Warning', msg: 'Datalayer ' + datalayer + ' is not configurable...' }); //showDialog(300, 300, 'Warning', 'Datalayer ' + datalayer + ' is not configurable...', Ext.msg.OK, null);
                me.application.fireEvent('nhconfig', node);
                break;
        }
    },

    showContextMenu: function (dataview, record, item, index, e, eOpts) {
        var me = this;
        var tree = me.getDirTree();
        var node = tree.getSelectedNode();
        var obj = record.data;
        if (utilsObj.isSecEnable == "False" || (utilsObj.isSecEnable == "True" && utilsObj.isAdmin == "True")) {
            if (obj.type === "ScopesNode") {
                var scopesMenu = Ext.widget('scopesmenu');
                scopesMenu.showAt(e.getXY());
            } else if (obj.type === "ScopeNode") {
                var scopeMenu = Ext.widget('scopemenu');
                scopeMenu.showAt(e.getXY());
            } else if (obj.type === "SiteNode") {
                var siteMenu = Ext.widget('sitemenu');
                siteMenu.showAt(e.getXY());
            } else if (obj.type === "FolderNode") {
                var folderMenu = Ext.widget('foldermenu');
                folderMenu.showAt(e.getXY());
            } else if (obj.type === "ContextNode") {
                var contextMenu = Ext.widget('contextmenu');
                contextMenu.showAt(e.getXY());
            } else if (obj.type === "ApplicationNode") {
                var applicationMenu = Ext.widget('applicationmenu');
                applicationMenu.showAt(e.getXY());
            } else if (obj.type === "DataObjectNode") {
                var appDataMenu = Ext.widget('appdatamenu');
                appDataMenu.showAt(e.getXY());
            } else if (obj.type === "ValueListsNode") {
                var valueListsMenu = Ext.widget('valuelistsmenu');
                valueListsMenu.showAt(e.getXY());
            } else if (obj.type === "ValueListNode") {
                var valueListMenu = Ext.widget('valuelistmenu');
                valueListMenu.showAt(e.getXY());
            } else if (obj.type === "ListMapNode") {
                var valueListMapMenu = Ext.widget('valuelistmapmenu');
                valueListMapMenu.showAt(e.getXY());
            } else if (obj.type === "GraphsNode") {
                var graphsMenu = Ext.widget('graphsmenu');
                graphsMenu.showAt(e.getXY());
            } else if (obj.type === "GraphNode") {
                var graphMenu = Ext.widget('graphmenu');
                graphMenu.showAt(e.getXY());
            } else if (obj.type === "DataObjectsNode") {
                var graphMenu = Ext.widget('appdatarefreshmenu');

                //                if (node.data.property["Data Mode"] == "Live") {
                //                    if (node.parentNode.data.property["LightweightDataLayer"] == "No") {
                //                        graphMenu.items.map['switchToCached'].setVisible(true);
                //                        graphMenu.items.map['switchToLive'].setVisible(false);
                //                        graphMenu.items.map['showCacheInfo'].setVisible(false);
                //                    }
                //                    	
                //                } else if (node.parentNode.data.property["LightweightDataLayer"] == "No") {
                //                    graphMenu.items.map['switchToCached'].setVisible(false);
                //                    graphMenu.items.map['switchToLive'].setVisible(true);
                //                    graphMenu.items.map['showCacheInfo'].setVisible(true);
                //                    	
                //                } else if (node.parentNode.data.property["LightweightDataLayer"] == "Yes") {
                //                    graphMenu.items.map['switchToCached'].setVisible(false);
                //                    graphMenu.items.map['switchToLive'].setVisible(false);
                //                    graphMenu.items.map['showCacheInfo'].setVisible(true);
                //                }


                if (node.data.property["Data Mode"] == "Live") {
                    if (node.parentNode.data.property["LightweightDataLayer"] == "No") {
                        graphMenu.items.map['switchToCached'].setVisible(true);
                        graphMenu.items.map['switchToLive'].setVisible(false);
                        graphMenu.items.map['showCacheInfo'].setVisible(false);
                    } else if (node.parentNode.data.property["LightweightDataLayer"] == "Yes") {
                        graphMenu.items.map['switchToCached'].setVisible(true);
                        graphMenu.items.map['switchToLive'].setVisible(false);
                        graphMenu.items.map['showCacheInfo'].setVisible(false);
                    }
                } else if (node.data.property["Data Mode"] == "Cache") {
                    if (node.parentNode.data.property["LightweightDataLayer"] == "Yes") {
                        graphMenu.items.map['switchToCached'].setVisible(false);
                        graphMenu.items.map['switchToLive'].setVisible(true);
                        graphMenu.items.map['showCacheInfo'].setVisible(true);
                    }
                    else if (node.parentNode.data.property["LightweightDataLayer"] == "No") {
                        graphMenu.items.map['switchToCached'].setVisible(false);
                        graphMenu.items.map['switchToLive'].setVisible(true);
                        graphMenu.items.map['showCacheInfo'].setVisible(true);
                    }
                }
                graphMenu.showAt(e.getXY());
            } else if (obj.type === "DataPropertyNode") {
                if (obj.property) {
                    if (obj.property.isVirtual == 'True') {
                        var virtualpropertymenu = Ext.widget('virtualpropertymenu');
                        virtualpropertymenu.showAt(e.getXY());
                    }
                }
            }
        }
        else if ((utilsObj.isSecEnable == "True" && utilsObj.isAdmin == "False")) {
            if (obj.type === "ApplicationNode") {
                var applicationMenu = Ext.widget('applicationmenu');
                for (var i = 0; i < 6; i++) {
                    applicationMenu.items.items[i].hide();
                }
                applicationMenu.showAt(e.getXY());
            }
            else if (obj.type === "ScopesNode") {
                var scopesMenu = Ext.widget('scopesmenu');
                scopesMenu.items.items[0].hide();
                scopesMenu.showAt(e.getXY());
            }
            else if (obj.type === "DataObjectNode") {
                var appDataMenu = Ext.widget('appdatamenu');
                appDataMenu.showAt(e.getXY());
            }
            else if (obj.type === "ValueListsNode") {
                var valueListsMenu = Ext.widget('valuelistsmenu');
                valueListsMenu.showAt(e.getXY());
            } else if (obj.type === "ValueListNode") {
                var valueListMenu = Ext.widget('valuelistmenu');
                valueListMenu.showAt(e.getXY());
            } else if (obj.type === "ListMapNode") {
                var valueListMapMenu = Ext.widget('valuelistmapmenu');
                valueListMapMenu.showAt(e.getXY());
            } else if (obj.type === "GraphsNode") {
                var graphsMenu = Ext.widget('graphsmenu');
                graphsMenu.showAt(e.getXY());
            } else if (obj.type === "GraphNode") {
                var graphMenu = Ext.widget('graphmenu');
                graphMenu.showAt(e.getXY());
            } else if (obj.type === "DataObjectsNode") {
                var graphMenu = Ext.widget('appdatarefreshmenu');

                if (node.data.property["Data Mode"] == "Live") {
                    if (node.parentNode.data.property["LightweightDataLayer"] == "No") {
                        graphMenu.items.map['switchToCached'].setVisible(true);
                        graphMenu.items.map['switchToLive'].setVisible(false);
                        graphMenu.items.map['showCacheInfo'].setVisible(false);
                    }
                    //graphMenu.items.map['refreshCacheId'].setVisible(false);	
                    //graphMenu.items.map['importCacheId'].setVisible(false);	
                } else if (node.parentNode.data.property["LightweightDataLayer"] == "No") {
                    graphMenu.items.map['switchToCached'].setVisible(false);
                    graphMenu.items.map['switchToLive'].setVisible(true);
                    graphMenu.items.map['showCacheInfo'].setVisible(true);
                    //graphMenu.items.map['refreshCacheId'].setVisible(true);	
                    //graphMenu.items.map['importCacheId'].setVisible(true);	
                }
                else if (node.parentNode.data.property["LightweightDataLayer"] == "Yes") {
                    graphMenu.items.map['switchToCached'].setVisible(false);
                    graphMenu.items.map['switchToLive'].setVisible(false);
                    graphMenu.items.map['showCacheInfo'].setVisible(true);
                }
                graphMenu.showAt(e.getXY());
            } else if (obj.type === "DataPropertyNode") {
                if (obj.property) {
                    if (obj.property.isVirtual == 'True') {
                        var virtualpropertymenu = Ext.widget('virtualpropertymenu');
                        virtualpropertymenu.showAt(e.getXY());
                    }
                }
            }
        }


    },

    onAppDataRefreshClick: function (item, e, eOpts) {

        var me = this;
        var tree = me.getDirTree();
        var node = tree.getSelectedNode();
        var store = tree.store; //me.store;

        if (!node)
            node = me.getRootNode();
        var state = tree.getState();
        var nodeState = '/Scopes/' + node.internalId;
        tree.body.mask('Loading...', 'x-mask-loading');
        var storeProxy = store.getProxy();
        store.getProxy().extraParams.refresh = true;
        storeProxy.on('exception', function (proxy, response, operation) {
            var responseObj = Ext.JSON.decode(response.responseText);
            var userMsg = responseObj['message'];
            var detailMsg = responseObj['stackTraceDescription'];
            var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification' });
            Ext.ComponentQuery.query('#expValue', expPanel)[0].setValue(userMsg);
            Ext.ComponentQuery.query('#expValue2', expPanel)[0].setValue(detailMsg);
            //Ext.widget('messagepanel', { title: 'Error', msg: msg });
            //showDialog(500, 300, 'Error', msg, Ext.Msg.OK, null);
        }, me);
        store.load({
            node: node,
            callback: function (records, options, success) {
                tree.body.unmask();
                tree.view.refresh();
                tree.store.getProxy().extraParams.refresh = false;

            }
        });
    },

    onItemDblClick: function (dataview, record, item, index, e, eOpts) {
        var me = this;

        if (record.data.type == 'GraphNode') {
            me.application.fireEvent('opengraphmap', me);
            me.getSearchPanel().expand();
        }
        else if (record.data.type == 'DataObjectNode') {
            me.onShowDataGrid(item, e, eOpts);
        }
    },

    /*onTextfieldBlur: function (component, e, eOpts) {
	
    if (component.dataIndex != undefined) {
			
    var me = this;
    var gridPanel = me.getMainContent().activeTab;
    var gridStore = gridPanel.getStore();
    var gridProxy = gridStore.getProxy();
    gridStore.currentPage = 1;
    gridProxy.on('exception', function (proxy, response, operation) {
    gridPanel.destroy();
    var rtext = response.responseText;
    if (rtext != undefined) {
    var error = 'SUCCESS = FALSE';
    var index = rtext.toUpperCase().indexOf(error);
    msg = rtext;
    Ext.widget('messagepanel', { title: 'Error', msg: msg });
    //showDialog(500, 300, 'Error', msg, Ext.Msg.OK, null);
    }
    }, me);
    gridStore.load({
    callback: function (records, response) {
    alert('load in textBlur...');
    if (records != undefined && records[0] != undefined && records[0].store.proxy.reader.metaData) {
    //gridPanel.reconfigure(gridStore, records[0].store.proxy.reader.metaData.columns);
    gridPanel.reconfigure(gridStore);
    }

    }
    });
    }
    },*/

    onFileUpload: function (item, e, eOpts) {
        var me = this;
        var win = Ext.widget('fileuploadwindow');
        var form = win.down('form');
        var tree = me.getDirTree();
        var node = tree.getSelectedNode();

        var formRecord = {
            scope: node.parentNode.data.text,
            application: node.data.text
        };

        form.getForm().setValues(formRecord);

        win.on('Save', function () {
            win.destroy();
            Ext.example.msg('Notification', 'File Uploaded successfully!');
        }, me);

        win.on('reset', function () {
            win.destroy();
        }, me);

        win.show();
    },

    onFileDownload: function (item, e, eOpts) {
        var me = this;
        var tree = me.getDirTree();
        var node = tree.getSelectedNode();
        var win = Ext.widget('filedownloadwindows');
        var scope = node.parentNode.data.text;
        var app = node.data.text;
        var form = win.down('downloadform');
        var formRecord = {
            scope: scope,  //node.parentNode.data.text,
            application: app
        };
        var grid = form.down('grid');
        var store = grid.getStore();
        var storeProxy = store.getProxy();
        form.getForm().setValues(formRecord);
        store.on('beforeload', function (store, action) {
            var params = storeProxy.extraParams;
            params.scope = scope;
            params.application = app;
        }, me);
        storeProxy.on('exception', function (proxy, response, operation) {
            var msg = Ext.JSON.decode(response.responseText).message;
            Ext.widget('messagepanel', { title: 'Error', msg: msg });
            //showDialog(500, 300, 'Error', msg, Ext.Msg.OK, null);
        }, me);
        /*store.on('exception',function( store, records, options ){
        alert('exception occeured...');
        },me);
        */
        store.load({
            callback: function (records, options, success) {
                if (store.data.length == 0) {
                    store.add({ 'File': 'No Record found to download' });
                    grid.reconfigure(store);
                }
            }
        });
        win.show();
    },

    onAddSettings: function (button, e, eOpts) {
        var me = this;
        var nameID;
        var valueID;
        var myFieldSet = Ext.getCmp('settingfieldset');
        if (myFieldSet.items.items.length >= 1) {
            var nameID = 'key' + (myFieldSet.items.items.length + 1);
            var valueID = 'value' + (myFieldSet.items.items.length + 1);
        } else {
            var nameID = 'key1';
            var valueID = 'value1';
        }
        var newSetting = me.addSettings("", "", nameID, valueID);
        myFieldSet.add(newSetting);
        myFieldSet.doLayout();
        myFieldSet.items.items[myFieldSet.items.length - 1].items.items[0].allowBlank = false;

    },

    onApplicationFormAfterRender: function (component, eOpts) {
        //Adding settings into setting Field set.
        var key = '';
        var value = '';
        var me = this;
        var tree = me.getDirTree();
        var node = tree.getSelectedNode();
        if (node.data.record != null) {
            if (node.data.record.Configuration != null) {
                if (node.data.record.Configuration.AppSettings != null) {
                    if (node.data.record.Configuration.AppSettings.Settings != null) {
                        for (var i = 0; i < node.data.record.Configuration.AppSettings.Settings.length; i++) {
                            if (node.data.record.Configuration.AppSettings.Settings[i].Key != "iRINGCacheConnStr") {
                                key = node.data.record.Configuration.AppSettings.Settings[i].Key;
                                value = node.data.record.Configuration.AppSettings.Settings[i].Value;
                                var newSetting = me.addSettings(key, value, ('key' + i), ('value' + i));
                                newSetting[0].items[0].allowBlank = false;
                                if (component.items.map['settingfieldset'])
                                    component.items.map['settingfieldset'].add(newSetting);
                            }
                        }
                    }
                }
            }
        } // End of adding settings.
    },

    onRefreshDataObjectCache: function (item, e, eOpts) {
        var me = this;
        var tree = me.getDirTree();
        var node = tree.getSelectedNode();

        Ext.Ajax.request({
            url: 'AdapterManager/RefreshObjectCache',
            method: 'POST',
            timeout: 3600000,  // 1 hour
            params: {
                'nodeid': node.data.id, //node.attributes.id,
                'objectType': node.data.text//node.text
            },
            success: function (response, request) {
                var responseObj = Ext.decode(response.responseText);
                if (responseObj.Level == 0) {
                    Ext.widget('messagepanel', { title: 'Refresh Cache Result', msg: 'Object cache refreshed successfully.' });
                    //showDialog(450, 100, 'Refresh Cache Result', 'Object cache refreshed successfully.', Ext.Msg.OK, null);
                }
                else {
                    Ext.widget('messagepanel', { title: 'Refresh Cache Error', msg: responseObj.Messages.join() });
                    //showDialog(500, 160, 'Refresh Cache Error', responseObj.Messages.join(), Ext.Msg.OK, null);
                }
            },
            failure: function (response, request) {
                var responseObj = Ext.decode(response.responseText);
                Ext.widget('messagepanel', { title: 'Refresh Cache Error', msg: responseObj.Messages.join() });
                //showDialog(500, 160, 'Refresh Cache Error', responseObj.Messages.join(), Ext.Msg.OK, null);
            }
        })
    },

    onRefreshCache: function (item, e, eOpts) {
        var me = this;
        var tree = me.getDirTree();
        var node = tree.getSelectedNode();
        Ext.Ajax.request({
            url: 'AdapterManager/RefreshCache',
            method: 'POST',
            timeout: 28800000,  // 8 hours
            params: {
                'nodeid': node.data.id//node.attributes.id
            },
            success: function (response, request) {
                var responseObj = Ext.decode(response.responseText);

                if (responseObj.Level == 0) {
                    Ext.widget('messagepanel', { title: 'Refresh Cache Result', msg: 'Cache refreshed successfully.' });
                    //showDialog(450, 100, 'Refresh Cache Result', 'Cache refreshed successfully.', Ext.Msg.OK, null);
                }
                else {
                    Ext.widget('messagepanel', { title: 'Refresh Cache Error', msg: responseObj.Messages.join() });
                    //showDialog(500, 160, 'Refresh Cache Error', responseObj.Messages.join(), Ext.Msg.OK, null);
                }
            },
            failure: function (response, request) {
                var responseObj = Ext.decode(response.responseText);
                Ext.widget('messagepanel', { title: 'Refresh Cache Error', msg: responseObj.Messages.join() });
                //showDialog(500, 160, 'Refresh Cache Error', responseObj.Messages.join(), Ext.Msg.OK, null);
            }
        })
    },

    onShowCacheInfo: function (item, e, eOpts) {
        var me = this;
        var win = Ext.widget('importcachewindow');
        var form = win.down('form');
        var tree = me.getDirTree();
        var node = tree.getSelectedNode();
        form.node = node;
        form.display();
        win.on('Save', function () {
            win.destroy();
        }, me);

        win.on('reset', function () {
            win.destroy();
        }, me);
        win.show();
    },

    onDeleteCache: function (item, e, eOpts) {
        var me = this;
        var tree = me.getDirTree();
        var node = tree.getSelectedNode();
        Ext.Ajax.request({
            url: 'AdapterManager/DeleteCache',
            method: 'POST',
            timeout: 120000,  // 2 minutes
            params: {
                'nodeid': node.data.id//node.attributes.id
            },
            success: function (response, request) {
                var responseObj = Ext.decode(response.responseText);

                if (responseObj.Level == 0) {
                    Ext.widget('messagepanel', { title: 'Delete Cache Result', msg: 'Cache deleted successfully.' });
                    //showDialog(450, 100, 'Delete Cache Result', 'Cache deleted successfully.', Ext.Msg.OK, null);
                }
                else {
                    Ext.widget('messagepanel', { title: 'Delete Cache Error', msg: responseObj.Messages.join() });
                    //showDialog(500, 160, 'Delete Cache Error', responseObj.Messages.join(), Ext.Msg.OK, null);
                }
            },
            failure: function (response, request) {
                var responseObj = Ext.decode(response.responseText);
                Ext.widget('messagepanel', { title: 'Delete Cache Error', msg: responseObj.Messages.join() });
                //showDialog(500, 160, 'Delete Cache Error', responseObj.Messages.join(), Ext.Msg.OK, null);
            }
        })
    },

    onAddVirtualProperty: function (item, e, eOpts) {
        var me = this;
        var win = Ext.widget('virtualpropertywindow');
        var form = win.down('form');
        var grid = form.down('grid');
        grid.getStore().removeAll();
        var tree = me.getDirTree();
        var node = tree.getSelectedNode();
        var properties = [];
        var formRecord = {
            objectName: node.data.text,
            scope: node.data.id.split('/')[0],
            app: node.data.id.split('/')[1]
        };
        var ii = 0;
        node.eachChild(function (child) {
            properties.push([ii, child.data.text, child.data.property.Name]);
            ii++;
        });

        var mapCombo = grid.down('#propertyNameCmb').getEditor(); //form.down('#propertyNameCmb');
        mapCombo.store = Ext.create('Ext.data.SimpleStore', {
            fields: ['value', 'text', 'name'],
            autoLoad: true,
            data: properties
        });

        form.getForm().setValues(formRecord);

        win.on('Save', function () {
            win.destroy();
        }, me);

        win.on('reset', function () {
            win.destroy();
        }, me);

        win.show();
    },

    onSaveVirtualProperties: function (button, e, eOpts) {
        var me = this;
        var form = button.up('form').getForm();
        var win = button.up('window');
        var objectName = form.findField('objectName').getValue();
        var propertyName = form.findField('propertyName').getValue();
        var delimeter = form.findField('delimeter').getValue();
        var scope = form.findField('scope').getValue();
        var app = form.findField('app').getValue();
        var oldPropertyName = form.findField('oldPropertyName').getValue();
        var properties;
        Ext.Ajax.request({
            url: 'AdapterManager/VirtualProperties',
            timeout: 600000,
            params: {
                scope: scope,
                app: app
            },
            success: function (response, request) {
                var folder = {};
                var flag = true;
                var res = Ext.decode(response.responseText);
                if (res.virtualProperties.length >= 1) { //Adding if few properties alrady exist.
                    properties = res;
                    for (var i = 0; i < res.virtualProperties.length; i++) {
                        if (res.virtualProperties[i].propertyName == oldPropertyName) {
                            folder = res.virtualProperties[i];
                            flag = false;
                        }
                    }
                } else { //Adding new virtual property if existing properties are zero.
                    var virtualProperty = {};
                    virtualProperty.virtualProperties = [];
                    properties = virtualProperty;
                }

                folder.objectName = objectName;
                folder.delimiter = delimeter;
                folder.virtualPropertyValues = [];
                folder.columnName = propertyName;
                folder.propertyName = propertyName;
                folder.dataType = 11;
                folder.dataLength = 0;
                folder.isNullable = true;
                folder.keyType = 1;
                folder.showOnIndex = false;
                folder.numberOfDecimals = 0;
                folder.isReadOnly = false;
                folder.showOnSearch = false;
                folder.isHidden = false;
                folder.description = null;
                folder.aliasDictionary = null;
                folder.referenceType = null;
                folder.isVirtual = true;
                //virtualProperty.virtualProperties.push(folder);
                if (flag) {
                    var flagForExisting = true;
                    for (var k = 0; k < properties.virtualProperties.length; k++) {
                        if (properties.virtualProperties[k].propertyName == propertyName)
                            flagForExisting = false;
                    }
                    if (flagForExisting)
                        properties.virtualProperties.push(folder);
                    else {
                        var msg = 'Can not add duplicate property.'
                        //showDialog(300, 80, 'Saving Result', msg, Ext.Msg.OK, null);
                        Ext.widget('messagepanel', { title: 'Saving Result', msg: msg });
                        return false;
                    }
                }

                var gridStore = button.up('form').down('grid').getStore();

                for (var i = 0; i < gridStore.data.length; i++) {
                    var virtualPropertyValues = {};
                    if (gridStore.data.items[i].data.propertyType == 'Constant')
                        virtualPropertyValues.type = 0;
                    else
                        virtualPropertyValues.type = 1;

                    virtualPropertyValues.valueText = gridStore.data.items[i].data.valueText;
                    virtualPropertyValues.propertyName = gridStore.data.items[i].data.propertyName;
                    virtualPropertyValues.length = gridStore.data.items[i].data.propertyLength;
                    folder.virtualPropertyValues.push(virtualPropertyValues);
                }

                Ext.Ajax.request({
                    url: 'AdapterManager/SaveVirtualProperties',
                    timeout: 600000,
                    method: 'POST',
                    params: {
                        scope: scope,
                        app: app,
                        tree: Ext.JSON.encode(properties)
                    },
                    success: function (response, request) {

                        win.fireEvent('save', me);
                        me.getDirTree().onReload();
                    },
                    failure: function (response, request) {
                        Ext.widget('messagepanel', { title: 'Saving Result', msg: 'An error has occurred while saving virtual property.' });
                        //showDialog(400, 100, 'Saving Result', 'An error has occurred while saving virtual property.', Ext.Msg.OK, null);
                    }
                });
            },
            failure: function (response, request) {
                Ext.widget('messagepanel', { title: 'Error', msg: 'An error has occurred while getting virtual property.' });
                //showDialog(400, 100, 'Error', 'An error has occurred while getting virtual property.', Ext.Msg.OK, null);
            }
        });
    },

    onEditVirtualProperty: function (item, e, eOpts) {
        var me = this;
        var win = Ext.widget('virtualpropertywindow');
        win.setTitle('Edit Virtual Property');
        var form = win.down('form');
        var grid = form.down('grid');
        var store = grid.getStore();
        store.removeAll();
        var tree = me.getDirTree();
        var node = tree.getSelectedNode();
        form.getForm().findField('oldPropertyName').setValue(node.data.text);
        Ext.Ajax.request({
            url: 'AdapterManager/VirtualProperties',
            timeout: 600000,
            params: {
                scope: node.data.id.split('/')[0],
                app: node.data.id.split('/')[1]
            },
            success: function (response, request) {
                var vProperties = Ext.decode(response.responseText).virtualProperties;
                var res = vProperties[0];
                for (j = 0; j < vProperties.length; j++) {
                    if (vProperties[j].propertyName == node.data.text) {
                        res = vProperties[j];
                        break;
                    }
                }

                var properties = [];
                var formRecord = {
                    objectName: res.objectName,
                    scope: node.data.id.split('/')[0],
                    app: node.data.id.split('/')[1],
                    propertyName: res.propertyName,
                    delimeter: res.delimiter
                };
                var ii = 0;
                node.parentNode.eachChild(function (child) {
                    properties.push([ii, child.data.text, child.data.property.Name]);
                    ii++;
                });

                var mapCombo = grid.down('#propertyNameCmb').getEditor(); //form.down('#propertyNameCmb');
                mapCombo.store = Ext.create('Ext.data.SimpleStore', {
                    fields: ['value', 'text', 'name'],
                    autoLoad: true,
                    data: properties
                });

                form.getForm().setValues(formRecord);
                if (res.virtualPropertyValues != undefined) {
                    for (var i = 0; i < res.virtualPropertyValues.length; i++) {
                        var model = Ext.create('AM.model.VirtualPropertyModel');
                        model.data.propertyLength = res.virtualPropertyValues[i].length;
                        model.data.propertyName = res.virtualPropertyValues[i].propertyName;
                        if (res.virtualPropertyValues[i].type == 0)
                            model.data.propertyType = 'Constant';
                        else
                            model.data.propertyType = 'Property';
                        model.data.valueText = res.virtualPropertyValues[i].valueText;
                        store.add(model);
                    }
                }

                win.on('Save', function () {
                    win.destroy();
                }, me);

                win.on('reset', function () {
                    win.destroy();
                }, me);

                win.show();
            },
            failure: function (response, request) {
                Ext.widget('messagepanel', { title: 'Saving Result', msg: 'An error has occurred while saving virtual property.' });
                //showDialog(400, 100, 'Saving Result', 'An error has occurred while saving virtual property.', Ext.Msg.OK, null);
            }
        });
    },

    onDeleteVirtualProperty: function (item, e, eOpts) {
        var me = this;
        var tree = me.getDirTree();
        var node = tree.getSelectedNode();
        var scope = node.data.id.split('/')[0];
        var app = node.data.id.split('/')[1];
        Ext.Ajax.request({
            url: 'AdapterManager/VirtualProperties',
            timeout: 600000,
            params: {
                scope: scope,
                app: app
            },
            success: function (response, request) {
                var res = Ext.decode(response.responseText);
                for (var i = 0; i < res.virtualProperties.length; i++) {
                    if (res.virtualProperties[i].propertyName == node.data.text) {
                        res.virtualProperties.splice(i, 1);
                        break;
                    }
                }

                Ext.Ajax.request({
                    url: 'AdapterManager/SaveVirtualProperties',
                    timeout: 600000,
                    method: 'POST',
                    params: {
                        scope: scope,
                        app: app,
                        tree: Ext.JSON.encode(res)
                    },
                    success: function (response, request) {
                        me.getDirTree().onReload();
                    },
                    failure: function (response, request) {
                        Ext.widget('messagepanel', { title: 'Saving Result', msg: 'An error has occurred while saving virtual property.' });
                        //showDialog(400, 100, 'Saving Result', 'An error has occurred while saving virtual property.', Ext.Msg.OK, null);
                    }
                });

            },
            failure: function (response, request) {
                Ext.widget('messagepanel', { title: 'Error', msg: 'An error has occurred while deleting virtual property.' });
                //showDialog(400, 100, 'Error', 'An error has occurred while deleting virtual property.', Ext.Msg.OK, null);
            }
        });

    },

    onSwitchToCached: function (item, e, eOpts) {
        var me = this;
        me.switchDataMode('Cache');
    },

    onSwitchToLive: function (item, e, eOpts) {
        var me = this;
        me.switchDataMode('Live');
    },

    refreshScopes: function (item, e, eOpts) {
        this.getDirTree().onReload();
    },
    onRefreshRoot: function (item, e, eOpts) {
        this.getDirTree().onReload();
    },


    init: function (application) {
        Ext.QuickTips.init();
        this.control({
            "gridpanel": {
                metachange: this.handleMetachange
            },
            "directorypanel directorytree": {
                beforeload: this.onBeforeLoad
            },
            "menuitem[action=neweditscope]": {
                click: this.newOrEditScope
            },
            "menuitem[action=deletescope]": {
                click: this.deleteScope
            },
            "menuitem[action=neweditendpoint]": {
                click: this.newOrEditEndpoint
            },
            //            "menuitem[action=deleteendpoint]": {
            //                click: this.deleteEndpoint
            //            },
            "menuitem[action=deleteApplication]": {
                click: this.onDeleteApplication
            },
            "menuitem[action=newdatalayer]": {
                click: this.onNewDataLayer
            },
            "menuitem[action=regenerateall]": {
                click: this.onRegenerateAll
            },
            "menuitem[action=showdata]": {
                click: this.onShowDataGrid
            },
            "menuitem[action=refreshfacade]": {
                click: this.onRefreshFacade
            },
            "menuitem[action=configureendpoint]": {
                click: this.onConfigureEndpoint
            },
            "directorytree": {
                itemcontextmenu: this.showContextMenu,
                itemdblclick: this.onItemDblClick
            },
            "menuitem[action=refreshdata]": {
                click: this.onAppDataRefreshClick
            }, /*
            "textfield": {
                blur: this.onTextfieldBlur
            },*/
            "textfield": {
                specialkey: this.onSpecialKey
            },
            "menuitem[action=fileupload]": {
                click: this.onFileUpload
            },
            "menuitem[action=filedownload]": {
                click: this.onFileDownload
            },
            "button[action=addsettings]": {
                click: this.onAddSettings
            },
            "form": {
                afterrender: this.onApplicationFormAfterRender
            },
            "menuitem[action=refreshdataobjectcache]": {
                click: this.onRefreshDataObjectCache
            },
            "menuitem[action=refreshcache]": {
                click: this.onRefreshCache
            },
            "menuitem[action=showCacheInfo]": {
                click: this.onShowCacheInfo
            },
            "menuitem[action = deletcache]": {
                click: this.onDeleteCache
            },
            "menuitem[action=addvirtualproperty]": {
                click: this.onAddVirtualProperty
            },
            "button[action=savevirtualproperties]": {
                click: this.onSaveVirtualProperties
            },
            "menuitem[action=editvirtualproperty]": {
                click: this.onEditVirtualProperty
            },
            "menuitem[action=deletevirtualproperty]": {
                click: this.onDeleteVirtualProperty
            },
            "menuitem[action=switchToCached]": {
                click: this.onSwitchToCached
            },
            "menuitem[action=switchToLive]": {
                click: this.onSwitchToLive
            },
            "menuitem[action=refreshscopes]": {
                click: this.refreshScopes
            },
            "menuitem[action=appDataFiltersMenuItem]": {
                click: this.onAppDataFiltersMenuItem
            },
            "button[action=saveDataFilter]": {
                click: this.saveDataFilter
            },
            "menuitem[action=newOrEditFolder]": {
                click: this.onNewOrEditFolder
            },
            "menuitem[action=deleteFolder]": {
                click: this.onDeleteFolder
            },
            "menuitem[action=newOrEditContext]": {
                click: this.onNewOrEditContext
            },
            "menuitem[action=deleteContext]": {
                click: this.onDeleteContext
            },
            "menuitem[action=refreshRoot]": {
                click: this.onRefreshRoot
            },
            "menuitem[action=cacheupdate]": {
                click: this.onCacheUpdate
            },
            "menuitem[action=editrow]": {
                click: this.onEditrow
            }

        });
    },

    onAppDataFiltersMenuItem: function (item, e, eOpts) {
        var me = this;
        var centerPanel = me.getMainContent();
        centerPanel.getEl().mask("Loading...", "x-mask-loading");

        var tree = this.getDirTree();
        var node = tree.getSelectedNode();

        var contextName = node.parentNode.parentNode.parentNode.data.property['Internal Name'];
        var endpointName = node.parentNode.parentNode.data.property['Internal Name'];
        var graph = node.data.text;

        var relURI = "Directory/getDataFilter";
        var reqParam = { scope: contextName, app: endpointName, graph: graph, start: 0, limit: 25 };
        var getColsUrl = 'GridManager/pages';
        var oeUrl = 'Directory/getDataFilter';
        panelDisable();
        var dfcontroller = me.application.getController("df.controller.DataFilter");
        dfcontroller.dataFiltersMenuItem(centerPanel, node, relURI, reqParam, getColsUrl, "dobj");
    },

    saveDataFilter: function (button, e, eOpts) {
        var me = this;
        var dfcontroller = me.application.getController("df.controller.DataFilter");
        var filterFor = button.up('window').down('dataFilterForm').getForm().findField('filterFor').getValue();
        var directoryTree = me.getDirTree();
        var node = directoryTree.getSelectedNode();

        var graphNode = node.parentNode;
        var contextName = node.parentNode.parentNode.parentNode.data.property['Internal Name'];
        var endpointName = node.parentNode.parentNode.data.property['Internal Name'];
        var graph = node.data.text;
        var ctx = '?scope =' + contextName + '&app=' + endpointName + '&graph=' + graph;
        var relURI = "Directory/dataFilter";
        var reqParam = { scope: contextName, app: endpointName, graph: graph };
        dfcontroller.saveDataFilter(node, reqParam, ctx, relURI, button);

    },

    onSpecialKey: function (f, e) {
        if (f.labelCls.split(' ')[0] == 'ux-rangemenu-icon') {
            if (e.getKey() == e.ENTER) {
                if (f.grid != undefined) {
                    if (!f.up('menu').parentItem.checked) {
                        f.up('menu').parentItem.setChecked(true, true);
                    }
                }
                var me = this;
                var gridPanel = me.getMainContent().activeTab;
                var gridStore = gridPanel.getStore();
                var gridProxy = gridStore.getProxy();
                gridStore.currentPage = 1;
                gridProxy.on('exception', function (proxy, response, operation) {
                    gridPanel.destroy();
                    var rtext = response.responseText;
                    if (rtext != undefined) {
                        var error = 'SUCCESS = FALSE';
                        var index = rtext.toUpperCase().indexOf(error);
                        msg = rtext;
                        Ext.widget('messagepanel', { title: 'Error', msg: msg });
                        //showDialog(500, 300, 'Error', msg, Ext.Msg.OK, null);
                    }
                }, me);
                //var fVal = Ext.JSON.encode(f.dataIndex +":"+f.getValue());
                var fVal = f.dataIndex + ":" + f.getValue();
                gridStore.on('beforeload', function (store, action) {
                    var params = gridStore.proxy.extraParams;
                    params.filter = fVal;
                }, me);

                gridStore.load({
                    callback: function (records, response) {
                        if (records != undefined && records[0] != undefined && records[0].store.proxy.reader.metaData) {
                            //gridPanel.reconfigure(gridStore, records[0].store.proxy.reader.metaData.columns);
                            gridPanel.reconfigure(gridStore);
                        }
                    }
                });
            }
        }
    },

    onShowGrap: function (items, e, eOpts) {
        var me = this;
        var tree = this.getDirTree();
        var node = tree.getSelectedNode();
        content = me.getMainContent();
        contextName = node.parentNode.parentNode.parentNode.data.property['Internal Name'];
        endpointName = node.parentNode.parentNode.data.property['Internal Name'];
        var graph = node.data.text;
    },

    addSettings: function (key, value, nameID, valueID) {
        return [{
            xtype: 'container',
            margin: '10 20 0 96',
            layout: 'column',
            items: [
      {
          xtype: 'textfield',
          name: nameID,
          value: key,
          columnWidth: '0.30',
          allowBlank: true
      },
      {
          xtype: 'textarea',
          name: valueID,
          value: value,
          columnWidth: '0.60',
          grow: false,
          margin: '0 0 0 3'
      },
      {
          xtype: 'button',
          text: 'Delete',
          columnWidth: '0.10',
          margin: '0 0 0 3',
          tooltip: 'Click to Delete settings',
          handler: function () {
              this.findParentByType('container').destroy();
          }
      }
      ]
        }
    ]
    },

    switchDataMode: function (mode) {
        var me = this;
        var tree = me.getDirTree();
        var node = tree.getSelectedNode();
        var content = me.getMainContent();
        content.getEl().mask("Loading...", "x-mask-loading");
        Ext.Ajax.request({
            url: 'AdapterManager/SwitchDataMode',
            method: 'POST',
            timeout: 3600000,
            params: {
                'nodeid': node.data.id,
                'mode': mode
            },
            success: function (response, request) {
                var responseObj = Ext.decode(response.responseText);
                if (responseObj.success) {
                    if (responseObj.response.Level == 0) {
                        var parentNode = node.parentNode;
                        var nodeIndex = parentNode.indexOf(node);
                        parentNode.removeChild(node);
                        parentNode.insertChild(nodeIndex, Ext.JSON.decode(response.responseText).nodes[0]);
                        //me.setLoading(false);
                        tree.view.refresh();
                        //showDialog(500, 160, 'Result', responseObj.Messages.join('\n'), Ext.Msg.OK, null);
                        //Ext.widget('messagepanel', { title: 'Result', msg: responseObj.response.Messages.join('\n') });
                        var data = Ext.JSON.decode(response.responseText).nodes[0].property;
                        var detailGrid = tree.up('panel').down('propertypanel');
                        detailGrid.setSource(data);
                        Ext.example.msg('Notification', 'Data Mode switched successfully!');
                    }
                    content.getEl().unmask();
                    //tree.onReload();
                } else {
                    content.getEl().unmask();
                    var userMsg = responseObj.message;
                    var detailMsg = responseObj.stackTraceDescription;
                    var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification' });
                    Ext.ComponentQuery.query('#expValue', expPanel)[0].setValue(userMsg);
                    Ext.ComponentQuery.query('#expValue2', expPanel)[0].setValue(detailMsg);
                }
            },
            failure: function (response, request) {
                var responseObj = Ext.decode(response.responseText);
                Ext.widget('messagepanel', { title: 'Error', msg: responseObj.response.Messages.join('\n') });
                //showDialog(500, 160, 'Error', responseObj.Messages.join('\n'), Ext.Msg.OK, null);
            }
        });
    }
});
