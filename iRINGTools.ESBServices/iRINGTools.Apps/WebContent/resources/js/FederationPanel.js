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
        addnew: true,
        opentab:true,
        load:true,
        beforeload:true,
        selectionchange:true
    });

    this.tbar = this.buildToolbar();

    this.federationPanel = new Ext.tree.TreePanel({
      region: 'north',
      collapseMode: 'mini',
      height: 300,
      layout: 'fit',
      border: false,
      split: true,
      expandAll:true,
      
      rootVisible: false,
      lines: true,
      //singleExpand: true,
      useArrows: false,
      autoScroll:true,
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
    this.federationPanel.on('load', this.onLoad, this);
    this.federationPanel.on('beforeload', this.onBeforeLoad, this);
    this.federationPanel.on('dblclick', this.onDblClick, this);
    //this.federationPanel.on('expandnode', this.onExpand, this);
    //this.federationPanel.on('expandnode', this.select_node, this);
    this.federationPanel.on('refresh', this.onRefresh, this);
    this.federationPanel.getSelectionModel().on('selectionchange',this.onSelectionChange,this,this);

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
			text:'Open',
			icon:'resources/images/16x16/document-open.png',
			tooltip:'Open',
			disabled: false,
			handler: this.onEdit,
			scope: this
		},{xtype:"tbbutton",
			icon:'resources/images/16x16/document-new.png',
			tooltip:'New',
			text:'New',
			disabled: false,
			handler: this.onAddnew,
			scope: this
		},{xtype:"tbbutton",
			icon:'resources/images/16x16/edit-delete.png',
			tooltip:'Delete',
			text:'Delete',
			disabled: false,
			handler: this.onDelete,
			scope: this
		}]
  },
  
onDelete:function(){
    that = this;
    var node = this.getSelectedNode();
     if(node.hasChildNodes()){
          Ext.MessageBox.show({
                title: '<font color=red></font>',
                msg: 'Please select a child node to delete.',
                buttons: Ext.MessageBox.OK,
                icon: Ext.MessageBox.INFO
        });
    }else if(node==null){
        Ext.Msg.alert('Warning', 'Please select a node.')
    }else{
    Ext.Msg.show({
        msg: 'All the tabs will be closed. Do you want to delete this node?',
        buttons: Ext.Msg.YESNO,
        icon: Ext.Msg.QUESTION,
        fn: function(action){
                if(action=='yes'){                    
                //send ajax request
                Ext.Ajax.request({
                            url: 'deleteNode',
                            method: 'GET',
                            params: {'nodeId' : node.id, 'parentNodeID':node.parentNode.id},
                            success: function(o) {
                                // remove all tabs form tabpanel
                                Ext.getCmp('contentPanel').removeAll(true); // it will be removed in future
                                Ext.getCmp('contentPanel').disable(); // it will be removed in future
                                
                                // remove the node form tree
                                //that.federationPanel.selModel.selNode.parentNode.removeChild(node);
                                //Tree Reload
                                that.federationPanel.root.reload();
                                // fire event so that the Details panel will be changed accordingly
                                that.fireEvent('selectionchange',this)
                                Ext.Msg.alert('Sucess', 'Node has been deleted')
                                },
                            failure: function(f,a){
                                Ext.Msg.alert('Warning', 'Error!!!')
                            }
                        });
                }else if (action == 'no'){
                        Ext.Msg.alert('Info', 'Not now');
                }
        }
    });
    }
},

 getNodeBySelectedTab: function(tab) {
        var tabid = tab.id;
        nodeId = tabid.substr(4,tabid.length)  // tabid is "tab-jf23dfj-sd3fas-df33s-s3df"
        return this.getNodeById(nodeId)        // get the NODE using nodeid
  },

  getNodeById: function(nodeId) {
      if(this.federationPanel.getNodeById(nodeId)){ //if nodeID exists it will find out NODE
          return this.federationPanel.getNodeById(nodeId)
      }else{
          return false;
      }
  },
  getSelectedNode: function() {
  	return this.federationPanel.getSelectionModel().getSelectedNode();
  },
  
  selectNode:function(node){
      this.expandNode(node);
      this.federationPanel.getSelectionModel().select(node);
  },

  expandNode:function(node){
      this.federationPanel.expandPath(node.getPath())
  },

 onSelectionChange:function(sm,node){
     this.onClick(node)
 },
