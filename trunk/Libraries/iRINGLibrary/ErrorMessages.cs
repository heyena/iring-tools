﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace org.iringtools.library
{

    public sealed class ErrorMessages
    {

        #region Adapter Service

        public static readonly string errGetScopeName = "There is an error occured while getting Scope Name in service.";
        public static readonly string errAddScope = "There is an error occured while Adding Scope   ";
        public static readonly string errUpdateScope = "There is an error occured while Updating Scope.";
        public static readonly string errGetScope = "There is an error occured while getting Scope  in service.";
        public static readonly string errGetDictionary = "There is an error occured while getting Dictionary in service.";
        public static readonly string errAddNewApplication = "There is an error occured while Adding New Application " ;
        public static readonly string errUpdateApplication = "There is an error occured while Updating Application in service.";
        public static readonly string errDeleteScope = "There is an error occured while deleting Scope.";
        public static readonly string errSecurityGroups = "There is an error occured while getting security groups.";
        public static readonly string errGetUISettings = "There is an error occured while doing UI Settings.";
        public static readonly string errGetDataLayer = "There is an error occured while getting Data Layer.";
        public static readonly string errUdateDataLayer = "Threre is an error occured while updating Data Layer";
        public static readonly string errGetMapping = "Threre is an error occured while getting mapping";
        public static readonly string errGetCacheInfo = "There is an error occured while getting catche information.";
        public static readonly string errRefreshAll = "There is an error occured while refreshing.";
        public static readonly string errDeleteApp = "There is an error occured while deleting application";
        public static readonly string errSwitchDataMode= "There is an error occured while switching data mode";
        #endregion

        #region Directory Controller
        public static readonly string errGetUIScope = "There is an error occured while adding scope at controller";
        public static readonly string errGetUIDeleteScope = "There is an error occured while deleting scope at controller";
        public static readonly string errGetUIDeleteApplication = "There is an error occured while deleting application at controller";
        public static readonly string errGetUISecurityGroup = "There is an error occured in security group at controller";
        public static readonly string errGetUIInitializeUISettings = "There is an error occured in security group at controller";
        public static readonly string errGetUIDataLayer = "There is an error occured while getting Data Layer";
        public static readonly string errGetUINode = "There is an error occured while getting nodes";
        public static readonly string errAddUIApplication = "There is an error occured while Adding Application"; 

        #endregion

        #region Adapter Manager Controller
        public static readonly string errUIDBProviders = "There is an error occured while getting Data Base Provider";
        public static readonly string errUIDBDictionary = "There is an error occured while getting Data Base Dictionary";
        public static readonly string errUIRegenAll = "There is an error occured while regenerating";
        public static readonly string errUIRefreshAll = "There is an error occured while refereshing";
        public static readonly string errGetUICacheInfo = "There is an error occured while getting catche information";
        public static readonly string errUIRefresh = "There is an error occured while refreshing";
        public static readonly string errRefreshCache = "There is an error occured while refreshing cache";
        public static readonly string errUIDataFilter = "There is an error occured while filterind data";
        #endregion

        #region NHibernateController
        public static readonly string errUITableName = "There is an error occured while getting table name";
        public static readonly string errUItree = "There is an error occured while getting tree";
        public static readonly string errUISaveDBDirectory = "There is an error occured while saving DB Directory"; 
        #endregion
        #region MappingConroller
        public static readonly string errUIAddClassMap = "There is an error occured while class map";
        public static readonly string errUIAddTemplateMap = "There is an error occured while adding template map";
        public static readonly string errUIGetMappingNode = "There is an error occured while getting mapping node";
        public static readonly string errUIDeleteClassMap = "There is an error occured while deleting class map";
        public static readonly string errUIResetMapping = "There is an error occured while resetting mapping";
        public static readonly string errUIMakeProcessor = "There is an error occured while making processor";
        public static readonly string errUIGraphMapping = "There is an error occured while mapping graph";
        public static readonly string errUIUpdateMap = "There is an error occured while updating map";
        public static readonly string errUIDeletegraphMap = "There is an error occured while deleting graph map";
        public static readonly string errUIMapProperty = "There is an error occured while mapping property";
        public static readonly string errUIMapConstant = "There is an error occured while mapping constant";
        public static readonly string errUIMakeReference = "There is an error occured while making reference";
        public static readonly string errUIMapValueList = "There is an error occured while getting map value list";
        public static readonly string errUIDeleteMapTemplate = "There is an error occured while deleting mapping template";
        public static readonly string errUIDeleteValueList = "There is an error occured while deleting value list";
        public static readonly string errUIValueListMap = "There is an error occured while mapping  value list";
        public static readonly string errUIDeleteValueMap = "There is an error occured while deleting value map";
        public static readonly string errUICopyValueList = "There is an error occured while copying value list";
        public static readonly string errUIValueList = "There is an error occured while listing value";
        public static readonly string errUIGetLabels = "There is an error occured while getting labels";    
        #endregion

        #region FileController
        public static readonly string errUIExportFile = "There is an error occured while exporting file";
        public static readonly string errUIUploadFile = "There is an error occured while uploading file";
        public static readonly string errUIGetFile = "There is an error occured while getting file";  
        #endregion


    }
}