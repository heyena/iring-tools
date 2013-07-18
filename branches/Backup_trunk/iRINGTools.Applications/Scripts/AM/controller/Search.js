Ext.define('AM.controller.Search', {
  extend: 'Ext.app.Controller',
  views: [
     'common.PropertyPanel',
     'common.ContentPanel',
     'common.CenterPanel',
     'search.SearchPanel',
     'search.SearchTree'
  ],
  stores: [],
  models: [
    'SearchModel'
  ],
  refs: [
    {
      ref: 'searchPanel',
      selector: 'viewport > centerpanel > searchpanel'
    },
    {
      ref: 'searchProperty',
      selector: 'viewport > centerpanel > searchpanel > propertypanel'
    },
    {
      ref: 'searchContent',
      selector: 'viewport > centerpanel > searchpanel > contentpanel'
    }
  ],
  init: function () {
    this.control({
      'button[action=search]': {
        click: this.onSearchRdl
      }
    });
  },

  onSearchRdl: function () {
    var pan = this.getSearchPanel(),
          searchText = pan.dockedItems.items[1].items.items[0].getValue(),
          isreset = pan.dockedItems.items[1].items.items[2].checked,
          content = this.getSearchContent(),
          propPanel = this.getSearchProperty(),
          value = pan.dockedItems.items[1].items.items[5].value;
    if (!searchText && searchText.length == 0) return;

    var conf = {
      title: searchText,
      id: 'tab_' + searchText
    };
    var tree = Ext.widget('searchtree', conf);
    tree.on('itemexpand', function () {
      content.getEl().unmask();
    }, this);
    tree.on('itemclick', this.onSearchClick, this);
    tree.on('beforeload', function (store, action) {
      // content.getEl().mask('Loadindg...');
      var repo = null;
      if (action.node.data.type != "SearchNode" && action.node.data.type != "TemplateNode") {
        repo = action.node.data.text;
        if (repo && repo.indexOf("[") == -1) {
          repo = action.node.parentNode.data.text;
        }
      }

      store.proxy.extraParams.type = (action.node.data.type == "" ? 'SearchNode' : action.node.data.type);
      if (searchText != undefined && searchText != '') {
        store.proxy.extraParams.query = searchText;
        store.proxy.extraParams.reset = isreset;
        store.proxy.extraParams.limit = value;        
      }
      if (repo)
        store.proxy.extraParams.repositoryName = repo.substring(repo.indexOf("[") + 1, repo.indexOf("]"));
        
      if (action.node.parentNode && (action.node.data.identifier == null || action.node.data.identifier == '')) {
        store.proxy.extraParams.id = action.node.parentNode.data.identifier;
      } else {
        store.proxy.extraParams.id = action.node.data.identifier;
      }

    }, this);
    tree.on('beforeitemexpand', function () {
      //content.getEl().mask('Ldddddoading...'); 
    }, this);
    tree.on('load', function (store, model) {
      content.getEl().unmask();
      if (model.data.type == "ClassNode") {
        try {
          if (model.childNodes.length > 0) {
            propPanel.setSource(model.childNodes[0].data.record);
          }
          else {
            propPanel.setSource(model.data.record);
          }
        } catch (ex) { }
      }
    }, this);

    var exist = content.items.map[conf.id];
    if (exist == null) {
      content.add(tree).show();
    } else {
      exist.show();
    }
//  tree.store.getProxy().url  = 'refdata/getnode';
//  tree.store.load();

  tree.getStore().load();

  },

  onSearchClick: function (view, model, n, idx) {
    var content = this.getSearchContent(),
      propPanel = this.getSearchProperty();
    var node = model.store.getAt(idx);
    try {
      node.on('expand', function () {
        content.getEl().unmask();
      }, this);
      if (node.data.type == "ClassNode" && model.firstChild) {
        propPanel.setSource(model.firstChild.data.record);
      } else {
        propPanel.setSource(node.data.record);
      }
    } catch (e) { }
    node.expand();
  }
});