generateForm:function(formType){
    node = this.getSelectedNode();
    if (node != null){
            if( (node.hasChildNodes() && formType =='newForm')|| (!node.hasChildNodes() && formType =='editForm')){
  		 this.openTab(node,formType);
            }else if(!node.hasChildNodes() && formType =='newForm'){
                this.openTab(node.parentNode,formType);
            }else{
                  Ext.MessageBox.show({
                        title: '<font color=red></font>',
                        msg: 'Please select a child node to edit.',
                        buttons: Ext.MessageBox.OK,
                        icon: Ext.MessageBox.INFO
                });
            }
        }else{
          Ext.MessageBox.show({
                title: '<font color=red></font>',
                msg: 'Please select a node.',
                buttons: Ext.MessageBox.OK,
                icon: Ext.MessageBox.INFO
        });
        }
   
},

getAllChildNodes:function(parentNode,skippedIDs){
    var mainArr = new Array()
    var kids = parentNode.childNodes;   // Get the list of children
    var numkids = kids.length; // Figure out how many there are

    //find child
    for(var i = 0, len = numkids; i < len; i++) {
        subArr = new Array()
        subArr[0] =kids[i].attributes.id
        subArr[1] =kids[i].attributes.text
        if((typeof(skippedIDs) != 'undefined') && skippedIDs.indexOf(kids[i].attributes.id)<0){        
        mainArr.push(subArr)
        }else if((typeof(skippedIDs) == 'undefined')){
            mainArr.push(subArr)
        }
    } 
    return Ext.util.JSON.encode(mainArr);
 },

getNodesIDTitleByID:function(){

},

