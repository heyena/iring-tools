

Ext.define('df.view.DataFilterForm', {
    extend: 'Ext.form.Panel',
    alias: 'widget.dataFilterForm',
    height: 400,
    margin: 1,
    width: 767,
    autoScroll: true,
    initComponent: function() {
        var me = this;
        Ext.applyIf(me, {
            items: [
                {
                    xtype: 'fieldset',
                    anchor: '100%',
                    height: 197,
                    itemId: 'Expressions',
                    margin: 2,
                    autoScroll: true,
                    layout: {
                        type: 'auto'
                    },
                    title: 'Expressions',
                    items: [
                        {
                            xtype: 'container',
                            height: 50,
                            itemId: 'dataFilter_0',
                            layout: {
                                align: 'stretch',
                                type: 'hbox'
                            },
                            items: [
                                {
                                    xtype: 'displayfield',
                                    flex: 1,
                                    margins: '0 0 0 0',
                                    margin: '',
                                    value: '<b>Open Group Count</b>'
                                },
                                {
                                    xtype: 'displayfield',
                                    flex: 1,
                                    margins: '0 0 0 50',
                                    value: '<b>Property Name</b>'
                                },
                                {
                                    xtype: 'displayfield',
                                    flex: 1,
                                    margins: '0 0 0 25',
                                    value: '<b>Relational Operator</b>'
                                },
                                {
                                    xtype: 'displayfield',
                                    flex: 1,
                                    margins: '0 0 0 55',
                                    margin: '',
                                    name: 'ValueLabel',
                                    value: '<b>Value</b>'
                                },
                                {
                                    xtype: 'displayfield',
                                    flex: 1,
                                    margins: '0 0 0 25',
                                    value: '<b>Logical Operator</b>'
                                },
                                {
                                    xtype: 'displayfield',
                                    flex: 1,
                                    margins: '0 0 0 25',
                                    value: '<b>Close Group Count </b>'
                                },
                                {
                                    xtype: 'button',
                                    handler: function(button, event) {
                                        var container = this.up('fieldset');
                                        var config = Ext.apply({}, container.initialConfig.items[1]);
                                        var rowCount = container.items.length;
                                        var rowID = 'dataFilter'+'_'+rowCount;
                                        var rowCompList = Ext.ComponentQuery.query('#'+rowID, this);
                                        var rowCmp =  rowCompList[0];
                                        while(rowCmp){ 
                                            rowCount = rowCount+1;
                                            rowID = 'dataFilter'+'_'+rowCount;
                                            rowCmp = rowCompList[0];
                                        }
                                        config.itemId = rowID;
                                        //config.fieldLabel = rowCount;
                                        config.items[0].name = 'openGroupCount_' + rowCount;
                                        config.items[1].name = 'propertyName_' + rowCount;
                                        config.items[2].name = 'relationalOperator_' + rowCount;
                                        config.items[3].name = 'value_' + rowCount;
                                        config.items[4].name = 'logicalOperator_' + rowCount;
                                        config.items[5].name = 'closeGroupCount_' + rowCount;

                                        config.items[0].itemId = 'openCount_' + rowCount;
                                        config.items[1].itemId = 'propertyName_' + rowCount;
                                        config.items[2].itemId = 'relationalOperator_' + rowCount;
                                        config.items[3].itemId = 'value_' + rowCount;
                                        config.items[4].itemId = 'logicalOperator_' + rowCount;
                                        config.items[5].itemId = 'closeGroup_' + rowCount;
                                        config.items[6].itemId = 'delete_' + rowCount;

                                        container.add(config);
                                        this.up('dataFilterForm').getForm().findField('exprCount').setValue(rowCount);
                                    },
                                    margin: '5 0 5 10',
                                    border: false,
                                    itemId: 'save_0',
                                    icon: 'Content/img/16x16/add.png'
                                }
                            ]
                        },
                        {
                            xtype: 'container',
                            itemId: 'dataFilter_1',
                            layout: {
                                align: 'stretch',
                                type: 'hbox'
                            },
                            items: [
                                {
                                    xtype: 'textfield',
                                    flex: 1,
                                    margins: '5 0 5 0',
                                    itemId: 'openCount_1',
                                    name: 'openGroupCount_1'
                                },
                                {
                                    xtype: 'combobox',
                                    listConfig: {
                                        width: '450'
                                    },
                                    flex: 1,
                                    margins: '5 0 5 10',
                                    itemId: 'propertyName_1',
                                    name: 'propertyName_1',
                                    matchFieldWidth: false,
                                    displayField: 'text',
                                    queryMode: 'local',
                                    store: 'ColumnNameStore',
                                    valueField: 'text',
                                    listeners: {
                                        select: {
                                            fn: me.onPropertyName_1Select,
                                            scope: me
                                        },
                                        afterrender: {
                                            fn: me.onPropertyName_1AfterRender,
                                            scope: me
                                        },
                                        change: {
                                            fn: me.onPropertyName_1Change,
                                            scope: me
                                        }
                                    }
                                },
                                {
                                    xtype: 'combobox',
                                    listConfig: {
                                        width: 250
                                    },
                                    flex: 1,
                                    margins: '5 0 5 10',
                                    itemId: 'relationalOperator_1',
                                    name: 'relationalOperator_1',
                                    editable: false,
                                    matchFieldWidth: false,
                                    displayField: 'value',
                                    queryMode: 'local',
                                    store: 'RelationalStoreComplete',
                                    valueField: 'name'
                                },
                                {
                                    xtype: 'textfield',
                                    flex: 1,
                                    margins: '5 0 5 10',
                                    itemId: 'value_1',
                                    name: 'value_1'
                                },
                                {
                                    xtype: 'combobox',
                                    flex: 1,
                                    margins: '5 0 5 10',
                                    itemId: 'logicalOperator_1',
                                    name: 'logicalOperator_1',
                                    editable: false,
                                    store: [
                                        [
                                            '0',
                                            'AND'
                                        ],
                                        [
                                            '1',
                                            'OR'
                                        ]
                                    ]
                                },
                                {
                                    xtype: 'textfield',
                                    flex: 1,
                                    margins: '5 0 5 10',
                                    itemId: 'closeGroup_1',
                                    name: 'closeGroupCount_1'
                                },
                                {
                                    xtype: 'button',
                                    handler: function(button, event) {
                                        var container = this.up('fieldset');

                                        // container.el.mask('Deleting Expression ....');
                                        var rowCount = container.items.length;
                                        var str = this.up('container').getItemId();
                                        var curID = parseInt(str.charAt(str.length-1));
                                        var arrOpen = [];
                                        var arrProName = [];
                                        var arrRelaOper = [];
                                        var arrValue = [];
                                        var arrLogOper = [];
                                        var arrClose = [];
                                        //var arrDel =[];

                                        for(var i=curID; i <= rowCount-1; i++)    
                                        {
                                            var arrOpenList = Ext.ComponentQuery.query('#openCount_'+i, container);
                                            arrOpen[i] =  arrOpenList[0].getValue();   

                                            var arrProNameList = Ext.ComponentQuery.query('#propertyName_'+i, container);
                                            arrProName[i] =  arrProNameList[0].getValue();    

                                            var arrRelaOperList = Ext.ComponentQuery.query('#relationalOperator_'+i, container);
                                            arrRelaOper[i] =  arrRelaOperList[0].getValue();

                                            var arrValueList = Ext.ComponentQuery.query('#value_'+i, container);
                                            arrValue[i] =  arrValueList[0].getValue();

                                            var arrLogOperList = Ext.ComponentQuery.query('#logicalOperator_'+i, container);
                                            arrLogOper[i] =  arrLogOperList[0].getValue();

                                            var arrCloseList = Ext.ComponentQuery.query('#closeGroup_'+i, container);
                                            arrClose[i] =  arrCloseList[0].getValue();


                                            var DfList = Ext.ComponentQuery.query('#dataFilter_'+i, container);
                                            DfList[0].destroy();

                                        }
                                        //var config = Ext.apply({}, container.initialConfig.items[1]);

                                        for(var j=curID+1; j <= rowCount-1; j++)    
                                        {
                                            var AddDf = Ext.ComponentQuery.query('#save_0', container);
                                            var add =  AddDf[0];
                                            add.handler.call(add); 

                                            var arrOpenList = Ext.ComponentQuery.query('#openCount_'+(j-1), container);
                                            arrOpenList[0].setValue(arrOpen[j]);

                                            var arrProNameList = Ext.ComponentQuery.query('#propertyName_'+(j-1), container); 
                                            arrProNameList[0].setValue(arrProName[j]);

                                            var arrRelaOperList = Ext.ComponentQuery.query('#relationalOperator_'+(j-1), container);
                                            arrRelaOperList[0].setValue(arrRelaOper[j]);

                                            var arrValueList = Ext.ComponentQuery.query('#value_'+(j-1), container);
                                            arrValueList[0].setValue(arrValue[j]);

                                            var arrLogOperList = Ext.ComponentQuery.query('#logicalOperator_'+(j-1), container);
                                            arrLogOperList[0].setValue(arrLogOper[j]);

                                            var arrCloseList = Ext.ComponentQuery.query('#closeGroup_'+(j-1), container);
                                            arrCloseList[0].setValue(arrClose[j]);


                                        }
                                        //container.up('form').getForm().findField('exprCount').setValue(rowCount-1);
                                        var oeField = Ext.ComponentQuery.query('#exprCountId', container.up('form'));
                                        if(oeField[0].getValue() >= 1){
                                        	oeField[0].setValue(oeField[0].getValue() - 1);
                                        }else{
                                        	oeField[0].setValue(0);
                                        }
                                        //oeField[0].setValue(oeField[0].getValue() - 1);
                                        container.el.unmask();
                                	},
                                    border: false,
                                    itemId: 'delete_1',
                                    margin: '5 0 5 10',
                                    icon: 'Content/img/16x16/delete-icon.png',
                                    tooltip: 'remove Expression'
                                }
                            ]
                        }
                    ]
                },
                {
                    xtype: 'fieldset',
                    anchor: '100%',
                    height: 159,
                    itemId: 'OExpress',
                    margin: 5,
                    autoScroll: true,
                    layout: {
                        type: 'auto'
                    },
                    title: 'Order Expressions',
                    items: [
                        {
                            xtype: 'container',
                            itemId: 'OE_0',
                            layout: {
                                align: 'stretch',
                                type: 'hbox'
                            },
                            items: [
                                {
                                    xtype: 'displayfield',
                                    flex: 1,
                                    margin: 5,
                                    labelStyle: 'center',
                                    value: '<b>Property Name </b>'
                                },
                                {
                                    xtype: 'displayfield',
                                    flex: 1,
                                    value: '<b>Sort Order </b>'
                                },
                                {
                                    xtype: 'button',
                                    handler: function(button, event) {
                                        var container = this.up('fieldset');
                                        var config = Ext.apply({}, container.initialConfig.items[1]);
                                        var rowCount = container.items.length;
                                        var rowID = 'OE'+'_'+rowCount;
                                        var OElist = Ext.ComponentQuery.query('#'+rowID, this);
                                        var rowCmp = OElist[0];
                                        //var rowCmp = Ext.getCmp(rowID);
                                        while(rowCmp){
                                            rowCount = rowCount+1;
                                            rowID = 'dataFilter'+'_'+rowCount;
                                            var Dflist = Ext.ComponentQuery.query('#'+rowID, this);
                                            var rowCmp = Dflist[0];
                                            // rowCmp = Ext.getCmp(rowID);
                                        }
                                        config.itemId = rowID;
                                        //config.fieldLabel = rowCount;
                                        config.items[0].name = 'OEProName_' + rowCount;
                                        config.items[1].name = 'OESortOrder_' + rowCount;

                                        config.items[0].itemId = 'OEProName_' + rowCount;
                                        config.items[1].itemId = 'OESortOrder_' + rowCount;

                                        config.items[2].itemId = 'OEDelete_' + rowCount;
                                        container.add(config);
                                        container.up('form').getForm().findField('oeExprCount').setValue(rowCount);

                                    },
                                    margins: '5',
                                    border: false,
                                    height: 22,
                                    itemId: 'OEAdd_0',
                                    icon: 'Content/img/16x16/add.png'
                                }
                            ]
                        },
                        {
                            xtype: 'container',
                            height: 33,
                            itemId: 'OE_1',
                            layout: {
                                align: 'stretch',
                                type: 'hbox'
                            },
                            items: [
                                {
                                    xtype: 'combobox',
                                    listConfig: {
                                        width: 450
                                    },
                                    flex: 1,
                                    margins: '6',
                                    itemId: 'OEProName_1',
                                    name: 'OEProName_1',
                                    matchFieldWidth: false,
                                    displayField: 'text',
                                    queryMode: 'local',
                                    store: 'ColumnNameStore',
                                    valueField: 'text'
                                },
                                {
                                    xtype: 'combobox',
                                    flex: 1,
                                    margins: '6',
                                    itemId: 'OESortOrder_1',
                                    name: 'OESortOrder_1',
                                    editable: false,
                                    store: [
                                        [
                                            '0',
                                            'Asc'
                                        ],
                                        [
                                            '1',
                                            'Desc'
                                        ]
                                    ]
                                },
                                {
                                    xtype: 'button',
                                    handler: function(button, event) {
                                        var container = this.up('fieldset');
                                        container.el.mask('Deleting Order Expression ....');
                                        var rowCount = container.items.length;
                                        var str = this.up('container').getItemId();
                                        var curID = parseInt(str.charAt(str.length-1));
                                        var arrSortOrder = [];
                                        var arrProName = [];

                                        for(var i=curID; i <= rowCount-1; i++)
                                        {
                                            var arrSortOrderList = Ext.ComponentQuery.query('#OESortOrder_'+i, container);
                                            arrSortOrder[i] =  arrSortOrderList[0].getValue();
                                            
                                            var arrProNameList = Ext.ComponentQuery.query('#OEProName_'+i, container);
                                            arrProName[i] =  arrProNameList[0].getValue();
                                            
                                            var OeDisList = Ext.ComponentQuery.query('#OE_'+i, container);
                                            OeDisList[0].destroy();
                                        }

                                        for(var j=curID+1; j <= rowCount-1; j++)
                                        {

                                            var addOeList = Ext.ComponentQuery.query('#OEAdd_0', container);
                                            var add = addOeList[0];                                     
                                            add.handler.call(add);

                                            var arrSortOrderList = Ext.ComponentQuery.query('#OESortOrder_'+(j-1), container);
                                            arrSortOrderList[0].setValue(arrSortOrder[j]);
                                            
                                            var arrProNameList = Ext.ComponentQuery.query('#OEProName_'+(j-1), container);
                                            arrProNameList[0].setValue(arrProName[j]);
                                        }
                                        //container.up('form').getForm().findField('oeExprCount').setValue(rowCount-1);
                                        var oeField = Ext.ComponentQuery.query('#oeExprCountId', container.up('form'));
                                        if(oeField[0].getValue() >= 1){
                                        	oeField[0].setValue(oeField[0].getValue() - 1);
                                        }else{
                                        	oeField[0].setValue(0);
                                        }
                                        //oeField[0].setValue(oeField[0].getValue() - 1);
                                        container.el.unmask();
                                    },
                                    margins: '6',
                                    border: false,
                                    itemId: 'OEDelete_1',
                                    icon: 'Content/img/16x16/delete-icon.png'
                                }
                            ]
                        }
                    ]
                },{
                	xtype: 'checkboxfield',
                	fieldLabel : 'Apply for all users',
                	name: 'isAdmin',
                	value: 'false',
                	margin: 5,
                },{
                	xtype: 'hiddenfield',
                	name: 'filterFor'
                },{
                	xtype: 'hiddenfield',
                	name: 'exprCount',
                	itemId: 'exprCountId',
                	value: 1
                },{
                	xtype: 'hiddenfield',
                	name: 'oeExprCount',
                	itemId: 'oeExprCountId',
                	value: 1
                }
            ]
        });

        me.callParent(arguments);
    },

    onPropertyName_1Select: function(combo, records, eOpts) {
        var itemId = combo.itemId;
        var length = itemId.length;
        var i = itemId.substring(13, length);


        if(combo.value === "Transfer Type")
        {
            var relationalStore = Ext.getStore('RelationalStoreforTransferType');
            relationalStore.load();
            var arrRelaOperList = Ext.ComponentQuery.query('#relationalOperator_'+(i),this);
            arrRelaOperList[0].bindStore(relationalStore);

        } else{
            var relationalStore = Ext.getStore('RelationalStoreComplete');
            relationalStore.load();
            var arrRelaOperList = Ext.ComponentQuery.query('#relationalOperator_'+(i),this);
            arrRelaOperList[0].bindStore(relationalStore);
        }


        /*
        utilsObj.scope;

        var propertyComboBoxList = Ext.ComponentQuery.query('#propertyName_1', this);
        var propertyComboBox = propertyComboBoxList[0];
        //var propertyComboBox = Ext.getCmp('propertyName_1');
        var propertyStore = propertyComboBox.getStore();

        var propertyProxy = propertyStore.getProxy();
        var url = 'getColumnNames?' + '&scope ='  +utilsObj.scope+ '&xid='+utilsObj.xid ;
        var urlWithTransferType = url  + '&sort='+ "false";
        propertyStore.getProxy().url = urlWithTransferType;

        propertyStore.load();
        */
    },

    onPropertyName_1AfterRender: function(component, eOpts) {
        //var propertyComboBox = Ext.getCmp('propertyName_1');
        var propertyComboBoxList = Ext.ComponentQuery.query('#propertyName_1', this);
        var propertyComboBox = propertyComboBoxList[0];
        var propertyStore = propertyComboBox.getStore();

        var propertyProxy = propertyStore.getProxy();
        var url = 'getColumnNames?' + '&scope ='  +utilsObj.scope+ '&xid='+utilsObj.xid ;
        var urlWithTransferType = url  + '&sort='+ "false";
        propertyStore.getProxy().url = urlWithTransferType;

        propertyComboBoxList[0].bindStore(propertyStore);
    },

    onPropertyName_1Change: function(field, newValue, oldValue, eOpts) {
        if(oldValue !== undefined)
        {
            var itemId = field.itemId;
            var length = itemId.length;
            var i = itemId.substring(13, length);


            var arrRelaOperList = Ext.ComponentQuery.query('#relationalOperator_'+(i),this);
            arrRelaOperList[0].setValue("");
        }


    }

});