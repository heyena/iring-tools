// <reference path="../ext-3.2.1/adapter/ext/ext-base.js" />
/// <reference path="../ext-3.2.1/ext-all.js" />
/// <reference path="../ext-3.2.1/ux/ux-all.js" />

// Application instance for showing user-feedback messages.
var App = new Ext.App({});

Ext.onReady(function () {
	Ext.QuickTips.init();
	Ext.Ajax.timeout = 120000; //increase request time

	Ext.get('about-link').on('click', function () {
		var win = new Ext.Window({
			title: 'About Adapter Manager',
			bodyStyle: 'background-color:white;padding:5px',
			width: 700,
			height: 500,
			closable: true,
			resizable: false,
			autoScroll: true,
			buttons: [{
				text: 'Close',
				handler: function () {
					Ext.getBody().unmask();
					win.close();
				}
			}],
			autoLoad: 'about.html',
			listeners: {
				close: {
					fn: function () {
						Ext.getBody().unmask();
					}
				}
			}
		});

		Ext.getBody().mask();
		win.show();
	});

	/*
	* var actionPanel = new AdapterManager.ActionPanel({ id: 'action-panel',
	* region: 'west', width: 200,
	* 
	* collapseMode: 'mini', collapsible: true, collapsed: false });
	*/

	var searchPanel = new AdapterManager.SearchPanel({
		id: 'search-panel',
		title: 'Reference Data Search',
		collapsedTitle: 'Reference Data Search',
		region: 'south',
		height: 300,
		//collapseMode: 'mini',
		collapsible: true,
		collapsed: false,
		searchUrl: 'refdata/getnode',
		limit: 100
	});

	var contentPanel = new Ext.TabPanel({
		id: 'content-panel',
		region: 'center',
		collapsible: false,
		closable: true,
		enableTabScroll: true,
		border: true,
		split: true
	});

	var centrePanel = new Ext.Panel({
		id: 'centre-panel',
		region: 'center',
		layout: 'border',
		collapsible: false,
		closable: true,
		enableTabScroll: true,
		border: true,
		split: true,
		items: [searchPanel, contentPanel]
	});

	var directoryPanel = new AdapterManager.DirectoryPanel({
		id: 'nav-panel',
		title: 'Directory',
		collapsedTitle: 't',
		region: 'west',
		width: 260,
		collapsible: true,
		collapsed: false,
		navigationUrl: 'directory/getnode'
	});


	directoryPanel.on('newscope', function (npanel, node) {
		var newTab = new AdapterManager.ScopePanel({
			id: 'tab-' + node.id,
			record: node.attributes.record,
			url: 'directory/scope'
		});

		newTab.on('save', function (panel) {
			win.close();
			directoryPanel.onReload(node);
			if (node.expanded == false)
				node.expand();
		}, this);

		newTab.on('Cancel', function (panel) {
			win.close();
		}, this);

		var win = new Ext.Window({
			closable: true,
			modal: false,
			layout: 'fit',
			title: 'Add New Scope',
			iconCls: 'tabsScope',
			height: 180,
			width: 430,
			plain: true,
			items: newTab
		});

		win.show();
	}, this);


	directoryPanel.on('editscope', function (npanel, node) {
		var newTab = new AdapterManager.ScopePanel({
			id: 'tab-' + node.id,
			record: node.attributes.record,
			url: 'directory/scope'
		});

		var parentNode = node.parentNode;

		newTab.on('save', function (panel) {
			win.close();
			directoryPanel.onReload(node);
			if (parentNode.expanded == false)
				parentNode.expand();
		}, this);

		newTab.on('Cancel', function (panel) {
			win.close();
		}, this);

		var win = new Ext.Window({
			closable: true,
			modal: false,
			layout: 'fit',
			title: 'Edit Scope \"' + node.text + '\"',
			iconCls: 'tabsScope',
			height: 180,
			width: 430,
			plain: true,
			items: newTab
		});

		win.show();

	}, this);


	directoryPanel.on('deletescope', function (npanel, node) {

		Ext.Ajax.request({
			url: 'directory/deletescope',
			method: 'POST',
			params: {
				'nodeid': node.attributes.id
			},
			success: function (o) {
				directoryPanel.onReload(node);
			},
			failure: function (f, a) {
				//Ext.Msg.alert('Warning', 'Error!!!');
				var message = 'Error deleting scope!';
				showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
			}
		});

	}, this);

	directoryPanel.on('editgraphname', function (npanel, node) {
		contentPanel.removeAll(true);
	}, this);

	directoryPanel.on('configure', function (npanel, node) {
		var dataLayerValue = node.attributes.record.DataLayer;
		var application = node.text;
		var scope = node.parentNode.text;

		if (dataLayerValue == 'ExcelLibrary') {
			var newConfig = new AdapterManager.ExcelLibraryPanel({
				id: 'tab-c.' + scope + '.' + application,
				title: 'Configure - ' + scope + '.' + application,
				scope: scope,
				application: application,
				url: 'excel/configure',
				closable: true
			});

			contentPanel.add(newConfig);
			contentPanel.activate(newConfig);
		}
		else if (dataLayerValue == 'NHibernateLibrary') {
			var nhConfigId = scope + '.' + application + '.-nh-config';
			var nhConfigWizard = contentPanel.getItem(nhConfigId);

			if (nhConfigWizard) {
				nhConfigWizard.show();
			}
			else {
				nhConfigWizard = new AdapterManager.NHibernateConfigWizard({
					scope: scope,
					app: application
				});
				contentPanel.add(nhConfigWizard);
				contentPanel.activate(nhConfigWizard);
			}
		}
	}, this);

	directoryPanel.on('newapplication', function (npanel, node) {

		var newTab = new AdapterManager.ApplicationPanel({
			id: 'tab-' + node.id,
			scope: node.attributes.record,
			record: null,
			url: 'directory/application'
		});

		newTab.on('save', function (panel) {
			win.close();
			node.attributes.
			directoryPanel.onReload(node);
			if (node.expanded == false)
				node.expand();
		}, this);

		newTab.on('Cancel', function (panel) {
			win.close();
		}, this);

		var win = new Ext.Window({
			closable: true,
			modal: false,
			layout: 'fit',
			title: 'Add New Application',
			iconCls: 'tabsApplication',
			height: 200,
			width: 430,
			plain: true,
			items: newTab
		});

		win.show();

	}, this);

	directoryPanel.on('editapplication', function (npanel, node) {
		if (node == undefined || node == null)
			return;

		var newTab = new AdapterManager.ApplicationPanel({
			id: 'tab-' + node.id,
			scope: node.parentNode.attributes.record,
			record: node.attributes.record,
			url: 'directory/application'
		});

		var parentNode = node.parentNode.parentNode;

		newTab.on('save', function (panel) {
			win.close();
			var dataLayerValue = node.attributes.record.DataLayer;
			var application = node.text;
			var scope = node.parentNode.text;

			if (dataLayerValue == 'ExcelLibrary') {
				var configTab = contentPanel.items.map[scope + '.' + application + '.-nh-config'];						
			}
			else {
				var configTab = contentPanel.items.map['tab-c.' + scope + '.' + application];
			}

			if (configTab)
				configTab.destroy();

			directoryPanel.onReload(node);
			if (parentNode.expanded == false)
				parentNode.expand();
		}, this);

		newTab.on('Cancel', function (panel) {
			win.close();
		}, this);

		var win = new Ext.Window({
			closable: true,
			modal: false,
			layout: 'fit',
			title: 'Edit Application \"' + node.text + '\"',
			iconCls: 'tabsApplication',
			height: 200,
			width: 430,
			plain: true,
			items: newTab
		});

		win.show();

	}, this);

	directoryPanel.on('deleteapplication', function (npanel, node) {
		var parentNode = node.parentNode.parentNode;

		Ext.Ajax.request({
			url: 'directory/deleteapplication',
			method: 'POST',
			params: {
				'nodeid': node.attributes.id
			},
			success: function (o) {
				directoryPanel.onReload(node);
				if (parentNode.expanded == false)
					parentNode.expand();
			},
			failure: function (f, a) {
				//Ext.Msg.alert('Warning', 'Error!!!');
				var message = 'Error deleting application!';
				showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
			}
		});
	}, this);

	directoryPanel.on('opengraphmap', function (npanel, node) {

		var scope = node.parentNode.parentNode.parentNode;
		var application = node.parentNode.parentNode;

		var newTab = new AdapterManager.MappingPanel({
			title: 'GraphMap - ' + scope.text + "." + application.text + '.' + node.text,
			id: 'GraphMap - ' + scope.text + "-" + application.text + '.' + node.text,
			scope: scope.attributes.record,
			record: node.attributes.record,
			application: application.attributes.record,
			navigationUrl: 'mapping/getnode',
			searchPanel: searchPanel,
			directoryPanel: directoryPanel
		});

		contentPanel.add(newTab);
		contentPanel.activate(newTab);

	}, this);


	// Load Stores
	// searchPanel.load();

	// Finally, build the main layout once all the pieces are ready. This is also
	// a good
	// example of putting together a full-screen BorderLayout within a Viewport.
	var viewPort = new Ext.Viewport({
		layout: 'border',
		title: 'Scope Editor',
		border: false,
		items: [{
			xtype: 'box',
			region: 'north',
			applyTo: 'header',
			border: false,
			height: 55
		}, directoryPanel, centrePanel],
		listeners: {
			render: function () {
				// After the component has been rendered, disable the default browser
				// context menu
				Ext.getBody().on("contextmenu", Ext.emptyFn, null, {
					preventDefault: true
				});
			}
		},
		renderTo: Ext.getBody()
	});

});


