Ext.ns('AdapterManager');
/**
* @class AdapterManager.SearchPanel
* @author by Neha Bhardwaj
*/
AdapterManager.SearchPanel = Ext.extend(Ext.Panel, {
  title: 'Reference Data Search',
  layout: 'border',
  border: false,
  frame: false,
  split: true,
  searchUrl: null,
  limit: 500,
  refClassTabPanel: null,
  propertyPanel: null,
  searchStore: null,
  // contextButton: null,
  contextClassMenu: null,
  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

    this.contextClassMenu = new Ext.menu.Menu();
    this.contextClassMenu.add(this.buildClassContextMenu());

    this.propertyPanel = new AdapterManager.NameValueGrid({
        title: 'Details',
        region: 'east',
        width: 350,
        frame: false,
        border: true,
        split: true,
        collapsible: true
    }),

    this.refClassTabPanel = new Ext.TabPanel({
      id: 'content-pane',
      deferredRender: false,
      enableTabScroll: true,
      frame: false,
      border: true,
      activeItem: 0,
      iconCls: 'tabsClass'
    });

    this.mainPanel = new Ext.Panel({
      region: 'center',
      autoScroll: true,
      frame: false,
      border: false,
      layout: 'fit',
      items: [this.refClassTabPanel]
    });

    this.tbar = new Ext.Toolbar();
    this.tbar.add(this.buildToolbar());
    this.items = [this.mainPanel, this.propertyPanel];

    AdapterManager.SearchPanel.superclass.initComponent.call(this);
},

  buildToolbar: function () {
    var that = this;
    return [
                 {
                   xtype: 'textfield',
                   width: 300,
                   name: 'referencesearch',
                   id: 'referencesearch',
                   scope: this,
                   listeners: {
                     specialkey: function (f, e) {
                       if (e.getKey() == e.ENTER) {
                         that.onSearch();
                       }
                     }
                   }
                 },
            	 {
            	   xtype: "button",
            	   text: 'Search',
            	   icon: 'Content/img/16x16/document-properties.png',
            	   handler: this.onSearch,
            	   scope: this,
            	   style: {
            	     marginLeft: '5px'
            	   }

            	 }, {
            	   xtype: 'checkbox',
            	   //boxLabel:'Reset',
            	   name: 'reset',
            	   id: 'reset',
            	   style: {
            	     marginLeft: '3px',
            	     marginBottom: '6px'
            	   }
            	 },
                {
                  xtype: 'label',
                  text: 'Reset',
                  style: {
                    marginRight: '5px'
                  }

                }];
  },

  buildClassContextMenu: function () {
    return [{
      text: 'Promote',
      handler: this.onPromote,
      icon: 'Content/img/16x16/promote-icon.png',
      scope: this
    }];
  },


  getActiveTab: function () {
    return this.refClassTabPanel.getActiveTab();
  },

  getSelectedNode: function () {
    var activeTab = this.getActiveTab();
    if (activeTab != undefined)
      return activeTab.getSelectionModel().getSelectedNode();
  },

  showContextMenu: function (node, event) {
    if (node.isSelected()) {
      var x = event.browserEvent.clientX;
      var y = event.browserEvent.clientY;
      if (node.attributes.type == "ClassNode")
        this.contextClassMenu.showAt([x, y]);
    }
  },

  onSearch: function () {
    var searchText = Ext.get('referencesearch').getValue();
    var isreset = document.getElementById('reset').checked;
    // alert(isreset);

    if (searchText != '') {
      var treeLoader = new Ext.tree.TreeLoader({
        requestMethod: 'POST',
        url: this.searchUrl,
        baseParams: {
          id: null,
          type: null,
          repositoryName: null,
          query: searchText,
          reset: isreset,
          limit: this.limit,
          start: 0
        }
      });

      treeLoader.on("beforeload", function (treeLoader, node) {
        var repo = null;
        if (node.attributes.type != "SearchNode" && node.attributes.type != "TemplateNode") {
          repo = node.attributes.text;
          if (repo && repo.indexOf("[") == -1) {
            repo = node.parentNode.attributes.text;
          }
        }
        treeLoader.baseParams.type = node.attributes.type;
        treeLoader.baseParams.query = searchText;
        treeLoader.baseParams.reset = isreset;
        treeLoader.baseParams.limit = this.limit;
        treeLoader.baseParams.start = 0;
        if (repo)
          treeLoader.baseParams.repositoryName = repo.substring(repo.indexOf("[") + 1, repo.indexOf("]"));
        if (node.parentNode && node.attributes.identifier == null) {
          treeLoader.baseParams.id = node.parentNode.attributes.identifier;
        } else {
          treeLoader.baseParams.id = node.attributes.identifier;
        }
      }, this);

      var tree = new Ext.tree.TreePanel({
        title: searchText,
        enableDrag: true,
        ddGroup: 'refdataGroup',
        animate: true,
        lines: true,
        id: 'tab_' + searchText,
        autoScroll: true,
        style: 'padding-left:5px;',
        border: false,
        closable: true,
        rootVisible: false,
        loader: treeLoader,
        root: {
          nodeType: 'async',
          // draggable: true,
          type: 'SearchNode'
        },
        containerScroll: true
      });

      //	tree.on('beforeexpandnode', this.restrictExpand, this);

      tree.on('beforeload', function (node) {
        Ext.getCmp('content-pane').getEl().mask('Loading...');

      });
      tree.on('load', function (node) {
        Ext.getCmp('content-pane').getEl().unmask();

        // update the detail's panel with All properties
        if (node.attributes.type == "ClassNode") {
          try {
            if (node.childNodes.length > 0) {
              this.propertyPanel.setSource(node.childNodes[0].attributes.record);
            }
          } catch (e) { }
        }
      }, this);
      tree.getRootNode().expand();
      tree.on('click', this.onClick, this);
      this.refClassTabPanel.add(tree).show();
      tree.on('contextmenu', this.showContextMenu, this);
    }
  },
  onClick: function (node) {
    var obj = node.attributes;
    try {
      if (obj.type == "ClassNode" && node.childNodes.length > 0) {
        this.propertyPanel.setSource(node.childNodes[0].attributes.record);
      } else {
        this.propertyPanel.setSource(node.attributes.record);
      }
      //      this.contextButton.menu.removeAll();
      //      if (obj.type == 'ClassNode') {
      //        this.contextButton.menu.add(this.buildClassContextMenu());
      //      }
    } catch (e) { }
    node.expand();
  }

});