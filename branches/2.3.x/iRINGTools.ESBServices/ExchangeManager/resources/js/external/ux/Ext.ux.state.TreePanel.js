// vim: ts=4:sw=4:nu:fdc=4:nospell
/*global Ext */
/**
 * @class Ext.ux.state.TreePanel
 *
 * Ext.tree.TreePanel State Plugin
 * <br><br>
 * Usage:
<pre>
var tree = new Ext.tree.TreePanel({
     root:{
         nodeType:'async'
        ,id:'root'
        ,text:'Root'
    }
    ,loader: {
         url:'get-tree.php'
    }
    <b>,plugins:[new Ext.ux.state.TreePanel()]</b>
});
</pre>
 *
 * @author    Ing. Jozef Sakáloš
 * @copyright (c) 2009, by Ing. Jozef Sakáloš
 * @date      <ul>
 * <li>5. April 2009<li>
 * </ul>
 * @version   1.0
 * @revision  $Id: Ext.ux.state.TreePanel.js 676 2009-04-07 13:03:20Z jozo $
 *
 * @license Ext.ux.state.TreePanel.js is licensed under the terms of
 * the Open Source LGPL 3.0 license.  Commercial use is permitted to the extent
 * that the code/component(s) do NOT become part of another Open Source or Commercially
 * licensed development library or toolkit without explicit permission.
 *
 * <p>License details: <a href="http://www.gnu.org/licenses/lgpl.html"
 * target="_blank">http://www.gnu.org/licenses/lgpl.html</a></p>
 *
 * @forum     64714
 * @demo      http://examples.extjs.eu/?ex=treestate
 * @download  <ul>
 * <li><a href="http://examples.extjs.eu/Ext.ux.state.TreePanel.js.bz2">Ext.ux.state.TreePanel.js.bz2</a></li>
 * <li><a href="http://examples.extjs.eu/Ext.ux.state.TreePanel.js.gz">Ext.ux.state.TreePanel.js.gz</a></li>
 * <li><a href="http://examples.extjs.eu/Ext.ux.state.TreePanel.js.zip">Ext.ux.state.TreePanel.js.zip</a></li>
 * </ul>
 *
 * @donate
 * <form action="https://www.paypal.com/cgi-bin/webscr" method="post" target="_blank">
 * <input type="hidden" name="cmd" value="_s-xclick">
 * <input type="hidden" name="hosted_button_id" value="3430419">
 * <input type="image" src="https://www.paypal.com/en_US/i/btn/x-click-butcc-donate.gif"
 * border="0" name="submit" alt="PayPal - The safer, easier way to pay online.">
 * <img alt="" border="0" src="https://www.paypal.com/en_US/i/scr/pixel.gif" width="1" height="1">
 * </form>
 */
 
Ext.ns('Ext.ux.state');
 
// dummy constructor
Ext.ux.state.TreePanel = function() {};
 
Ext.override(Ext.ux.state.TreePanel, {
    /**
     * Initializes the plugin
     * @param {Ext.tree.TreePanel} tree
     * @private
     */
    init:function(tree) {
        // install event handlers on TreePanel
        tree.on({
            // add path of expanded node to stateHash
             beforeexpandnode:function(n) {
                this.stateHash[n.id] = n.getPath();
            }
 
            // delete path and all subpaths of collapsed node from stateHash
            ,beforecollapsenode:function(n) {
                delete this.stateHash[n.id];
                var cPath = n.getPath();
                for(var p in this.stateHash) {
                    if(this.stateHash.hasOwnProperty(p)) {
                        if(-1 !== this.stateHash[p].indexOf(cPath)) {
                            delete this.stateHash[p];
                        }
                    }
                }
            }
        });
 
        // update state on node expand or collapse
        tree.stateEvents = tree.stateEvents || [];
        tree.stateEvents.push('expandnode', 'collapsenode');
        tree.stateRestored =  false;
 
        // add state related props to the tree
        Ext.apply(tree, {
            // keeps expanded nodes paths keyed by node.ids
             stateHash:{}
 
            // apply state on tree initialization
            ,applyState:function(state) {
                if(state) {
                    Ext.apply(this, state);
 
                    // it is too early to expand paths here
                    // so do it once on root load
                    this.root.on({
                        load:{single:true, scope:this, fn:function() {
                            for(var p in this.stateHash) {
                                if(this.stateHash.hasOwnProperty(p)) {
                                    this.expandPath(this.stateHash[p]);
                                }
                            }
                        }}
                    });
                    tree.stateRestored = true;
                } else {
                    tree.stateRestored = true;
                }
            } // eo function applyState
 
            // returns stateHash for save by state manager
            ,getState:function() {
                return {stateHash:this.stateHash};
            } // eo function getState
        });
    } // eo function init
 
}); // eo override
 
// eof