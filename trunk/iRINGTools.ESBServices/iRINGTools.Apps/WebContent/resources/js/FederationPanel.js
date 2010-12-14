Ext.ns('FederationManager');
/**
* @class FederationManager.FederationPanel
* @extends Panel
* @author by Ritu Garg
*/
FederationManager.FederationPanel = Ext.extend(Ext.Panel, {
  title: 'Federation',
  layout: 'border',
  url: null,

  federationPanel: null,
  propertyPanel: null,

  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

    this.addEvents({
    	click: true,
    	refresh: true,
        edit: true,
        addnew: true
    });

    this.tbar = this.buildToolbar();

    this.federationPanel = new Ext.tree.TreePanel({
      region: 'north',
      collapseMode: 'mini',
      height: 300,
      layout: 'fit',
      border: false,

      rootVisible: false,
      lines: true,
      //singleExpand: true,
      useArrows: true,

      loader: new Ext.tree.TreeLoader({
        dataUrl: this.url
      }),

      root: {
        nodeType: 'async',
        text: 'Federation',
        expanded: true,
        draggable: false,
        icon: 'resources/images/16x16/internet-web-browser.png'
      }

    });

    this.propertyPanel = new Ext.grid.PropertyGrid({
            id:'property-panel',
            title: 'Details',
            region:'center',
            layout: 'fit',
            autoScroll:true,
            margin:'10 0 0 0',
            bodyStyle: 'padding-bottom:15px;background:#eee;',
            source:{},
            listeners: {
                    // to disable editable option of the property grid
                    beforeedit : function(e) {
                            e.cancel=true;
                    }
            }
    });

    this.items = [
      this.federationPanel,
      this.propertyPanel
    ];

    this.federationPanel.on('click', this.onClick, this);
    this.federationPanel.on('dblclick', this.onDblClick, this);
    this.federationPanel.on('expandnode', this.onExpand, this);

    var state = Ext.state.Manager.get("federation-state");

    if (state) {
    	if (this.federationPanel.expandPath(state) == false) {
    		Ext.state.Manager.clear("federation-state");
    		this.federationPanel.root.reload();
  	  }
    }

    // super
    FederationManager.FederationPanel.superclass.initComponent.call(this);
  },

  buildToolbar: function () {
    return [{
			xtype:"tbbutton",
			icon:'resources/images/16x16/view-refresh.png',
			tooltip:'Refresh',
			disabled: false,
			handler: this.onRefresh,
			scope: this
		},{
			xtype:"tbbutton",
			text:'Edit',
			icon:'resources/images/16x16/document-open.png',
			tooltip:'Edit',
			disabled: false,
			handler: this.onEdit,
			scope: this
		},{xtype:"tbbutton",
			icon:'resources/images/16x16/document-new.png',
			tooltip:'Add New',
			text:'Add New',
			disabled: false,
			handler: this.onAddnew,
			scope: this
		}]
  },

  getSelectedNode: function() {
  	return this.federationPanel.getSelectionModel().getSelectedNode();
  },

  openTab: function(node) {
    if (node != null) {
	/* 01. Start The Edit Form Component */

        // 01. Edit Form

        var obj = node.attributes
        var properties = node.attributes.items
        var nId = obj['id']

        if ('children' in obj != true) { // restrict to generate form those have children

                var list_items = '{'
                     +'xtype:"hidden",'//<--hidden field
                     +'name:"nodeID",' //name of the field sent to the server
                     +'value:"'+obj['id']+'"' //value of the field
                     +'},'
                     +'{'
                     +'xtype:"hidden",'//<--hidden field
                     +'name:"parentNodeID",' //parent node id
                     +'value:"'+node.parentNode.id+'"' //value of the field
                     +'}';

                /*
                 * Generate the fields items dynamically
                 */
                for ( var i = 0; i < properties.length; i++) {

                    var fname=properties[i].name
                    var xtype=''
                    var vtype=''
                    switch(fname){
                        case "Description":
                            xtype= 'xtype : "textarea"'
                            break;
                         case "URI":
                             xtype= 'xtype : "textfield"'
                            vtype= 'vtype : "url",'  // comma is required
                            break;
                         case "Read Only" :
                         case "Writable":
                             xtype= 'xtype : "combo",triggerAction: "all", mode: "local", store: ["true","false"],  displayField:"'+properties[i].value+'", width: 120'

                             break;
                         default:
                            xtype= 'xtype : "textfield"'
                            vtype= 'vtype : "uniquename",' // custom validation
                    }

                     list_items = list_items+',{'+xtype+',' +vtype + 'fieldLabel:"' + properties[i].name
                     + '",name:"'
                     + properties[i].name
                     + '",allowBlank:false, blankText:"This Field is required !", value:"'
                     +properties[i].value+'"}'

               }

                list_tems = eval('[' + list_items + ']')

                label = node.parentNode.text + ' : ' + obj['text']

                this.fireEvent('edit', this, node, label, list_tems)
			
  	}
      }
    },


  onClick: function(node) {
      // get all the attributes of node
        var properties = node.attributes.items;

        var gridSource = new Array();

        for ( var i = 0; i < properties.length; i++) {
                gridSource[properties[i].name] = properties[i].value;
        }

        // populate the property grid with gridSource
        this.propertyPanel.setSource(gridSource);
        this.fireEvent('click', this, node);
  },

  onDblClick: function(node) {
  	this.openTab(node);
  },

  onExpand: function(node) {
		//Ext.state.Manager.set('federation-state', node.getPath());
		//this.fireEvent('refresh', this, this.getSelectedNode());
	},

  onRefresh: function (btn, ev) {
  	Ext.state.Manager.clear('federation-state');
		this.federationPanel.root.reload();
  },

  onEdit: function (btn, ev) {
  	this.openTab(this.getSelectedNode());
  },

  onAddnew: function (btn, ev) {
  	node = this.getSelectedNode();
  	if (node != null)
  		this.fireEvent('addnew', this, node);
  }

});