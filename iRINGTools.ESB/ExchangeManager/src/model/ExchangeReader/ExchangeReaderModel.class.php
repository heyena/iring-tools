<?php
/**
 * @author Aswini Nayak (aknayak@bechtel.com)
*/

include_once('/model/RestfulService/curl.class.php');

class ExchangeReaderModel{
	private $exchangeUrl;
	private $projectid;

	function __construct(){
		//$this->projectid=$id;
		//$this->exchangeUrl = 'http://localhost:81/test/PRM/testxml.xml';
                $this->exchangeUrl = 'http://localhost:81/projects/iRINGTools.ESB/ExchangeManager/directory.xml';
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
		$curlObj = new curl($this->exchangeUrl);
		$fetchedData = $curlObj->exec();
		
		$xmlIterator = new SimpleXMLIterator($fetchedData);
		$resultArr="" ;
                foreach($xmlIterator  as $projects)
                {
                    if($projects ->getName() == "project"){ // element should be "project"

                        // if there is some application elements exists then create the children node
                        if($projects->dataExchanges->commodity->count()>0){

                            foreach($projects ->applicationData->children() as $application){

                                $grArr = array();
                                foreach($application->children()  as $graph){

                                    $grArr []= array("text"=>(string)$graph['name'], "icon" =>"resources/images/graph-icon.png", "leaf" => "true");
                                }
                                $appArr[]= array("text"=>(string)$application['name'], "id"=>(string)$application['id'], "children" => $grArr);
                            }
                        }
                        $appDataArr = array("text" => "Application Data", "icon" => "resources/images/app-icon.png", "children" => $appArr);

                        if($projects->applicationData->application->count()>0){

                            foreach($projects->dataExchanges->children() as $commodity){

                                foreach($commodity->children() as $exchange){

                                    $exchangeArr[] = array("text"=>(string)$exchange['name'],"id"=>(string)$exchange['id'],"icon" =>"resources/images/exc.bmp", "leaf" => "true");
                                }

                                $commArr[]  = array("text" => (string)$commodity['name'],"id" => (string)$commodity['id'], "cls" => "folder", "children"=>$exchangeArr);
                            }
                        }

                        $dataExchangeArr =  array("text" => "Data Exchange" ,"icon" =>"resources/images/refresh.gif", "children" => $commArr);

                        $appData_commArr= array($appDataArr,$dataExchangeArr);

                        $resultArr = array(array("text" =>(string)$projects['name']."[".$projects['id']."]", "cls" => "folder", "children" => $appData_commArr));
                       
                        }// end of if for check "project" element

                        else {
                            trigger_error('Invalid XML format to generate Tree.',E_USER_ERROR);
                        }
                    unset($grArr);
                    unset($appArr);
                    unset($appDataArr);
                    unset($exchangeArr);
                    unset($commArr);
                    unset($dataExchangeArr);
                    unset($appData_commArr);
                }                
                return json_encode($resultArr);
              }
         }// end of function "getDirectoryTreeJSONData"
?>