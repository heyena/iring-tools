Ext.ns('AdapterManager');
/**
* @class AdapterManager.DirectoryPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.DirectoryPanel = Ext.extend(Ext.Panel, {
    title: 'Directory',
    width: 220,

    collapseMode: 'mini',
    collapsible: true,
    collapsed: false,

    layout: 'border',
    border: true,
    split: true,

    navigationUrl: null,
    DirectoryPanel: null,

    /**
    * initComponent
    * @protected
    */
    initComponent: function () {

        this.addEvents({
            create: true,
            open: true,
            remove: true
        });

        this.tbar = this.buildToolbar();

        this.DirectoryPanel = new Ext.tree.TreePanel({
            region: 'center',
            collapseMode: 'mini',
            height: 200,
            layout: 'fit',
            border: false,

            rootVisible: true,
            lines: true,
            //singleExpand: true,
            useArrows: true,

            loader: new Ext.tree.TreeLoader({
                dataUrl: this.navigationUrl
            }),

            root: {
                nodeType: 'async',
                text: 'Scopes',
                expanded: true,
                draggable: false,
                icon: 'Content/img/internet-web-browser.png',
                id: 'src'
            }

        });

        this.items = [
      this.DirectoryPanel
    ];

        // super
        AdapterManager.DirectoryPanel.superclass.initComponent.call(this);
    },

    buildToolbar: function () {
        return [
      {
          text: 'New',
          handler: this.onCreate,
          //icon: 'Content/img/list-add.png',
          scope: this
      },
      {
          text: 'Open',
          handler: this.onOpen,
          //icon: 'Content/img/document-properties.png',
          scope: this
      },
      {
          text: 'Delete',
          handler: this.onRemove,
          //icon: 'Content/img/list-remove.png',
          scope: this
      }
    ]
    },

    onCreate: function (btn, ev) {
        var node = this.DirectoryPanel.getSelectionModel().getSelectedNode();
        var formType = "newForm";
        this.fireEvent('create', this, node, formType);
    },

    onOpen: function (btn, ev) {
        var node = this.DirectoryPanel.getSelectionModel().getSelectedNode();
        var formType = "editForm";
        this.fireEvent('open', this, node, formType);
    },

    onRemove: function (btn, ev) {
        var node = this.DirectoryPanel.getSelectionModel().getSelectedNode();
        this.fireEvent('remove', this, node);
    },

    /**
    * buildform
    * @private
    */
    buildForm: function (node, formType) {

        var name, scopeDescription;
        name = "";
        description = "";

        var obj = node.attributes;

        if (node.attributes.type == 'Application') {
            name = node.attributes.ScopeApplication.Name;
            scopeDescription = node.attributes.ScopeApplication.Description;
        }
        else {
            name = node.attributes.Scope.Name;
            scopeDescription = node.attributes.Scope.Description;
        }

        return [
        { xtype: 'hidden', name: 'formType', value: formType },
        { xtype: 'hidden', name: 'nodeID', value: obj['id'] },
        { xtype: 'hidden', name: 'parentNodeID', value: node.parentNode.id },
        { fieldLabel: 'Application Name', name: 'appName', allowBlank: true, xtype: 'textfield', width: 250, value: name },
        { fieldLabel: 'Description', name: 'description', allowBlank: true, xtype: 'textarea', width: 250, value: scopeDescription }

        ];
    }



});