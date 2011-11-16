using System;
using org.iringtools.modules.templateeditor.editorregion;
using PrismContrib.Base;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace org.iringtools.modules.templateeditor.rolespopup
{
    public interface IRolesPopup
    {
        bool? DialogResult { get; }
        void Show(RolesPopupModel model);
        event EventHandler Closed;
        RolesPopupModel rolesPopupModel { get; }
    }    
}