/*

Ext.override(Ext.layout.BorderLayout, {
	southTitleAdded: false,
	// private
	onLayout: function (ct, target) {

		var collapsed;
		if (!this.rendered) {

			target.position();
			target.addClass('x-border-layout-ct');
			var items = ct.items.items;
			collapsed = [];
			for (var i = 0, len = items.length; i < len; i++) {
				var c = items[i];
				var pos = c.region;
				if (c.collapsed) {
					collapsed.push(c);
				}
				c.collapsed = false;
				if (!c.rendered) {
					c.cls = c.cls ? c.cls + ' x-border-panel' : 'x-border-panel';
					c.render(target, i);
				}
				this[pos] = pos != 'center' && c.split ?
                        new Ext.layout.BorderLayout.SplitRegion(this, c.initialConfig, pos) :
                        new Ext.layout.BorderLayout.Region(this, c.initialConfig, pos);
				this[pos].render(target, c);
			}
			this.rendered = true;
		}

		var size = target.getViewSize();
		if (size.width < 20 || size.height < 20) { 
			if (collapsed) {
				this.restoreCollapsed = collapsed;
			}
			return;
		} else if (this.restoreCollapsed) {
			collapsed = this.restoreCollapsed;
			delete this.restoreCollapsed;
		}

		var w = size.width, h = size.height;
		var centerW = w, centerH = h, centerY = 0, centerX = 0;

		var n = this.north, s = this.south, west = this.west, e = this.east, c = this.center;
	

		if (n && n.isVisible()) {
			var b = n.getSize();
			var m = n.getMargins();
			b.width = w - (m.left + m.right);
			b.x = m.left;
			b.y = m.top;
			centerY = b.height + b.y + m.bottom;
			centerH -= centerY;
			n.applyLayout(b);
		}
		if (s && s.isVisible()) {
			var b = s.getSize();
			var m = s.getMargins();
			b.width = w - (m.left + m.right);
			b.x = m.left;
			var totalHeight = (b.height + m.top + m.bottom);
			b.y = h - totalHeight + m.top;
			centerH -= totalHeight;
			s.applyLayout(b);
			
			if (typeof s.collapsedEl != 'undefined' && s.collapsedTitle && this.southTitleAdded == false) {
				this.southTitleAdded = true;
				var cDiv = s.collapsedEl;
				var tpl = new Ext.Template('<div style="float: left;">{txt}</div>');

				var insertedHtml = tpl.insertFirst(cDiv, { txt: s.collapsedTitle });
				if (s.collapsedTitleStyle) {
					insertedHtml.applyStyles(s.collapsedTitleStyle);
				}

				if (s.collapsedTitleCls) {
					Ext.get(insertedHtml).addClass(s.collapsedTitleCls);
				}

			}
		}
		if (west && west.isVisible()) {
			var b = west.getSize();
			var m = west.getMargins();
			b.height = centerH - (m.top + m.bottom);
			b.x = m.left;
			b.y = centerY + m.top;
			var totalWidth = (b.width + m.left + m.right);
			centerX += totalWidth;
			centerW -= totalWidth;
			west.applyLayout(b);
		}
		if (e && e.isVisible()) {
			var b = e.getSize();
			var m = e.getMargins();
			b.height = centerH - (m.top + m.bottom);
			var totalWidth = (b.width + m.left + m.right);
			b.x = w - totalWidth + m.left;
			b.y = centerY + m.top;
			centerW -= totalWidth;
			e.applyLayout(b);
		}

		var m = c.getMargins();
		var centerBox = {
			x: centerX + m.left,
			y: centerY + m.top,
			width: centerW - (m.left + m.right),
			height: centerH - (m.top + m.bottom)
		};
		c.applyLayout(centerBox);

		if (collapsed) {
			for (var i = 0, len = collapsed.length; i < len; i++) {
				collapsed[i].collapse(false);
			}
		}

		if (Ext.isIE && Ext.isStrict) { // workaround IE strict repainting issue
			target.repaint();
		}
	}
}); 

*/