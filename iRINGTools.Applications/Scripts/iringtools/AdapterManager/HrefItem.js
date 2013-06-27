Ext.namespace('Ext.ux');
Ext.ux.HrefItem = function (config) {
  Ext.ux.HrefItem.superclass.constructor.call(this, config);
}
Ext.extend(Ext.ux.HrefItem, Ext.menu.Item, {
  setHref: function (href, target) {
    this.href = (href == undefined) ? "#" : href;
    this.hrefTarget = (target == undefined) ? "_blank" : target;
  }
});
Ext.reg('hrefitem', Ext.ux.HrefItem);