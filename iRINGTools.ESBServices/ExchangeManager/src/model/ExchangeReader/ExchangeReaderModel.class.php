<?php
/**
 * @author Aswini Nayak (aknayak@bechtel.com)
*/

include_once('model/RestfulService/curl.class.php');

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
	 * Convert XML to required JSON format for the Directory Tree.
	 *
	 * @param none.
	 * @returns JSON string
	 * @access public
	 */
	function getDirectoryTreeJSONData(){

            $resultArray = array();
            $curlObj     = new curl($this->exchangeUrl);
            $fetchedData = $curlObj->exec();
            $xmlIterator = new SimpleXMLIterator($fetchedData);

            // loop for scopes array
            foreach($xmlIterator->scope  as $scope)
            {
                 $applicationArray = array();
                // loop for applications array under each scope
                foreach($scope->applicationData->children() as $application){
               
                    $graphArray = array();
                    // loop for all graphs under each application
                    foreach($application->graphs->children() as $graph){
                       $graphArray[] = array(
                        'id'=> 'gpId_'.(string)$graph->id,
                        'uid'=> (string)$graph->id,
                        'text'=> (string)$graph->name,
                        'Title'=> (string)$graph->name,
                        'Description'=> (string)$graph->description,
                        'Commodity' => (string)$graph->commodity,
                        'icon' =>'resources/images/16x16/file-table.png',
                        'node_type' => 'graph',
                        'Scope' => (string)$scope->name,
                        'leaf' => 'true'
                      );
                    }
                    $applicationArray[]= array(
                        'id'=>'appId_'.(string)$application->id,
                        'uid'=>(string)$application->id,
                        'text'=>(string)$application->name,
                        'Title'=>(string)$application->name,                        
                        'Desccription'=>(string)$application['description'],
                        'node_type' => 'application',
                        'Scope' => (string)$scope->name,
                        'icon'=>'resources/images/16x16/applications-internet.png',
                        'children'=>$graphArray
                       );
                    

                    unset($graphArray);
                }

                $commodityArray = array();
                //loop for get all commodity array under each scope
                foreach($scope->dataExchanges->children() as $commodity){
                   $exchangeArray = array();
                    // get all dataExchange array under each commodity of scope
                    foreach($commodity->exchanges->children() as $exchange){
                      
                        $exchangeArray[] = array(
                        'id'=> 'exID_'.(string)$exchange->id,
                        'uid'=> (string)$exchange->id,
                        'text'=> (string)$exchange->name,
                        'Title'=> (string)$exchange->name,
                        'Description'=> (string)$exchange->description,
                        'Commodity' => (string)$commodity->name,
                        'Scope' => (string)$scope->name,
                        'icon' =>'resources/images/16x16/file-table-diff.png',
                        'node_type' => 'exchanges',
                        'leaf' => 'true'
                      );
                    }
                    $commodityArray[]= array(
                        'text'=>(string)$commodity->name,
                        'Title'=>(string)$commodity->name,
                        'node_type' => 'commodity',
                        'icon'=>'resources/images/16x16/class-badge.png',
                        'children'=>$exchangeArray
                       );

                    unset($exchangeArray);
                }
                    // combine Application Data and Data Exchange arrays
                    $scopeArray[]=array(
                        'text'=>(string)(string)$scope->name,
                        'Title'=>(string)(string)$scope->name,
                        'node_type' => 'scope',
                        'icon'=>'resources/images/16x16/system-file-manager.png',
                        'children'=>array(
                             array(
                             'text'=>'Application Data',
                             'icon'=>'resources/images/16x16/folder.png',
                             'children'=>$applicationArray),
                            array(
                             'text'=>'Data Exchange',
                             'icon'=>'resources/images/16x16/folder.png',
                             'children'=>$commodityArray),
                            )
                    );             
              }

             unset($applicationArray);
             unset($commodityArray);
             return json_encode($scopeArray);
        
       }// end of function "getDirectoryTreeJSONData"
}
?>