
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
	
	dataFiltersMenuItem: function(centerPanel,node,relURI, reqParam, getColsUrl, filterFor) {
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
            url : 'getDataFilterOE',
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
	    var form = obj.getValues(true);
	    var expressList = Ext.ComponentQuery.query('#Expressions', obj);
	    var express =  expressList[0];
	    //var express = Ext.getCmp('Expressions');
	    var config = Ext.apply({}, express.initialConfig.items[1]);
	    var ExpressCount = express.items.length; 
	    var arrOpen = [];
	    var arrProName = [];
	    var arrRelaOper = [];
	    var arrValue = [];
	    var arrLogOper = [];
	    var arrClose = [];
	
	
	    for(var i=1; i <= ExpressCount-1; i++)    
	    {
	        var arrOpenList = Ext.ComponentQuery.query('#openCount_'+i, obj);
	        arrOpen[i] =  arrOpenList[0].getValue();
	        // arrOpen[i] = Ext.getCmp('openCount_'+i).getValue();
	
	        var arrProNameList = Ext.ComponentQuery.query('#propertyName_'+i, obj);
	        arrProName[i] =  arrProNameList[0].getValue();    
	        // arrProName[i] = Ext.getCmp('propertyName_'+i).getValue();
	
	        var arrRelaOperList = Ext.ComponentQuery.query('#relationalOperator_'+i, obj);
	        arrRelaOper[i] =  arrRelaOperList[0].getValue();
	        // arrRelaOper[i] = Ext.getCmp('relationalOperator_'+i).getValue();
	
	        var arrValueList = Ext.ComponentQuery.query('#value_'+i, obj);
	        arrValue[i] =  arrValueList[0].getValue();
	        //  arrValue[i] = Ext.getCmp('value_'+i).getValue();
	
	        var arrLogOperList = Ext.ComponentQuery.query('#logicalOperator_'+i, obj);
	        arrLogOper[i] =  arrLogOperList[0].getValue();
	        // arrLogOper[i] = Ext.getCmp('logicalOperator_'+i).getValue();
	
	        var arrCloseList = Ext.ComponentQuery.query('#closeGroup_'+i, obj);
	        arrClose[i] =  arrCloseList[0].getValue();
	        // arrClose[i] = Ext.getCmp('closeGroup_'+i).getValue();
	
	        /* var  arrOpen = Ext.getCmp('openCount_'+i).getValue();
	        var  arrProName = Ext.getCmp('propertyName_'+i).getValue();
	        var  arrRelaOper = Ext.getCmp('relationalOperator_'+i).getValue();
	        var  arrValue = Ext.getCmp('value_'+i).getValue();
	        var  arrLogOper = Ext.getCmp('logicalOperator_'+i).getValue();
	        var  arrClose = Ext.getCmp('closeGroup_'+i).getValue();*/
	
	    }
	
	    var context = ctx;
	    var oExpressList = Ext.ComponentQuery.query('#OExpress', obj);
	    var oExpress =  oExpressList[0];
	    //var oExpress = Ext.getCmp('OExpress');
	    var config = Ext.apply({}, oExpress.initialConfig.items[1]);
	    var OExpressCount = oExpress.items.length; 
	    console.log(" Order Expressions " + OExpressCount);
	
	    var arrSortOrder = [];
	    var arrProNameOE = [];
	
	    for(var i=1; i <= OExpressCount-1; i++)
	    {
	
	        var arrSortOrderList = Ext.ComponentQuery.query('#OESortOrder_'+i, obj);
	        arrSortOrder[i] =  arrSortOrderList[0].getValue();
	        var arrProNameList = Ext.ComponentQuery.query('#OEProName_'+i, obj);
	        arrProNameOE[i] =  arrProNameList[0].getValue();
	
	    }
	
	    Ext.Ajax.request({
	        url : relURI,
	        params : {
	            expressCountOE : OExpressCount,
	            expressCount : ExpressCount,
	            openGroup : arrOpen,
	            propertyNameOE : arrProNameOE,
	            sortOrder : arrSortOrder,
	            propertyName : arrProName,
	            relationalOper : arrRelaOper,
	            value : arrValue,
	            logicalOper : arrLogOper,
	            closeGroup : arrClose,
	            reqParams: reqParams
	        },
	        method: 'POST',
	        timeout : 120000,
	        success : function(response, request) {
	            Ext.Ajax.request({
	                url : 'reset?dtoContext=' + escape(context.substring(1)),
	                method : 'POST'
	
	            });
	            button.up('.window').close();
	        },
	        failure : function(response, request) {
	            Ext.Msg.alert("save failed");
	        }
	    });  
	}
    
});
