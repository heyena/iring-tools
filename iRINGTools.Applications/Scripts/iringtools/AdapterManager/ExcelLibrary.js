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

    btnNext: null,
    btnPrev: null,

    /**
    * initComponent
    * @protected
    */
    initComponent: function () {

        this.addEvents({
            next: true
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

        this.btnNext = new Ext.Button({
            text: 'Next',
            //icon: 'resources/images/16x16/document-save.png',
            //tooltip: 'Save',
            disabled: false,
            handler: this.onNext,
            scope: this
        });

        this.btnPrev = new Ext.Button({
            text: 'Prev',
            //icon: 'resources/images/16x16/document-save.png',
            //tooltip: 'Save',
            disabled: true,
            scope: this
        });

        this.bbar = new Ext.Toolbar();
        this.bbar.addButton(new Ext.Toolbar.Fill());
        this.bbar.addButton(this.btnPrev);
        this.bbar.addButton(this.btnNext);

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
            }
        ];

        // super
        AdapterManager.ExcelSourcePanel.superclass.initComponent.call(this);
    },
    
    onNext: function () {
        
        that = this;

        this.getForm().submit({
            waitMsg: 'Uploading file...',
            url: this.url,
            success: function (f, a) {
                Ext.Msg.alert('Success', 'Uploaded file "' + f.items.items[3].value + '" to the server');
                that.fireEvent('Next', that, f.items.items[3].value);
            },
            failure: function (f, a) {
                Ext.Msg.alert('Warning', 'Error uploading file!')
            }
        });
    }

});

/**
* @class AdapterManager.ConfigurationPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.ConfigurationPanel = Ext.extend(Ext.Panel, {
    title: 'Directory',
    width: 220,

    collapseMode: 'mini',
    collapsible: true,
    collapsed: false,

    layout: 'border',
    border: true,
    split: true,

    scope: null,
    application: null,
    path: null,
    url: null,

    configurationPanel: null,
    rootNode: null,
    treeLoader: null,

    /**
    * initComponent
    * @protected
    */
    initComponent: function () {

        this.addEvents({            
            ReloadNode: true,
            Next: true            
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

        var path = "";

        if (this.path != null) {
            path = this.path;
        }

        this.tbar = this.buildToolbar();

        this.btnNext = new Ext.Button({
            text: 'Next',
            //icon: 'resources/images/16x16/document-save.png',
            //tooltip: 'Save',
            disabled: false,
            handler: this.onNext,
            scope: this
        });

        this.btnPrev = new Ext.Button({
            text: 'Prev',
            //icon: 'resources/images/16x16/document-save.png',
            //tooltip: 'Save',
            disabled: true,
            scope: this
        });

        this.bbar = new Ext.Toolbar();
        this.bbar.addButton(new Ext.Toolbar.Fill());
        this.bbar.addButton(this.btnPrev);
        this.bbar.addButton(this.btnNext);
                
        this.treeLoader = new Ext.tree.TreeLoader({
            baseParams: { 
                scope: scope, 
                application: application,
                path: path,
                type: null 
            },
            url: this.url
        });

        this.treeLoader.on("beforeload", function (treeLoader, node) {
            treeLoader.baseParams.type = node.attributes.type;
        }, this);

        this.rootNode = new Ext.tree.AsyncTreeNode({
            id: 'root',
            text: 'Workbook',
            expanded: true,
            draggable: false,
            icon: 'Content/img/internet-web-browser.png',
            type: 'ExcelWorkbookNode'
        });

        this.configurationPanel = new Ext.tree.TreePanel({
            region: 'center',
            collapseMode: 'mini',
            height: 300,
            layout: 'fit',
            border: false,
            split: true,
            expandAll: true,
            rootVisible: true,
            lines: true,
            autoScroll: true,            
            useArrows: true,
            loader: this.treeLoader,
            root: this.rootNode            
        });

        this.configurationPanel.on('contextmenu', this.showContextMenu, this);

        this.items = [
            this.configurationPanel
        ];

        var state = Ext.state.Manager.get("AdapterManager");

        if (state) {
            if (this.configurationPanel.expandPath(state) == false) {
                Ext.state.Manager.clear("AdapterManager");
                this.configurationPanel.root.reload();
            }
        }

        // super
        AdapterManager.ConfigurationPanel.superclass.initComponent.call(this);
    },

    buildToolbar: function () {
        return [
      {
          text: 'Reload',
          handler: this.onReload,
          //icon: 'Content/img/list-remove.png',
          scope: this
      }
    ]
    },
        
    onReload: function () {        
        this.configurationPanel.root.reload();
    },

    onReloadNode: function (node) {        
        node.reload();
    }, 

    onNext: function (panel) {        
        var nodes = this.configurationPanel.getChecked("record");

        Ext.Ajax.request({
            url: 'excel/configure',    // where you wanna post
            success: function(f, a) {
                
            },   // function called on success
            failure: function(f, a) {
                
            },
            params: { jsonData: Ext.encode(nodes) }  // your json data
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

    layout: 'card',
    activeItem: 0,
    border: true,
    split: true,

    scope: null,
    application: null,
    form: null,
    configurationPanel: null,
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

        this.form = new AdapterManager.ExcelSourcePanel({
            scope: this.scope,
            application: this.application,            
            url: 'excel/source',
        });

        this.form.on('Next', this.onNext, this);

        this.items = [
            this.form
        ];

        // super
        AdapterManager.ExcelLibraryPanel.superclass.initComponent.call(this);
    },

    onNext: function(panel, path) {
        var configPanel = new  AdapterManager.ConfigurationPanel({
            scope: this.scope,
            application: this.application,
            path: path,
            url: 'excel/getnode'
        });

        this.items.add(configPanel);

        var idx = this.items.getCount() - 1;
        this.layout.setActiveItem(idx);
        this.activeItem = idx;        
    }

});
