Ext.ns('AdapterManager');

/**
* @class AdapterManager.ExcelSourcePanel
* @extends FormPanel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.ExcelSourcePanel = Ext.extend(Ext.FormPanel, {
    width: 120,
    layout: 'fit',

    frame: true,
    border: true,

    fileUpload: true,
    labelWidth: 150, // label settings here cascade unless    
    method: 'POST',
    bodyStyle: 'padding:10px 5px 0',

    border: false, // removing the border of the form
    defaults: {
        width: 330,
        msgTarget: 'side'
    },
    defaultType: 'textfield',
    buttonAlign: 'left', // buttons aligned to the left            
    autoDestroy: false,

    scope: null,
    application: null,
    dataLayer: null,

    btnNext: null,
    btnPrev: null,

    /**
    * initComponent
    * @protected
    */
    initComponent: function () {

        this.addEvents({
            uploaded: true
        });

        var scope = "";

        if (this.scope != null) {
            scope = this.scope.Name;
        }

        var application = "";
        var dataLayer = "";

        if (this.application != null) {
            application = this.application.Name;
            dataLayer = this.application.Assembly;
        }

        this.btnUpload = new Ext.Button({
            text: 'Upload',
            //icon: 'resources/images/16x16/document-save.png',
            //tooltip: 'Save',
            disabled: false,
            handler: this.onUpload,
            scope: this
        });

        this.btnCancel = new Ext.Button({
            text: 'Cancel',
            //icon: 'resources/images/16x16/document-save.png',
            //tooltip: 'Save',
            disabled: true,
            scope: this
        });

        this.bbar = new Ext.Toolbar();
        this.bbar.addButton(new Ext.Toolbar.Fill());
        this.bbar.addButton(this.btnUpload);
        this.bbar.addButton(this.btnCancel);

        this.items = [
            { xtype: 'hidden', name: 'Scope', value: scope },
            { xtype: 'hidden', name: 'Application', value: application },
            { xtype: 'hidden', name: 'DataLayer', value: dataLayer },
            {
                xtype: 'fileuploadfield',
                name: 'SourceFile',
                emptyText: 'Select an Excel Source File',
                fieldLabel: 'Excel Source File',
                buttonText: '',
                buttonCfg: {
                    iconCls: 'upload-icon'
                }
            },
            { xtype: 'checkbox', name: 'Generate', boxLabel: 'Generate Configuration' }
        ];

        // super
        AdapterManager.ExcelSourcePanel.superclass.initComponent.call(this);
    },
    
    onUpload: function () {
        
        that = this;

        this.getForm().submit({
            waitMsg: 'Uploading file...',
            url: this.url,
            success: function (f, a) {
                that.fireEvent('Uploaded', that, f.items.items[3].value);            },
            failure: function (f, a) {
                Ext.Msg.alert('Warning', 'Error uploading file "' + f.items.items[3].value + '"!');
            }
        });
    }

});

