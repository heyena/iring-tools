using System;
using System.Windows;
using System.Windows.Controls;
using org.iringtools.modules.templateeditor.editorregion;
using PrismContrib.Base;

namespace org.iringtools.modules.templateeditor.rolespopup
{
    public partial class RolesPopup : ChildWindow, IRolesPopup
    {
        private RolesPopupModel _rolesPopupModel = null;

        public RolesPopup()
        {
            InitializeComponent();
        }        

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            this.Closed(this, new DialogClosedEventArgs(this.DialogResult));
        }

        new public event EventHandler Closed;

        #region IRolesPopup Members

        bool? IRolesPopup.DialogResult
        {
            get
            {
                return this.DialogResult;
            }
        }

        void IRolesPopup.Show(RolesPopupModel model)
        {
            _rolesPopupModel = model;

            radRoleRange.DataContext = _rolesPopupModel;
            roleRange.DataContext = _rolesPopupModel;

            radRoleValue.DataContext = _rolesPopupModel;

            radRoleValueReference.DataContext = _rolesPopupModel;
            roleValueReference.DataContext = _rolesPopupModel;

            radRoleValueLiteral.DataContext = _rolesPopupModel;
            roleValueLiteral.DataContext = _rolesPopupModel;
            roleValueLiteralDatatype.DataContext = _rolesPopupModel;

            this.Show();
        }

        public RolesPopupModel rolesPopupModel
        {
            get
            {
                return this._rolesPopupModel;
            }
        }

        #endregion
    }
}

