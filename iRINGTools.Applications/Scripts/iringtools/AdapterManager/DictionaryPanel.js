Ext.ns('AdapterManager');
/**
* @class AdapterManager.DictionaryPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.DictionaryPanel = Ext.extend(Ext.Panel, {
  title: 'Dictionary',
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

  dictionaryPanel: null,
  propertiesPanel: null,
  url: null,

  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

    this.addEvents({
      close: true,
      save: true,
      reset: true,
      validate: true,
      tabChange: true,
      refresh: true,
      selectionchange: true,
      configure: true
    });

    this.tbar = this.buildToolbar();

    var scope = "";

    if (this.scope != null) {
      scope = this.scope.Name;
    }

    var application = "";

    if (this.application != null) {
			application = this.application.Name;
    }

    this.treeLoader = new Ext.tree.TreeLoader({
			baseParams: { 
				type: null,
				scope: null,
				application: null
			},
			url: this.navigationUrl
    });

    this.treeLoader.on("beforeload", function (treeLoader, node) {
      treeLoader.baseParams.type = node.attributes.type;            
    }, this);

    this.rootNode = new Ext.tree.AsyncTreeNode({
      id: 'root',
      text: 'Dictionary',
      expanded: true,
      draggable: false,
      icon: 'Content/img/internet-web-browser.png',
      type: 'DictionaryNode'
    });

    this.dictionaryPanel = new Ext.tree.TreePanel({
      region: 'center',
      collapseMode: 'mini',
      height: 300,
      layout: 'fit',
      lines: true,
      border: false,
      split: true,
      expandAll: true,
      rootVisible: true,
      lines: true,
      autoScroll: true,
      //singleExpand: true,     
      loader: this.treeLoader,
      root: this.rootNode
    });

    this.propertiesPanel = new Ext.Panel({
      region: 'east',
      height: 300,
      layout: 'fit'
    });

    this.items = [
      this.dictionaryPanel,
      this.propertiesPanel
  	];

    this.on('close', this.onCloseTab, this)

    // super
    AdapterManager.DictionaryPanel.superclass.initComponent.call(this);
	},

  buildToolbar: function () {
    return [{
      xtype: "tbbutton",
      text: 'Save',
      icon: 'resources/images/16x16/document-save.png',
      tooltip: 'Save',
      disabled: false,
      handler: this.onSave,
      scope: this
    }, {
      xtype: "tbbutton",
      text: 'Reset',
      icon: 'resources/images/16x16/edit-clear.png',
      tooltip: 'Reset',
      disabled: false,
      handler: this.onReset,
      scope: this
		}]
  },

  getActiveTab: function () {
    if (Ext.getCmp('content-panel').items.length != 0) { // check is there any tab in contentPanel
      return Ext.getCmp('content-panel').getActiveTab();
    }
    else {
      return false;
    }
  },

  onCloseTab: function (node) {
    // check number of tabs in panel to make disabled the centerPanel if its the last tab has been closed.
    if ((Ext.getCmp('content-panel').items.length) == 1) {
        Ext.getCmp('content-panel').enable()
    }
  },

  onReset: function () {
    this.form.getForm().reset();
  },

  onConfigure: function () {
    this.fireEvent('configure', this, this.scope, this.record);
  },

  onSave: function () {
        
  }

});