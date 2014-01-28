
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
                var dfList = Ext.JSON.decode(response.responseText);
                if(dfList !== null)
                {
                    for(var i=0; i < dfList.length; i++)
                    {
                        console.log("dfList.length is " + dfList.length);
                        console.log("dfList[i][3] is " + dfList[i][3]);
                        var open = dfList[i][0];
                        var log = dfList[i][1];
                        var rel = dfList[i][2];
                        var prop = dfList[i][3];
                        var value = dfList[i][4];
                        var close = dfList[i][5];

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


                       /* Ext.getCmp('openCount_'+(i+1)).setValue(open);
                        Ext.getCmp('propertyName_'+(i+1)).setValue(prop);
                        Ext.getCmp('relationalOperator_'+(i+1)).setValue(rel);
                        Ext.getCmp('value_'+(i+1)).setValue(value);
                        Ext.getCmp('logicalOperator_'+(i+1)).setValue(log);
                        Ext.getCmp('closeGroup_'+(i+1)).setValue(close);*/

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
                var dfList = Ext.JSON.decode(response.responseText);
                //  formdata.setValues({name : form.name,description : form.description, sourceUri : form.sourceUri, sourceScopeName : form.sourceScope, sourceAppName : form.sourceApp, sourceGraphName : form.sourceGraph , targetUri :form.targetUri, targetScopeName : form.targetScope, targetAppName : form.targetApp, targetGraphName : form.targetGraph, oldConfigName : commConfigName, oldCommName : commodity, oldScope :scope});   
                if(dfList !== null)
                {
                    for(var i=0; i < dfList.length; i++)
                    {

                        var prop = dfList[i][0];
                        var sort = dfList[i][1];      

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
        if(dfList !== null)
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
	
	FillDataFilterOE: function(dfList){
	   for(var i=0; i < dfList.length; i++) {
		   	var prop = dfList[i][0];
	        var sort = dfList[i][1];      
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
	
	},
	
	FillDataFilterExpression: function(dfList){
		for(var i=0; i < dfList.length; i++){
	            console.log("dfList.length is " + dfList.length);
	            console.log("dfList[i][3] is " + dfList[i][3]);
	            var open = dfList[i][0];
	            var log = dfList[i][1];
	            var rel = dfList[i][2];
	            var prop = dfList[i][3];
	            var value = dfList[i][4];
	            var close = dfList[i][5];
	
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
	
	},
	
	saveDataFilter: function(node,reqParams, ctx, relURI,button) {
        var me = this;
	    var obj = me.getDataFilter();
	    var form = obj.getForm();
	    var isAdmin = form.findField('isAdmin').getValue();
	    if (isAdmin == 'on') {
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