/**
* @class AdapterManager.ExcelLibraryPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.ExcelLibraryPanel = Ext.extend(Ext.Panel, {
    title: 'ExcelLibrary',
    width: 120,

    collapseMode: 'mini',
    //collapsible: true,
    //collapsed: false,
    closable: true,

    layout: 'border',
    border: true,
    split: true,

    scope: null,
    application: null,    
    configurationPanel: null,
    propertyPanel: null,
    url: null,

    btnNext: null,
    btnPrev: null,

    /**
    * initComponent
    * @protected
    */
    initComponent: function () {

        this.addEvents({
            save: true,
            reset: true,
            validate: true,
            refresh: true,
            selectionchange: true
        });

        var scope = "";

        if (this.scope != null) {
            scope = this.scope.Name;
        }

        var application = "";
        var dataLayer = "";

        if (this.application != null) {
            application = this.application.Name;
            dataLayer = this.application.Assembly;
        }

        this.tbar = this.buildToolbar();

        this.workbookMenu = new Ext.menu.Menu();
        this.workbookMenu.add(this.buildWorkbookMenu());

        this.worksheetMenu = new Ext.menu.Menu();
        this.worksheetMenu.add(this.buildWorksheetMenu());

        this.columnMenu = new Ext.menu.Menu();
        this.columnMenu.add(this.buildColumnMenu());

        this.treeLoader = new Ext.tree.TreeLoader({
            baseParams: {
                scope: scope,
                application: application,
                type: null
            },
            url: 'excel/getnode'
        });

        this.treeLoader.on("beforeload", function (treeLoader, node) {
            treeLoader.baseParams.type = node.attributes.type;
        }, this);

        this.rootNode = new Ext.tree.AsyncTreeNode({
            id: 'root',
            text: 'Workbook',
            expanded: true,
            draggable: false,
            icon: 'Content/img/excel.png',
            type: 'ExcelWorkbookNode'
        });

        this.configurationPanel = new Ext.tree.TreePanel({
            layout: 'fit',
            region: 'center',
            border: false,
            split: true,
            lines: true,
            expandAll: true,
            rootVisible: true,
            lines: true,
            autoScroll: true,
            useArrows: true,
            loader: this.treeLoader,
            root: this.rootNode
        });

        this.configurationPanel.on('contextmenu', this.showContextMenu, this);
        this.configurationPanel.on('click', this.onClick, this);
       
        this.propertyPanel = new Ext.grid.PropertyGrid({
            title: 'Details',
            region: 'east',            
            width: 200,
            split: true,
            collapseMode: 'mini',
            stripeRows: true,
            collapsible: true,
            autoScroll: true,
            border: false,
            frame: false,
            height: 150,
            // bodyStyle: 'padding-bottom:15px;background:#eee;',
            source: {},
            listeners: {
                // to disable editable option of the property grid
                beforeedit: function (e) {
                    e.cancel = true;
                }
            }
        });
        
        this.items = [
            this.configurationPanel,
            this.propertyPanel
        ];

        // super
        AdapterManager.ExcelLibraryPanel.superclass.initComponent.call(this);
    },
    
    buildToolbar: function () {
        return [
      {
          text: 'Reload',
          handler: this.onReload,
          icon: 'Content/img/16x16/view-refresh.png',
          scope: this
      },
      {
          text: 'Save',
          handler: this.onSave,
          icon: 'Content/img/16x16/document-save.png',
          scope: this
      },
      {
          text: 'Upload',
          handler: this.onUpload,
          //icon: 'Content/img/list-remove.png',
          scope: this
      }      
    ]
    },

    buildWorkbookMenu: function () {
        return [
            {
                text: 'Add Worksheet(s)',
                handler: this.onAddWorkSheets,
                icon: 'Content/img/16x16/document-new.png',
                scope: this
            },
            {
                text: 'Reload Worksheets',
                handler: this.onReloadNode,
                icon: 'Content/img/16x16/view-refresh.png',
                scope: this
            }
        ]
    },

    buildWorksheetMenu: function () {
        return [
            {
                text: 'Edit Worksheet',
                handler: this.onEditWorksheet,
                icon: 'Content/img/16x16/document-open.png',
                scope: this
            },
            {
                text: 'Remove Worksheet',
                handler: this.onRemoveWorksheet,
                icon: 'Content/img/16x16/edit-delete.png',
                scope: this
            },
            {
                text: 'Reload Worksheet',
                handler: this.onReloadNode,
                icon: 'Content/img/16x16/view-refresh.png',
                scope: this
            },
            {
                xtype: 'menuseparator'
            },
            {
                text: 'Add Worksheet(s)',
                handler: this.onAddWorksheets,
                icon: 'Content/img/16x16/document-new.png',
                scope: this
            },
            {
                text: 'Add Column(s)',
                handler: this.onAddColumns,
                icon: 'Content/img/16x16/document-new.png',
                scope: this
            }
        ]
    },

    buildColumnMenu: function () {
        return [
            {
                text: 'Rename Column',
                handler: this.onRenameColumn,
                icon: 'Content/img/16x16/document-open.png',
                scope: this
            },
            {
                text: 'Remove Column',
                handler: this.onRemoveColumn,
                icon: 'Content/img/16x16/edit-delete.png',
                scope: this
            },
            {
                text: 'Reload Column',
                handler: this.onReloadNode,
                icon: 'Content/img/16x16/view-refresh.png',
                scope: this
            },
            {
                xtype: 'menuseparator'
            },
            {
                text: 'Add Column(s)',
                handler: this.onAddColumns,
                icon: 'Content/img/16x16/document-new.png',
                scope: this
            }
        ]
    },

    onAddWorksheets: function(node) {
    },

    onAddColumns: function(node) {
    },

    onEditWorksheet: function(node) {
    },

    onRemoveWorksheet: function(node) {
    },

    onRenameColumn: function(node) {
    },

    onRemoveColumn: function(node) {
    },

    onReload: function () {
        this.configurationPanel.root.reload();
    },

    onReloadNode: function (node) {
        node.reload();
    },

    onUpload: function (panel) {
        
        var form = new AdapterManager.ExcelSourcePanel({
            scope: this.scope,
            application: this.application,            
            url: 'excel/upload'
        });
                
        var newWin = new Ext.Window({
            width: 400,
            layout: 'fit',            
            height:300,
            autoScroll:true,
            modal: true,
            items: form
        });

        form.on('uploaded', function() {
            newWin.close();
            this.configurationPanel.root.reload();
        }, this);

        newWin.show();
    },

    onSave: function (panel) {

        Ext.Ajax.request({
            url: 'excel/configure',    // where you wanna post
            method: 'POST',
            success: function (f, a) {

            },   // function called on success
            failure: function (f, a) {

            },
            params: {
                Scope: this.scope.Name,
                Application: this.application.Name,
                DataLayer: this.application.Assembly
            }
        });

    },
    
    showContextMenu: function (node, event) {

        //  if (node.isSelected()) { 
        var x = event.browserEvent.clientX;
        var y = event.browserEvent.clientY;

        var obj = node.attributes;

        if (obj.type == "ExcelWorkbookNode") {
            this.workbookMenu.showAt([x, y]);
        } else if (obj.type == "ExcelWorksheetNode") {
            this.worksheetMenu.showAt([x, y]);
        } else if (obj.type == "ExcelColumnNode") {
            this.columnMenu.showAt([x, y]);
        }
        //}
    },

    onClick: function (node) {
        try {
            this.propertyPanel.setSource(node.attributes.record);
        } catch (e) {
            //  alert(e);
        }
    }  

});
