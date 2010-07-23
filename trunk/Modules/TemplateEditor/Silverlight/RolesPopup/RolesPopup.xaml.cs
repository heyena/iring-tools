using System;
using System.Windows;
using System.Windows.Controls;
using org.iringtools.modules.templateeditor.editorregion;
using PrismContrib.Base;
using System.Collections.Generic;
using System.ComponentModel;

namespace org.iringtools.modules.templateeditor.rolespopup
{
    public partial class RolesPopup : ChildWindow, IRolesPopup
    {
        private RolesPopupModel _rolesPopupModel = null;

        public RolesPopup()
        {
            InitializeComponent();
            radRoleRange.Checked += new RoutedEventHandler(radioButtonCheckedHandler);
            radRoleValue.Checked += new RoutedEventHandler(radioButtonCheckedHandler);
            radRoleValueLiteral.Checked += new RoutedEventHandler(radioButtonCheckedHandler);
            radRoleValueReference.Checked += new RoutedEventHandler(radioButtonCheckedHandler);
        }

        void radioButtonCheckedHandler(object sender, RoutedEventArgs e)
        {
            if (radRoleRange.IsChecked == true)
            {
                roleRange.IsEnabled = true;
                radRoleValueLiteral.IsEnabled = false;
                radRoleValueReference.IsEnabled = false;
                roleValueLiteral.IsEnabled = false;
                roleValueLiteralDatatype.IsEnabled = false;
                roleValueReference.IsEnabled = false;
            }
            else if (radRoleValue.IsChecked == true)
            {
                roleRange.IsEnabled = false;
                radRoleValueLiteral.IsEnabled = true;
                radRoleValueReference.IsEnabled = true;
                roleValueLiteral.IsEnabled = true;
                roleValueLiteralDatatype.IsEnabled = radRoleValueLiteral.IsChecked == true;
                roleValueReference.IsEnabled = true;
            }            
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            FinalizeModel();
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

        private void FinalizeModel()
        {
            if (radRoleRange.IsChecked == true)
            {
                _rolesPopupModel.SelectedRoleValueLiteral = String.Empty;
                _rolesPopupModel.SelectedRoleValueLiteralDatatype = new KeyValuePair<string, string>();
                _rolesPopupModel.SelectedRoleValueReference = String.Empty;
            }
            else if (radRoleValue.IsChecked == true)
            {
                if (radRoleValueReference.IsChecked == true)
                {
                    _rolesPopupModel.SelectedRoleValueLiteral = String.Empty;
                    _rolesPopupModel.SelectedRoleValueLiteralDatatype = new KeyValuePair<string, string>();
                    _rolesPopupModel.SelectedRoleRange = new KeyValuePair<string, string>();
                }
                else if (radRoleValueLiteral.IsChecked == true)
                {
                    _rolesPopupModel.SelectedRoleValueReference = String.Empty;
                    _rolesPopupModel.SelectedRoleRange = new KeyValuePair<string, string>();
                }                
            }
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

