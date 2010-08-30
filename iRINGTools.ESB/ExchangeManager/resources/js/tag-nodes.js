Ext.onReady(function(){
  var tagtree = new Ext.tree.TreePanel({
    renderTo:'tag-nodes-div',
    baseCls : 'x-plain',
    bodyBorder:false,
    border:true,
    footer : true,
    //autoHeight:true,
    height: 369,
    useArrows:true, // true for vista like
    autoScroll:true,
    animate:true,
    // lines :true,
    //enableDD:true,
    containerScroll: true,
    rootVisible: false,
    frame: true,
    ctCls: 'x-box-layout-ct',
    //requestMethod:'GET', default is post
    root:
    {
      nodeType: 'async',
      iconCls: 'my-icon',
      text: 'Directory'
    },
    dataUrl: 'check-tag-nodes.json',
    buttons: [{
      text: 'Transfer Data'
    }]
  });
});