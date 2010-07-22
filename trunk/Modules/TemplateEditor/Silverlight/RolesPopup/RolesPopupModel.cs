using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace org.iringtools.modules.templateeditor.rolespopup
{
    public class RolesPopupModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal ObservableCollection<KeyValuePair<string, string>> _ranges;
        internal ObservableCollection<KeyValuePair<string, string>> _literalDatatypes;

        private KeyValuePair<string, string> _SelectedRoleRange;
        private string _SelectedRoleValueReference;
        private string _SelectedRoleValueLiteral;
        private KeyValuePair<string, string> _SelectedRoleValueLiteralDatatype;
        private bool _HasRange;
        private bool _HasValue;
        private bool _ValueHasReference;
        private bool _ValueHasLiteral;
        private bool _IsBaseTemplate;
        private bool _IsSpecializedTemplate;

        public void RaisePropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
            }
        }
        
        public KeyValuePair<string, string> SelectedRoleRange
        {
            get
            {
                return _SelectedRoleRange;

            }
            set
            {
                _SelectedRoleRange = value;
            }
        }

        public string SelectedRoleValueReference
        {
            get
            {
                return _SelectedRoleValueReference;
            }
            set
            {
                _SelectedRoleValueReference = value;
                RaisePropertyChanged(this, "SelectedRoleValueReference");
            }
        }

        public string SelectedRoleValueLiteral
        {
            get
            {
                return _SelectedRoleValueLiteral;
            }
            set
            {
                _SelectedRoleValueLiteral = value;
                RaisePropertyChanged(this, "SelectedRoleValueLiteral");
            }
        }

        public KeyValuePair<string, string> SelectedRoleValueLiteralDatatype
        {
            get
            {
                return _SelectedRoleValueLiteralDatatype;
            }
            set
            {
                _SelectedRoleValueLiteralDatatype = value;
                RaisePropertyChanged(this, "SelectedRoleValueLiteralDatatype");
            }
        }

        public ObservableCollection<KeyValuePair<string, string>> Ranges
        {
            get
            {
                return _ranges;

            }
            set
            {
                _ranges = value;
            }
        }

        public ObservableCollection<KeyValuePair<string, string>> LiteralDataTypes
        {
            get
            {
                return _literalDatatypes;
            }
            set
            {
                _literalDatatypes = value;
            }
        }

        public bool HasRange
        {
            get
            {
                return _HasRange;
            }
            set
            {
                _HasRange = value;
                RaisePropertyChanged(this, "HasRange");
            }
        }

        public bool HasValue
        {
            get
            {
                return _HasValue;
            }
            set
            {
                _HasValue = value;
                RaisePropertyChanged(this, "HasValue");
            }
        }

        public bool ValueHasReference
        {
            get
            {
                return _ValueHasReference;
            }
            set
            {
                _ValueHasReference = value;
                RaisePropertyChanged(this, "ValueHasReference");
            }
        }

        public bool ValueHasLiteral
        {
            get
            {
                return _ValueHasLiteral;
            }
            set
            {
                _ValueHasLiteral = value;
                RaisePropertyChanged(this, "ValueHasLiteral");
            }
        }

        public bool IsBaseTemplate
        {
            get
            {
                return _IsBaseTemplate;
            }
            set
            {
                _IsBaseTemplate = value;
                RaisePropertyChanged(this, "IsBaseTemplate");
            }
        }

        public bool IsSpecializedTemplate
        {
            get
            {
                return _IsSpecializedTemplate;
            }
            set
            {
                _IsSpecializedTemplate = value;
                RaisePropertyChanged(this, "IsSpecializedTemplate");
            }
        }

        public bool ValueTypesEnabled
        {
            get
            {
                return _IsSpecializedTemplate && _HasValue;
            }            
        }

        public bool ValueReferenceEnabled
        {
            get
            {
                return ValueTypesEnabled && _ValueHasReference;
            }
        }

        public bool ValueLiteralEnabled
        {
            get
            {
                return ValueTypesEnabled && _ValueHasLiteral;
            }
        }
    }
}