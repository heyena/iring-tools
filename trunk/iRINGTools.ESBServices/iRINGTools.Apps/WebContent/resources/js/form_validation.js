/*
 * @File Name : form_validation.js
 * @Path : resources/js
 * @Using Lib : Ext JS Library 3.2.1(lib/ext-3.2.1)
 *
 * This file intended to implement custom validation on form
 * It contains different kinds of rules those can be apply on form fields
 */

var nameErrLength = 'Minimum 4 character is required !'
var nameErrUnique = 'Name already in use !';

Ext.apply(Ext.form.VTypes,{
    //uniquenameMask: /[A-Za-z0-9 ]/i, //restrict user input and allow these only
    uniquename:function(val){
         if(val.length <4 ){
            Ext.apply(Ext.form.VTypes,{
                uniquenameText:nameErrLength
            });
            return false;
        }else{
          Ext.Ajax.request({
                url: 'save-form.php',
                method: 'POST',
                params: 'name=' + val,
                success: function(o) {
                    if (o.responseText == 0) {
                        resetNameValidator(false);
                        Ext.apply(Ext.form.VTypes, {
                            uniquenameText: nameErrUnique
                        });
                        return false;
                    } else {
                        return true;
                        resetNameValidator(true);
                    }
                }
            });
            return true;
        }
    }
})

function resetNameValidator(is_error) {
    Ext.apply(Ext.form.VTypes, {
        uniquename : function(val) {
            if (val.length < 4) {
                Ext.apply(Ext.form.VTypes, {
                    uniquenameText: nameErrLength
                });
                return false;
            } else {
                Ext.Ajax.request({
                    url: 'validate-form',
                    method: 'POST',
                    params: 'name=' + val,
                    success: function(o) {
                        if (o.responseText == 0) {
                            resetNameValidator(false);
                        } else {
                            resetNameValidator(true);
                        }
                    }
                });
                return is_error;
            }
        }
    });
}
