
Ext.define('AM.view.nhibernate.DataObjectPanel', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.dataObjectPane',
    layout: 'border',
    frame: false,
    border: false,
    contextName: null,
    endpoint: null,
    items: [
                { xtype: 'nhibernatetreepanel',  region: 'west' },
                { xtype: 'editorPanel', region: 'center' }
            ],

    initComponent: function () {
        var wizard = this;
        var scopeName = this.contextName;
        var appName = this.endpoint;
        var userTableNames;

        if (scopeName) {

            var conf = {
                id: scopeName + '.' + appName + '.tree-panel',
                contextName: scopeName,
                endpoint: appName
            };

            var treePanel = Ext.widget('nhibernatetree', conf);

            var confEditor = {
                id: scopeName + '.' + appName + '.editor-panel'
            };

            var editorPanel = Ext.widget('editorPanel', confEditor);            

            Ext.Ajax.request({
                url: 'AdapterManager/DataType',
                method: 'GET',

                success: function (response, request) {
                    var dataTypeName = Ext.JSON.decode(response.responseText);
                    AM.view.nhibernate.dataTypes.value = new Array();
                    var i = 0;
                    while (!dataTypeName[i])
                        i++;
                    while (dataTypeName[i]) {
                        AM.view.nhibernate.dataTypes.value.push([i, dataTypeName[i]]);
                        i++;
                    }
                },
                failure: function (f, a) {
                    if (a.response)
                        showDialog(500, 400, 'Error', a.response.responseText, Ext.Msg.OK, null);
                }
            });

            Ext.EventManager.onWindowResize(this.doLayout, this);

            Ext.Ajax.request({
                url: 'AdapterManager/DBDictionary',
                method: 'POST',
                params: {
                    scope: scopeName,
                    app: appName
                },
                success: function (response, request) {
                    AM.view.nhibernate.dbDict.value = Ext.JSON.decode(response.responseText);
                    var dbDict = AM.view.nhibernate.dbDict.value;

                    if (dbDict.ConnectionString) {
                        var base64 = AM.view.nhibernate.Utility;
                        AM.view.nhibernate.dbDict.value.ConnectionString = base64.decode(dbDict.ConnectionString);
                    }

                    var dbObjectsTree = Ext.widget('nhibernatetree');

                    if (dbDict.dataObjects.length > 0) {
                        // populate data source form
                        showTree(scopeName, appName);                        
                    }
                    else {
                        dbObjectsTree.disable();
                        setDsConfigPane(scopeName, appName);
                    }
                },
                failure: function (response, request) {
                    setDsConfigPane(scopeName, appName);
                }
            });


        }
        this.callParent(arguments);

    }
});

