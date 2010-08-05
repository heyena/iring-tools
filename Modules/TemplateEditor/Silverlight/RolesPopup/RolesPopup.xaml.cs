using System;
using System.Windows;
using System.Windows.Controls;
using org.iringtools.modules.templateeditor.editorregion;
using PrismContrib.Base;
using System.Collections.Generic;
using System.ComponentModel;
using org.iringtools.modulelibrary.events;
using System.Linq;

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

            roleRestrictionType.SelectionChanged += new SelectionChangedEventHandler(roleRestrictionType_SelectionChanged);

            roleRange.SelectionChanged += new SelectionChangedEventHandler(rangeSelectionChanged);

            lstRestrictions.SelectionChanged += new SelectionChangedEventHandler(restrictionsSelectionChanged);

            OKButton.Click += (object sender, RoutedEventArgs e) =>
            {
                ButtonClickHandler(new ButtonEventArgs(this, OKButton));
            };

            CancelButton.Click += (object sender, RoutedEventArgs e) =>
            {
                ButtonClickHandler(new ButtonEventArgs(this, CancelButton));
            };

            addRestriction.Click += (object sender, RoutedEventArgs e) =>
            {
                ButtonClickHandler(new ButtonEventArgs(this, addRestriction));
            };

            removeRestriction.Click += (object sender, RoutedEventArgs e) =>
            {
                ButtonClickHandler(new ButtonEventArgs(this, removeRestriction));
            };
        }

        void ButtonClickHandler(ButtonEventArgs e)
        {
            if (e.Name.ToString() == "OKButton")
            {
                FinalizeModel();
                this.DialogResult = true;
            }
            else if (e.Name.ToString() == "CancelButton")
            {
                this.DialogResult = false;
            }
            else if (e.Name.ToString() == "addRestriction")
            {
                string type = ((KeyValuePair<string, string>)roleRestrictionType.SelectedItem).Value;
                string value = roleRestrictionValue.Text;
                _rolesPopupModel.AddRestriction(type, value);
            }
            else if (e.Name.ToString() == "removeRestriction")
            {
                _rolesPopupModel.RemoveRestriction();
            }
        }


        void roleRestrictionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (roleRestrictionType.SelectedItem != null)
            {
                _rolesPopupModel.SelectedRoleRestrictionType = (KeyValuePair<string, string>)roleRestrictionType.SelectedItem;
            }
        }

        public void restrictionsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (lstRestrictions.SelectedItem != null)
                {
                    _rolesPopupModel.SelectedRoleRestriction = ((KeyValuePair<string, object>)lstRestrictions.SelectedItem).Value;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
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

        public void rangeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (roleRange.SelectedItem != null)
                {
                    KeyValuePair<string, string> range = (KeyValuePair<string, string>)roleRange.SelectedItem;

                    if (range.Value != null && range.Value.Equals("<Use Selected Item>"))
                    {
                        KeyValuePair<string, string> cmbItem = new KeyValuePair<string, string>(_rolesPopupModel.ModelSelectedIMLabel, _rolesPopupModel.ModelSelectedIMURI);

                        //GvR need to fix this issue of add already existing item
                        var items = from query in _rolesPopupModel.Ranges 
                                    where query.Key == cmbItem.Key
                                    select query;

                        if (items.Count() == 0)
                        {
                            _rolesPopupModel.Ranges.Add(cmbItem);
                            roleRange.SelectedItem = cmbItem;
                        }
                        else
                        {
                            roleRange.SelectedItem = items.FirstOrDefault();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
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
                _rolesPopupModel.RoleValueLiteral = String.Empty;
                _rolesPopupModel.RoleValueLiteralDatatype = new KeyValuePair<string, string>();
                _rolesPopupModel.RoleValueReference = String.Empty;
            }
            else if (radRoleValue.IsChecked == true)
            {
                if (radRoleValueReference.IsChecked == true)
                {
                    _rolesPopupModel.RoleValueLiteral = String.Empty;
                    _rolesPopupModel.RoleValueLiteralDatatype = new KeyValuePair<string, string>();
                    _rolesPopupModel.RoleRange = new KeyValuePair<string, string>();
                }
                else if (radRoleValueLiteral.IsChecked == true)
                {
                    _rolesPopupModel.RoleValueReference = String.Empty;
                    _rolesPopupModel.RoleRange = new KeyValuePair<string, string>();
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

            lstRestrictions.DataContext = _rolesPopupModel;
            roleRestrictionType.DataContext = _rolesPopupModel;
            roleRestrictionValue.DataContext = _rolesPopupModel;
            addRestriction.DataContext = _rolesPopupModel;
            removeRestriction.DataContext = _rolesPopupModel;

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

