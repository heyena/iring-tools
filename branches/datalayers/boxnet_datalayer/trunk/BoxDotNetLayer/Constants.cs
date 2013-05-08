using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bechtel.DataLayer
{
    public static class Constants
    {
        public const string DELIMITER_CHAR = "^";
        public const string OBJECT_PREFIX = "Object_";

        // NP
        public const string Search = "Search";
        public const string SearchParamName = "q";
        // NP

        public static class ObjectName
        {
            public const string Folders = "folders";
            public const string Files = "documents";
            public const string Locations = "Locations";
        }

        public enum SearchObjectType
        {
            folder,
            file
        };


    }
}