openTab: function(node,formType) {
 
     var obj = node.attributes
        var properties = node.attributes.properties
        var nId = obj['id']

         var list_items = '{'
                     +'xtype : "hidden",'//hidden field
                     +'name:"formType",' // it will contain 'editForm/newForm'
                     +'value:"'+formType+'"' //value of the field
                     +'},';
        if(formType=='newForm'){
                list_items = list_items+'{'
                     +'xtype:"hidden",'//hidden field
                     +'name:"parentNodeID",' //it will contain "ID Generators||Namespaces||Repositories
                     +'value:"'+obj['id']+'"' //value of the field
                     +'}';
         }
         if(formType=='editForm'){
                list_items = list_items+'{'
                     +'xtype:"hidden",'//hidden field
                     +'name:"nodeID",' //name of the field sent to the server
                     +'value:"'+obj['id']+'"' //value of the field
                     +'},'
                     +'{'
                     +'xtype:"hidden",'//hidden field
                     +'name:"parentNodeID",' //it will contain "ID Generators||Namespaces||Repositories
                     +'value:"'+node.parentNode.id+'"' //value of the field
                     +'}';
        }
        
	/* 01. Start The New/Edit Form Component Items*/          

        /*
         * Generate the fields items dynamically
         */

        for ( var i = 0; i < properties.length; i++) {

            var fname=properties[i].name
            var value=''
            var xtype=''
            switch(fname){
                case "Description":
                    xtype= 'xtype : "textarea", width : 230'
                    break;
                 case "URI":
                     xtype= 'xtype : "textfield", width : 230'
                    break;
                 case "Read Only" :
                 case "Writable":
                     xtype= 'xtype : "combo", width : 230, triggerAction: "all", editable : false, mode: "local", store: ["true","false"],  displayField:"'+properties[i].value+'", width: 120'
                     break;
                 case 'Repository Type':
                     xtype= 'xtype : "combo",width : 230, triggerAction: "all", editable : false, mode: "local", store: ["RDS/WIP", "Camelot", "Part8"],  displayField:"'+properties[i].value+'", width: 120'
                 break;
                 case 'ID Generator':
                     // get all the IDGenerators
                     var allIDGenerators = this.getAllChildNodes(this.federationPanel.getRootNode().findChild('id','idGenerator'))
                     xtype= 'xtype : "combo",hiddenName:"ID Generator", width : 230, triggerAction: "all", editable : false, mode: "local", store: '+allIDGenerators+',  displayField:"'+properties[i].value+'", width: 120'
                 break;
                 case 'Namespace List':
                     var imgPath='./resources/js/external/ux/images/'
                     var selNameSpaceIDsArr = new Array()
                     var nodeIDTitleArr = new Array()
                     if(properties[i].value !=null && properties[i].value!=''){
                         var selNameSpaces=properties[i].value.items;
                         selNameSpaceIDsArr= (selNameSpaces.toString()).split(',');

                        // nodeIDTitleArr = this.getNodesIDTitleByID(selNameSpaceIDsArr)                        
                         
                         for(var j=0;j < selNameSpaceIDsArr.length; j++ ){
                            
                            subArr = new Array()
                            subArr[0] =selNameSpaceIDsArr[j]                // contains nameSpaceID
                            subArr[1] =this.getNodeById(subArr[0]).text     // contains nameSpaceTitle
                            
                             if(subArr[0]!='' && subArr[1]!=undefined){
                                  nodeIDTitleArr.push(subArr)
                             }
                         }    
                     }
                     
                     // get all the namespances
                     var filteredNameSpaces = this.getAllChildNodes(this.federationPanel.getRootNode().findChild('id','namespace'),selNameSpaceIDsArr)

                     xtype='xtype : "itemselector", fieldLabel: "Namespace List",name: "itemselector",'
                         +'imagePath: "'+imgPath+'", '
                         +'multiselects: [{ width: 200,height: 150,displayField: "text", valueField: "value",'
                         +'store:'+filteredNameSpaces+'},{ width: 200,height: 150, store: '+Ext.util.JSON.encode(nodeIDTitleArr)+'}]';

                 break;
                 default:
                    xtype= 'xtype : "textfield", width : 230'
            }

             if(formType=='editForm'){
                 value = ', value:"'+ properties[i].value+'"';
            	 label = node.parentNode.text + ' : ' + obj['text'];
             }else{
            	 label = obj['text']+':(New)';
             }
             list_items = list_items+',{'+xtype+', fieldLabel:"' + properties[i].name
             + '",name:"'
             + properties[i].name
             + '",allowBlank:false, blankText:"This Field is required !"'
             +value+'}'

       }
        list_tems = eval('[' + list_items + ']')
        //label = node.parentNode.text + ' : ' + obj['text']+'('+formType+')'
        this.fireEvent('opentab', this, node, label, list_tems)
 },


  onClick: function(node) {      

        var gridSource = new Array();
        if(node!=undefined && node!=null && !node.hasChildNodes()){
            // get all the attributes of node
            var properties = node.attributes.properties;
            for ( var i = 0; i < properties.length; i++) {
                    gridSource[properties[i].name] = properties[i].value;
            }
        }
        // populate the property grid with gridSource
        this.propertyPanel.setSource(gridSource);
    
        this.fireEvent('click', this, node);
  },

  onDblClick: function(node) {
      this.generateForm('editForm')
  },

  onExpand: function(node) {
		//Ext.state.Manager.set('federation-state', node.getPath());
		//this.fireEvent('refresh', this, this.getSelectedNode());
	},

  onRefresh: function (node) {
      Ext.state.Manager.clear('federation-state');
      this.federationPanel.root.reload();
  },

  onEdit: function (btn, ev) {
      this.generateForm('editForm')
  },

  onAddnew: function (btn, ev) {
     this.generateForm('newForm')
        
  },
  
  onLoad: function () {
      this.federationPanel.getRootNode().expand(true);
      this.federationPanel.body.unmask();
  },
  
  onBeforeLoad: function () {
      this.federationPanel.body.mask('Loading...', 'x-mask-loading')
  }

});