Ext.ns('AdapterManager');
/**
* @class FederationManager.SearchPanel
* @author by Aswini Nayak
*/
var treeLoader, searchText;
AdapterManager.SearchPanel = Ext.extend(Ext.Panel, {
  title: 'Reference Data Search',
  layout: 'border',
  border: true,
  split: true,
  searchUrl: null,
  limit: 100,
  refClassTabPanel: null,
  propertyPanel: null,
  searchStore: null,
  /**
  * initComponent
  * @protected
  */
  initComponent: function () {
    this.tbar = this.buildToolbar();
    this.propertyPanel = new Ext.grid.PropertyGrid({
      id: 'class-property-panel',
      title: 'Details',
      region: 'east',
      // layout: 'fit',
      stripeRows: true,
      collapsible: true,
      autoScroll: true,
      width: 450,
      split: true,
      bodyBorder: true,
      collapsed: false,
      border: true,
      frame: true,
      source: {},
      listeners: {
        // to disable editable option of the property grid
        beforeedit: function (e) {
          e.cancel = true;
        }
      }
    });

    this.refClassTabPanel = new Ext.TabPanel({
      id: 'content-pane',
      region: 'center',
      deferredRender: false,
      enableTabScroll: true,
      border: true,
      activeItem: 0
    });


    this.items = [this.refClassTabPanel, this.propertyPanel];

    // super
    AdapterManager.SearchPanel.superclass.initComponent.call(this);
  },
  buildToolbar: function () {
    return [
                 {
                   xtype: 'textfield',
                   allowBlank: false,
                   blankText: 'This field can not be blank',
                   name: 'referencesearch',
                   id: 'referencesearch',
                   style: {
                     marginLeft: '15px'
                   },
                   scope: this,
                   listeners: {
                     specialkey: function (f, e) {
                       if (e.getKey() == e.ENTER) {
                         var query = Ext.get('referencesearch').getValue();
                         //alert(query);
                       }
                     }
                   }
                 },
            	 {
            	   xtype: 'checkbox',
            	   boxLabel: 'Reset',
            	   name: 'reset',
            	   style: {
            	     marginRight: '5px',
            	     marginLeft: '3px'
            	   }
            	 },
                {
                  xtype: "tbbutton",
                  text: 'Search',
                  handler: this.onSearch,
                  scope: this,
                  style: {
                    marginLeft: '5px'
                  }

                }];
  },
  onSearch: function () {
    searchText = Ext.get('referencesearch').getValue();
    treeLoader = new Ext.tree.TreeLoader({
      requestMethod: 'POST',
      url: this.searchUrl,
      baseParams: {
        type: null,
        query: searchText,
        limit: this.limit,
        start: 0
      }
    });
    var tree = new Ext.tree.TreePanel({
      title: searchText,
      useArrows: true,
      animate: true,
      lines: false,
      id: 'tab_' + searchText,
      autoScroll: true,
      style: 'padding-left:5px;',
      border: false,
      closable: true,
      rootVisible: false,
      loader: treeLoader,
      root: {
        nodeType: 'async',
        qtipCfg: 'Aswini',
        draggable: false
      },
      containerScroll: true
    });

    //	tree.on('beforeexpandnode', this.restrictExpand, this);

    tree.on('beforeload', function (node) {
      Ext.getCmp('content-pane').getEl().mask('Loading...');
    });
    tree.on('load', function (node) {
      Ext.getCmp('content-pane').getEl().unmask();
    });
    tree.getRootNode().expand();
    tree.on('click', this.onClick, this);
    this.refClassTabPanel.add(tree).show();
  },
  onClick: function (node) {
    try {
      this.propertyPanel.setSource(node.attributes.record);
    } catch (e) {
    };
    switch (node.attributes.text) {
      case "Classifications":
        // alert("send request for classifications:"+'class/'+node.parentNode.attributes.identifier);
        treeLoader.url = "refdata/classes";
        treeLoader.baseParams = {
          id: node.parentNode.attributes.identifier,
          query: searchText,
          limit: this.limit,
          start: 0,
          type: node.attributes.type
        };
        break;
      case "Superclasses":
        //alert("send request for Superclasses:"+'superClass/'+node.parentNode.attributes.identifier);
        treeLoader.url = "refdata/superClasses";
        treeLoader.baseParams = {
          id: node.parentNode.attributes.identifier,
          query: searchText,
          limit: this.limit,
          start: 0
        };

        break;
      case "Subclasses":
        //alert("send request for Subclasses:"+'subClasses/'+node.parentNode.attributes.identifier);
        treeLoader.url = "refdata/subClasses";
        treeLoader.baseParams = {
          id: node.parentNode.attributes.identifier,
          query: searchText,
          limit: this.limit,
          start: 0
        };
        break;
      case "Templates":
        //alert("send request for Subclasses:"+'subClasses/'+node.parentNode.attributes.identifier);
        treeLoader.url = "refdata/templates";
        treeLoader.baseParams = {
          id: node.parentNode.attributes.identifier,
          query: searchText,
          limit: this.limit,
          start: 0
        };
        break;
      default:
        if (node.attributes.type == 'templateNode') {
          treeLoader.url = "refdata/roles";
          treeLoader.baseParams = {
            id: node.attributes.identifier,
            query: searchText,
            limit: this.limit,
            start: 0
          };
        };
        break;



    }
    node.expand();
  }

});