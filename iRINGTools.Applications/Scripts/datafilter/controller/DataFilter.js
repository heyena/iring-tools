
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
	
	init: function(application) {
	    this.control({
	    	/*"menuitem[action=dataFiltersMenuItem]": {
	            click: this.dataFiltersMenuItem
	        },
	        "button[action=saveDataFilter]": {
	            click: this.saveDataFilter
	        }*/
	
	    });
	},
	
	abc : function(){
		
	},

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
        var urlWithTransferType = url  + '&sort='+ "true";
        propertyStore.getProxy().url = urlWithTransferType;

        propertyStore.load();

        var obj = me.getDataFilter();
        var dfList;
        
        Ext.Ajax.request({
            url : relURI, 
            params: reqParam,
            method: 'POST',
            timeout : 86400000, // 24 hours
            success : function(response, request) {
                var respObj = Ext.JSON.decode(response.responseText);
                if (respObj !== null)
                {
                    for (var i = 0; i < respObj.length; i++)
                    {
                        var open = respObj[i].dfList[0];
                        var log = respObj[i].dfList[1];
                        var rel = respObj[i].dfList[2];
                        var prop = respObj[i].dfList[3];
                        var value = respObj[i].dfList[4];
                        var close = respObj[i].dfList[5];

                        if (i+1 >= 2)
                        {
                            var AddDf = Ext.ComponentQuery.query('#save_0', obj);
                            var add =  AddDf[0];
                            add.handler.call(add); 
                        }
                        var arrOpenList = Ext.ComponentQuery.query('#openCount_'+(i+1), obj);
                        arrOpenList[0].setValue(open);
                        var arrProNameList = Ext.ComponentQuery.query('#propertyName_'+(i+1), obj); 
                        arrProNameList[0].setValue(prop);  
                        var columnName = Ext.ComponentQuery.query('#propertyName_'+(i+1), obj)[0].getValue();
                        var relationalStore;
                        if(columnName === "Transfer Type")
                        {
                            relationalStore = Ext.getStore('RelationalStoreforTransferType');
                            relationalStore.load();

                        } else{
                        	
                            relationalStore = Ext.getStore('RelationalStoreComplete');
                            relationalStore.load();
                        }
                        var arrRelaOperList = Ext.ComponentQuery.query('#relationalOperator_'+(i+1), obj);
                        arrRelaOperList[0].bindStore(relationalStore);  
                        arrRelaOperList[0].setValue(rel); 

                        var arrValueList = Ext.ComponentQuery.query('#value_'+(i+1), obj);
                        arrValueList[0].setValue(value);                
                        var arrLogOperList = Ext.ComponentQuery.query('#logicalOperator_'+(i+1), obj);
                        arrLogOperList[0].setValue(log);                
                        var arrCloseList = Ext.ComponentQuery.query('#closeGroup_'+(i+1), obj);
                        arrCloseList[0].setValue(close);
                    }
                    centerPanel.getEl().unmask();
                    view.show();
                }
                obj.getForm().findField('filterFor').setValue(filterFor);
            },
            failure : function(response, request) {
                centerPanel.getEl().unmask();
                Ext.Msg.alert("Error fetching data to fill form");
            }
        });
        Ext.Ajax.request({
            url: oeUrl,
            params: reqParam,
            method: 'POST',
            timeout : 86400000, // 24 hours
            success : function(response, request) {
                var resObj = Ext.JSON.decode(response.responseText);
                if (resObj !== null) {
                    for (var i = 0; i < resObj.length; i++) {

                        var prop = resObj[i].dfList[0];
                        var sort = resObj[i].dfList[1];  

                        if (i+1 >= 2)
                        {
                            var OEAddList = Ext.ComponentQuery.query('#OEAdd_0', obj); 
                            var add =  OEAddList[0];
                            add.handler.call(add);
                        }
                        var arrProNameList = Ext.ComponentQuery.query('#OEProName_'+(i+1), obj); 
                        arrProNameList[0].setValue(prop);  

                        var arrSortOrderList = Ext.ComponentQuery.query('#OESortOrder_'+(i+1), obj); 
                        arrSortOrderList[0].setValue(sort);  

                    }
                    centerPanel.getEl().unmask();
                    view.show();
                }
            },
            failure : function(response, request) {
                centerPanel.getEl().unmask();
                Ext.Msg.alert("Error fetching Filters");
            }
        });
        if (respObj !== null)
        {
            centerPanel.getEl().unmask();
            view.show();
        }
	},
	
	getPropertyName: function(scope, xid){
			var view = Ext.widget('dataFilterWIn');    
  	        var propertyComboBoxList = Ext.ComponentQuery.query('#propertyName_1', view);
	        var propertyComboBox = propertyComboBoxList[0];
	        var propertyStore = propertyComboBox.getStore();
	        var propertyProxy = propertyStore.getProxy();
	        var url = 'getColumnNames?' + '&scope ='  +scope+ '&xid='+xid ;
	        var urlWithTransferType = url  + '&sort='+ "true";
	        propertyStore.getProxy().url = urlWithTransferType;
	        propertyStore.load();
	
	},
		
	saveDataFilter: function(node,reqParams, ctx, relURI,button) {
        var me = this;
	    var obj = me.getDataFilter();
	    var form = obj.getForm();
	    var isAdmin = form.findField('isAdmin').getValue();
	    if (isAdmin == 'On') {
	        isAdmin = true;
	    }
	    var reqParam = Ext.JSON.encode(reqParams);
	    form.submit({
	        url: relURI,
	        params: {
	            isAdmin: isAdmin,
	            reqParams: reqParam
	        },
	        method: 'POST',
	        timeout: 120000,
	        success: function (response, request) {
	            Ext.Ajax.request({
	                url: 'directory/reset?dtoContext=' + escape(ctx.substring(1)),
	                method: 'POST'
	            });
	            button.up('.window').close();
	            panelEnable();
	        },
	        failure: function (response, request) {
	            Ext.Msg.alert("save failed");
	        }
	    });  
	}
    
});
