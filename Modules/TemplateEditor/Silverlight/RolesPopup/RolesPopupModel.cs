using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace org.iringtools.modules.templateeditor.rolespopup
{
    public class RolesPopupModel
    {
        internal ObservableCollection<KeyValuePair<string, string>> _ranges;
        internal ObservableCollection<KeyValuePair<string, string>> _literalDatatypes;

        private KeyValuePair<string, string> _SelectedRoleRange;
        private string _SelectedRoleValueReference;
        private string _SelectedRoleValueLiteral;
        private KeyValuePair<string, string> _SelectedRoleValueLiteralDatatype;

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

    }
}