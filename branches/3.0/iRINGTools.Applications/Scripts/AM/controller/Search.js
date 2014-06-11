Ext.define('AM.controller.Search', {
    extend: 'Ext.app.Controller',

    models: [
    'SearchModel'
  ],
    stores: [
    'SearchStore',
    'SearchCmbStore'
  ],
    views: [
    'search.SearchPanel',
    'search.SearchToolbar',
    'search.SearchTree'
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

    onSearchRdl: function (button, e, eOpts) {
        var me = this,
            pan = me.getSearchPanel(),
            tree,
            dockItem = pan.items.items[2].dockedItems[0],
            searchToolbar = pan.items.items[2].dockedItems.items[0],
	        searchText = searchToolbar.items.items[0].value,
            isreset = searchToolbar.items.items[2].checked,
            content = me.getSearchContent(),
            propPanel = me.getSearchProperty(),
            value = pan.down('#searchLimitCombo').value;

        if (!searchText && searchText.length === 0) return;

        var conf = {
            title: searchText,
            id: 'tab_' + searchText
        };
        var exist = content.items.map[conf.id];
        if (exist) {
            tree = exist;
        } else {
            tree = Ext.widget('searchtree', conf);
            content.add(tree);
        }
        tree.on('itemexpand', function () {
            pan.getEl().unmask();
        }, me);
        tree.on('itemclick', me.onSearchClick, me);
        tree.on('beforeload', function (store, action) {
            pan.getEl().mask('Loading...');
            var repo = null;
            var params = store.proxy.extraParams;
            if (action.node.data.type != "SearchNode" &&
                  action.node.data.type != "TemplateNode" &&
                  action.node.data.type != "SubclassesNode") {
				  if(action.node.data.record)
					repo = action.node.data.record.Repository;
            }

            params.type = (action.node.data.type === '' ? 'SearchNode' : action.node.data.type);
            if (searchText !== undefined && searchText !== '') {
                params.query = searchText;
                params.reset = isreset;
                params.limit = value;
            }
            if (repo) {
                params.repositoryName = repo;
            }
            if (action.node.parentNode &&
              (action.node.data.identifier === null ||
                action.node.data.identifier === '')) {
                params.id = action.node.parentNode.data.identifier;
            } else {
                params.id = action.node.data.identifier;
            }

        }, me);
        tree.on('beforeitemexpand', function () {
            pan.getEl().mask('Loading...');
        }, me);
        tree.on('load', function (store, model) {
            pan.getEl().unmask();
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
        }, me);

        tree.show();
        tree.getStore().load();
    },

    onSearchClick: function (view, model, n, idx) {
        var me = this;
        var content = me.getSearchContent(),
        propPanel = me.getSearchProperty();
        var node = view.getSelectionModel().getLastSelected();
        try {
            node.on('expand', function () {
                content.getEl().unmask();
            }, me);
            if (node.data.type == "ClassNode" && model.firstChild) {
                propPanel.setSource(model.firstChild.data.record);
            } else {
                propPanel.setSource(node.data.record);
            }
        } catch (e) { }
        node.expand();
    },

    init: function (application) {
        this.control({
            "button[action=search]": {
                click: this.onSearchRdl
            }
        });
    }
});
