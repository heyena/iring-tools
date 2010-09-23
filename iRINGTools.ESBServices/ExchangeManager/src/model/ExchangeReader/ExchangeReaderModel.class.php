<?php
/**
 * @author Aswini Nayak (aknayak@bechtel.com)
*/

include_once('/model/RestfulService/curl.class.php');

class ExchangeReaderModel{
	private $exchangeUrl;
	private $projectid;

	function __construct(){
            $this->exchangeUrl = DIRECTORY_REQUEST_URL;
                
	}

	function readXML(){
		$curlObj = new curl($this->exchangeUrl);
		$fetchedData = $curlObj->exec();
		print_r($fetchedData);
	}

        /**
	 * Generate the array of application data graphs: applicationID wise grouping.
	 *
	 * @param one(Object)
	 * @returns array
	 * @access private
	 */
        private function getApplicationGraphArray($appGraphs){

                $applicationGraphArray= array();

                foreach($appGraphs  as $appGraph)
                {
                        $graphArray         =	array();

                        $applicationId      = 	'appId_'.(string)$appGraph ->applicationId;
                        $graphIdentifier    = 	(string)$appGraph ->identifier;
                        $graphName          =	(string)$appGraph ->name;
                        $graphCommodity     = 	(string)$appGraph ->commodity;
                        $graphDescription   = 	(string)$appGraph ->description;

                        $graphArray = array("text" =>$graphName, "id"=>$graphIdentifier, "applicationId"=>$applicationId,"Description"=>$graphDescription, "Commodity"=>$graphCommodity, "Title" =>$graphName, "icon" =>"resources/images/16x16/file-table.png", "leaf" => "true");

                        $applicationGraphArray[$applicationId][]=$graphArray;

                }
                unset($graphArray);
                return $applicationGraphArray;
        }

        /**
	 * Generate the array of applications scope wise grouping
	 *
	 * @param one(Object)
	 * @returns array
	 * @access private
	 */
        private function getScopeDataExchangesArray($dataExchanges){

                $exchangeScopeArray	=	array();

                foreach($dataExchanges  as $dataExchange)
                {  
                        $exchangeArray 	=   array();
                        $scopeVal 	=   (string)$dataExchange->scope;
                        $dataExchangeID =   'dxId_'.(string)$dataExchange->identifier;
                        $commodity 	=   (string)$dataExchange->commodity;

                        $exchangeArray	=   array("Title"=>(string)$dataExchange->name, "text" =>(string)$dataExchange->name, "id"=>$dataExchangeID,"Scope"=>$scopeVal,"Description"=>(string)$dataExchange->description, "icon" =>"resources/images/16x16/file-table-diff.png", "leaf" => "true");

                        $exchangeScopeArray[$scopeVal][$commodity]['Title']	=   $commodity;
                        $exchangeScopeArray[$scopeVal][$commodity]['text']	=   $commodity;
                        $exchangeScopeArray[$scopeVal][$commodity]['icon']	=   'resources/images/16x16/class-badge.png';
                        $exchangeScopeArray[$scopeVal][$commodity]['children'][]=   $exchangeArray;;  // exchange Array should be inside this children
                }
                unset($exchangeArray);
                return $exchangeScopeArray;
        }

        /**
	 * Convert XML to required JSON format for the Directory Tree.
	 *
	 * @param none.
	 * @returns JSON string
	 * @access public
	 */
	function getDirectoryTreeJSONData(){

            $resultArray    =	array();
            $appScopeArray  =	array();

            $curlObj     = new curl($this->exchangeUrl);
            $fetchedData = $curlObj->exec();
            $xmlIterator = new SimpleXMLIterator($fetchedData);

            $applications       =   $xmlIterator->applications->application;
            $graphArr           =   $this->getApplicationGraphArray($xmlIterator->applicationData->graph);
            $dataExchangeArray  =   $this->getScopeDataExchangesArray($xmlIterator->dataExchanges->exchange);

                // make graphs array of Application Data along with Scope as Key of array
                foreach($applications  as $applicaton)
                {
                        $appArray 	=   array();
                        $scopeVal 	=   (string)$applicaton->scope;
                        $applicationId  =   'appId_'.(string)$applicaton->id;

                        $appArray = array("text" =>(string)$applicaton->name, "id"=>$applicationId,"Description"=>(string)$applicaton->description,'Scope'=>(string)$applicaton->scope,"Title" =>(string)$applicaton->name, "icon" => "resources/images/16x16/applications-internet.png", "children"=>$graphArr[$applicationId]);

                        $appScopeArray[$scopeVal]["Title"]	=   "Application Data";
                        $appScopeArray[$scopeVal]["text"]	=   "Application Data";
                        $appScopeArray[$scopeVal]["icon"]	=   "resources/images/16x16/folder.png";
                        $appScopeArray[$scopeVal]["children"][]	=   $appArray;
                }
                // get the scopes Exchange Manger
                $scopeArray =   array_keys($appScopeArray);

                // make complete directory tree array
                foreach($scopeArray as $scope){

                    $dataExchangeScopeArray  = $dataExchangeArray[$scope];

                    // create the array as per the same length of $dataExchangeScopeArray
                    $numericArray =  range ( 0, count($dataExchangeScopeArray)-1, 1 );

                    // changed the keys(scope value) of array to keys(0,1,..). It is must for Tree Generation
                    $dXArray  = array_combine($numericArray, $dataExchangeScopeArray);

                    $resultArray []= array("Title"=>$scope, "text"=>$scope, "icon"=>"resources/images/16x16/system-file-manager.png", "children"=>array($appScopeArray [$scope], array("Title"=>"Data Exchange", "text"=>"Data Exchange","icon" =>"resources/images/16x16/folder.png","children"=>$dXArray)));
                }
                unset($dXArray);
                unset($appArray);
                unset($graphArr );
                unset($scopeArray);
                unset($numericArray);
                unset($appScopeArray);
                unset($dataExchangeArray);
                unset($dataExchangeScopeArray);
                return json_encode($resultArray);
       }// end of function "getDirectoryTreeJSONData"
}
?>