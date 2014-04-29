
Ext.define('df.controller.DataFilter', {
    extend: 'Ext.app.Controller',

    id: 'dataFilter',

    models: [
		'Names'
	],

    stores: [
		'ColumnNameStore',
		'RelationalStoreComplete',
		'RelationalStoreforTransferType'
	],

    views: [
		'DataFilterForm',
		'DataFilterWin'
	],

    refs: [
		{
		    ref: 'dataFilter',
		    selector: 'dataFilterForm',
		    xtype: 'dataFilterForm'
		}
	],

    init: function (application) {
        this.control({
        /*"menuitem[action=dataFiltersMenuItem]": {
        click: this.dataFiltersMenuItem
        },
        "button[action=saveDataFilter]": {
        click: this.saveDataFilter
        }*/

    });
},

abc: function () {

},

//dataFiltersMenuItem: function (centerPanel, node, relURI, reqParam, getColsUrl, filterFor, oeUrl) {
//    var me = this;
//    var view = Ext.widget('dataFilterWin');
//    view.setTitle("Configure Data Filter");
//    // getting propertiesName...

//    var propertyComboBoxList = Ext.ComponentQuery.query('#propertyName_1', view);
//    var propertyComboBox = propertyComboBoxList[0];
//    var propertyStore = propertyComboBox.getStore();

//    var propertyProxy = propertyStore.getProxy();
//    var url = getColsUrl;
//    var respObj;
//    Ext.Ajax.request({
//        url: "Scripts/datafilter/Test.json",
//        method: 'POST',
//        timeout: 86400000, // 24 hours
//        success: function (response, request) {
//            var respObj = Ext.JSON.decode(response.responseText);
//            propertyStore.loadData(respObj.metaData.columns);
//        },
//        failure: function (response, request) {
//            centerPanel.getEl().unmask();
//            Ext.Msg.alert("Error fetching data to fill form");
//        }
//    });

//    if (respObj !== null) {
//        centerPanel.getEl().unmask();
//        view.show();
//    }
//},

dataFiltersMenuItem: function (centerPanel, node, relURI, reqParam, getColsUrl, filterFor, oeUrl) {
    var me = this;
    var view = Ext.widget('dataFilterWin');
    view.setTitle("Configure Data Filter");
    // getting propertiesName...

    var propertyComboBoxList = Ext.ComponentQuery.query('#propertyName_1', view);
    var propertyComboBox = propertyComboBoxList[0];
    var propertyStore = propertyComboBox.getStore();
    var propertyProxy = propertyStore.getProxy();
    var url = getColsUrl;
    var obj = me.getDataFilter();
    var respObj;
    Ext.Ajax.request({
        //url: "Scripts/datafilter/Test.json",
        url: url,
        params: reqParam,
        method: 'POST',
        timeout: 86400000, // 24 hours
        success: function (response, request) {
            var respObj = Ext.JSON.decode(response.responseText);
            propertyStore.loadData(respObj.metaData.columns);
        },
        failure: function (response, request) {
            centerPanel.getEl().unmask();
            Ext.Msg.alert("Error fetching data to fill form");
        }
    });
    Ext.Ajax.request({
        //url: "Scripts/datafilter/TestOE.json",
        url: relURI,
        params: reqParam,
        method: 'POST',
        timeout: 86400000, // 24 hours
        success: function (response, request) {
            var respObj = Ext.JSON.decode(response.responseText);
            //propertyStore.loadData(respObj.metaData.columns);
            if (respObj.items !== null) {
                var resp = respObj.items;
                if (resp.Expressions.length > 0) {
                    for (var i = 0; i < resp.Expressions.length; i++) {
                        var lItem = resp.Expressions[i];
                        var open = lItem.OpenGroupCount;
                        var log = lItem.LogicalOperator;
                        var rel = lItem.RelationalOperator;
                        var prop = lItem.PropertyName;
                        var value = lItem.Values;
                        var close = lItem.CloseGroupCount;

                        if (i + 1 >= 2) {
                            var AddDf = Ext.ComponentQuery.query('#save_0', obj);
                            var add = AddDf[0];
                            add.handler.call(add);
                        }

                        var arrOpenList = Ext.ComponentQuery.query('#openCount_' + (i + 1), obj);
                        arrOpenList[0].setValue(open);
                        var arrProNameList = Ext.ComponentQuery.query('#propertyName_' + (i + 1), obj);
                        arrProNameList[0].setValue(prop);
                        var columnName = Ext.ComponentQuery.query('#propertyName_' + (i + 1), obj)[0].getValue();
                        var relationalStore;
                        if (columnName === "Transfer Type") {
                            relationalStore = Ext.getStore('RelationalStoreforTransferType');
                            relationalStore.load();

                        } else {

                            relationalStore = Ext.getStore('RelationalStoreComplete');
                            relationalStore.load();
                        }
                        var arrRelaOperList = Ext.ComponentQuery.query('#relationalOperator_' + (i + 1), obj);
                        arrRelaOperList[0].bindStore(relationalStore);
                        arrRelaOperList[0].setValue(rel);

                        var arrValueList = Ext.ComponentQuery.query('#value_' + (i + 1), obj);
                        arrValueList[0].setValue(value);
						if(rel==10 || rel==11)
							arrValueList[0].disable(true);
						else
							arrValueList[0].enable(true);
							
                        var arrLogOperList = Ext.ComponentQuery.query('#logicalOperator_' + (i + 1), obj);
                        arrLogOperList[0].setValue(log);
                        var arrCloseList = Ext.ComponentQuery.query('#closeGroup_' + (i + 1), obj);
                        arrCloseList[0].setValue(close);
                        obj.getForm().findField('exprCount').setValue(i + 1);
                        //}
                        //centerPanel.getEl().unmask();
                        //view.show();
                    }
                }
                if (resp.OrderExpressions.length > 0) {

                    for (var i = 0; i < resp.OrderExpressions.length; i++) {
                        var lItem = resp.OrderExpressions[i];
                        var prop = lItem.PropertyName;
                        var sort = lItem.SortOrder;

                        if (i + 1 >= 2) {
                            var OEAddList = Ext.ComponentQuery.query('#OEAdd_0', obj);
                            var add = OEAddList[0];
                            add.handler.call(add);
                        }
                        var arrProNameList = Ext.ComponentQuery.query('#OEProName_' + (i + 1), obj);
                        arrProNameList[0].setValue(prop);

                        var arrSortOrderList = Ext.ComponentQuery.query('#OESortOrder_' + (i + 1), obj);
                        arrSortOrderList[0].setValue(sort);
                        obj.getForm().findField('oeExprCount').setValue(i + 1);
                    }
                }
                obj.getForm().findField('isAdmin').setValue(resp.isAdmin);

                obj.getForm().findField('filterFor').setValue(filterFor);
            }

        },
        failure: function (response, request) {
            centerPanel.getEl().unmask();
            Ext.Msg.alert("Error fetching data to fill form");
        }
    });

    if (respObj !== null) {
        centerPanel.getEl().unmask();
        view.show();
    }
},


saveDataFilter: function (node, reqParams, ctx, relURI, button) {
    var me = this;
    var obj = me.getDataFilter();
    var form = obj.getForm();
    var isAdmin = form.findField('isAdmin').getValue();
    if (isAdmin == 'On') {
        isAdmin = true;
    }
    //var reqParam = Ext.JSON.encode(reqParams);
    var reqParam2 = reqParams['scope'] + '.' + reqParams['app'] + '.' + reqParams['graph'];
    form.submit({
        url: relURI,
        params: {
            isAdmin: isAdmin,
            reqParams: reqParam2//reqParam
        },
        method: 'POST',
        timeout: 120000,
        success: function (response, request) {
            if (Ext.ComponentQuery.query('myGrid')[0]) {
                var gridStore = Ext.ComponentQuery.query('myGrid')[0].getStore();
                if (gridStore)
                    gridStore.reload();
            }



            /*Ext.Ajax.request({
            url: 'directory/reset?dtoContext=' + escape(ctx.substring(1)),
            method: 'POST'
            });*/
            button.up('.window').close();
            panelEnable();
        },
        failure: function (response, request) {
            Ext.Msg.alert("save failed");
        }
    });
}

});
