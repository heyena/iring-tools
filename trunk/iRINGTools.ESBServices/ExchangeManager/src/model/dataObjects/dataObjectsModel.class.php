<?php
/**
 * @author Aswini Nayak (aknayak@bechtel.com)
*/

include_once('model/RestfulService/curl.class.php');

class dataObjectsModel{
	private $exchangeUrl;
	private $dtiUrl;
	private $dtoUrl;
	private $dtiXMLData;
	private $nodeType,$uriParams;
	private $cacheKey,$dtocacheKey,$cacheDTI;
	private $rdlArray=array();
	function __construct(){
	}

	/* This function will assign different types of keys[e.g. $cacheKey,$dtocacheKey,$cacheDTI] depending on the nodetype
	 * Call this function when we want to check the key of Particular type
	 */
	private function setCacheKey($params,$type=null){
            
		$this->nodeType=$params['nodetype'];
		switch($this->nodeType){
			case "exchanges":
				$this->cacheKey = $type.''.$params['scope'].'_'.$params['nodetype'].'_'.$params['exchangeID'];
				$this->dtocacheKey = $type.''.'dto_'.$params['scope'].'_'.$params['nodetype'].'_'.$params['exchangeID'];
                                if(isset($params['exchangeAction']) && $params['exchangeAction']=="viewHistory"){
                                    $this->historyCacheKey = 'hst_'.$type.''.$params['scope'].'_'.$params['nodetype'].'_'.$params['exchangeID'];
                                }
				break;
			case "graph":
				$this->cacheKey = $type.''.$params['scope'].'_'.$params['applname'].'_'.$params['graphs'];
				$this->dtocacheKey = $type.''.'dto_'.$params['scope'].'_'.$params['applname'].'_'.$params['graphs'];
				break;
		}

	}
	
	private function buildWSUri($params){
		$this->nodeType=$params['nodetype'];
		switch($this->nodeType){
			case "exchanges":
             if(!isset($params['exchangeAction'])){
				if(isset($params['hasreviewed'])){
					$append ='/submit';
					$this->dtiSubmitUrl = DXI_REQUEST_URL.'/'.$params['scope'].'/'.$params['nodetype'].'/'.$params['exchangeID'].$append;
					//$this->dtiSubmitUrl = DXI_SUBMIT_REQUEST_URL;
					
				}
				$this->dtiUrl = DXI_REQUEST_URL.'/'.$params['scope'].'/'.$params['nodetype'].'/'.$params['exchangeID'];
				$this->dtoUrl = DXO_REQUEST_URL.'/'.$params['scope'].'/'.$params['nodetype'].'/'.$params['exchangeID'];
				$this->setCacheKey($params);
                            }else if($params['exchangeAction']=='viewHistory'){
                            $this->dtiHistoryUrl = EXCHANGE_HISTORY_URI;//.'/'.$params['scope'].'/'.$params['nodetype'].'/'.$params['exchangeID'];
                            $this->setCacheKey($params);
                        }
                        break;

			case "graph":
				$this->dtiUrl = APP_REQUEST_URI.'/'.$params['scope'].'/'.$params['applname'].'/'.$params['graphs'].'?hashAlgorithm=MD5';
				$this->dtoUrl = APP_REQUEST_URI.'/'.$params['scope'].'/'.$params['applname'].'/'.$params['graphs'].'/page';
				$this->setCacheKey($params);
				break;
		}
	}

	/*
	get from cache
	if its available & not empty then go for actual exchange
	*/
	function setDataObjects($params){
		$this->buildWSUri($params);
		$this->dtiXMLData = $this->getCacheData();
		
		// checking the dti from cache
		if(($this->dtiXMLData!=false)&&(!empty($this->dtiXMLData))){
			//$replaceString = str_replace('<dataTransferIndices xmlns="http://iringtools.org/adapter/dti">','<xr:exchangeRequest xmlns="http://iringtools.org/adapter/dti" xmlns:xr="http://iringtools.org/common/request"><xr:dataTransferIndices>',$this->dtiXMLData);
				//****$replaceString = str_replace('<dataTransferIndices xmlns="http://www.iringtools.org/dxfr/dti">','<xr:exchangeRequest xmlns="http://www.iringtools.org/dxfr/dti" xmlns:xr="http://www.iringtools.org/dxfr/request"><xr:dataTransferIndices>',$this->dtiXMLData);

				$replaceString = str_replace('<dataTransferIndices xmlns="http://www.iringtools.org/dxfr/dti">','<xr:exchangeRequest xmlns="http://www.iringtools.org/dxfr/dti" xmlns:xr="http://www.iringtools.org/dxfr/request"><xr:dataTransferIndices>',$this->dtiXMLData);
				//*** $sendXmlData = str_replace("</dataTransferIndices>","</xr:dataTransferIndices><xr:reviewed>".$params['hasreviewed']."</xr:reviewed></xr:exchangeRequest>", $replaceString);
				$sendXmlData = str_replace("</dataTransferIndices>","</xr:dataTransferIndices><xr:reviewed>".$params['hasreviewed']."</xr:reviewed></xr:exchangeRequest>", $replaceString);
				
				
				$curlObj = new curl($this->dtiSubmitUrl);
				$curlObj->setopt(CURLOPT_POST, 1);
				$curlObj->setopt(CURLOPT_HTTPHEADER, Array("Content-Type: application/xml"));
				$curlObj->setopt(CURLOPT_POSTFIELDS,$sendXmlData);
				$curlObj->setopt(CURLOPT_HEADER, false);
				$fetchedData = $curlObj->exec();
				$curlObj->close();
				if(!empty($fetchedData)){
					// Removing the cache
					$this->removeDtiCache();
					$resultArray = $this->getDataxchangeResult($fetchedData);
					$rowdataArray=array();
					// check the array is created and if its not that means some error was from WS & display the custom messgae in the grid

					if(!empty($resultArray)){
						foreach($resultArray as $key =>$val){
							$rowdataArray[]=array($key,$val);
						}
					}else{
						$rowdataArray[] = array("<font color='green'><b>Error</b></font>","<font color='green'><b>Error</b></font>");
					}
					$columnsDataArray = array(array('id'=>'Identifier','header'=>'Identifier','dataIndex'=>'Identifier'),array('id'=>'Message','header'=>'Message','sortable'=>'true','width'=>350,'dataIndex'=>'Message'));
					$headerListDataArray=array(array('name'=>'Identifier'),array('name'=>'Message'));
					echo json_encode(array("success"=>"true",
										   "headersList"=>(json_encode($headerListDataArray)),
										   "rowData"=>json_encode($rowdataArray),
										   "columnsData"=>json_encode($columnsDataArray)));
				}else{
					echo json_encode(array("success"=>"false","response"=>"Server not responding"));
				}
		}else{
			// when cache detsroyed and DTI not found from cache in that case we will agin send the request to get the latest dti
			// & won't need to generate the DTO just submit this dti
			// call getDataObjects($params,$dtoRequired=false)
			$this->setDtitoCache($params);
			$this->setDataObjects($params);
			exit;
			//*** echo json_encode(array("success"=>"false","response"=>"Please Try again.. Cache destroyed"));
		}
	}
	private function getDataxchangeResult($fetchedData){
		$xmlIterator = new SimpleXMLIterator($fetchedData);
		$resultArr=array();
		$statusList = $xmlIterator->statusList;

		// check the response level from ws it may be success/error
		/*$wsResponselevel = $xmlIterator->level;
		if($wsResponselevel=='')
		*/
		
		foreach($statusList->status as $status)
		{
			//print_r($status);

				$identifier = (string)$status->identifier;
				
				foreach($status->messages as $message)
				{
					//print_r($message);
					$messagestr = (string)$message->message;
					$resultArr[$identifier]=$messagestr;
				}

		}
		return $resultArr;
	}
	
	private function removeDtiCache(){
		@session_start();
		if(isset($_SESSION['dti_detail'][$this->cacheKey]))
		{
			unset($_SESSION['dti_detail'][$this->cacheKey]);
			unset($_SESSION[$this->dtocacheKey]);
			unset($_SESSION[$this->dtocacheKey.'_dtoCounts']);
			return true;
		}else{
			return false;
		}
	}

	private function cacheData($dtiXMLData){
		@session_start();
		//echo 'Session id: '.session_name();
		if(!isset($_SESSION['dti_detail'][$this->cacheKey]))
		{
			$_SESSION['dti_detail'][$this->cacheKey] = $dtiXMLData;
		}

	}

	function getCacheData(){
		@session_start();
		if(isset($_SESSION['dti_detail'][$this->cacheKey]))
		{
			return $_SESSION['dti_detail'][$this->cacheKey];
		}
	}

	function checkCacheData(){
		@session_start();
		if(!isset($_SESSION['dti_detail'][$this->cacheKey]))
		{
			return false;
		}else{
			return true;
		}
	}


	private function setDtitoCache($params){
		if($this->checkCacheData()){
			$this->cacheDTI = true;
			$this->dtiXMLData = $this->getCacheData();
		}else{
			$this->dtiXMLData = $this->getDtiInfo($params);
			// cache the data
			if(($this->dtiXMLData!=false) && (!empty($this->dtiXMLData))) $this->cacheData($this->dtiXMLData);
		}
	}

	/*
	 * @params : array of exchangeID,scopeid,nodetype,limit
	 */

	function getDataObjects($params)
	{
		
	 /**
	  * Will use the DXIObject to get the identifiers List with the particular exchangeID
	 */
		// This function generates the uri and assign the uniquie-key to store in cache
		$this->buildWSUri($params);
		$this->setDtitoCache($params);
		
			/* we will check the key's existence from
				session and if its there then fetch from
				session or else send the request to get the dti info
			*/
		if(($this->dtiXMLData!=false)&&(!empty($this->dtiXMLData))){
			$dxoResponse = $this->getDXOInfo($this->uriParams,$this->dtiXMLData);
			if(($dxoResponse!=false) && ($dxoResponse!='')){
				return json_encode($this->createJSONDataFormat($dxoResponse));
			}else{
				echo json_encode(array("success"=>"false"));
			}
		}else{
			echo json_encode(array("success"=>"false"));
		}
	}

	/**
		@params
		get the Data transfer indexes
	 */

	private function getDtiInfo($params){
		$curlObj = new curl($this->dtiUrl);
		// will work on this
		$curlObj->setopt(CURLOPT_USERPWD,AUTH_USERID.":".AUTH_USERPWD);
		$fetchedData = $curlObj->exec();
		$curlObj->close();
		return $this->validateResponseCode($curlObj,$fetchedData);
	}

	// Function to validate the http_code
	private function validateResponseCode($curlObj,$fetchedData){
		if(($curlObj->getStatus('http_code')==200) && ($curlObj->getStatus('errno')==0)){
			return $fetchedData;
		}else{
			return false;
		}
	}

	private function getDXOInfo($exchangeID,$postParams){
		$this->dtoUrl = $this->dtoUrl;
		$curlObj = new curl($this->dtoUrl);
		$curlObj->setopt(CURLOPT_POST, 1);
		$curlObj->setopt(CURLOPT_HTTPHEADER, Array("Content-Type: application/xml"));
		$curlObj->setopt(CURLOPT_POSTFIELDS,$postParams);
		$curlObj->setopt(CURLOPT_HEADER, false);
		// will work on this
		$curlObj->setopt(CURLOPT_USERPWD,AUTH_USERID.":".AUTH_USERPWD);
		$fetchedData = $curlObj->exec();
		$curlObj->close();
		return $this->validateResponseCode($curlObj,$fetchedData);
		
	}


	/**
	 * This function used to traversing the Class Objects of a particular DataTransferObject
	 *
	 * @param three(Object,Object,Object)
	 * @returns Array
	 * @access public
	 */
	function classObjectsTraverse($dataTransferObject,$classObject,$mainClassObject){
		$headerNamesArray=array();
		$classReferenceArray=array();
		$tempRoleValueArray=Array();
		$tempClassReferenceArray=array();

		// Traverse each templateObjects under the Main classObject
		foreach($classObject->templateObjects->templateObject as $templateObject)
		{

			// iterate Role objects under each template
			$tempRoleObjectNameArray = array();
			foreach($templateObject->roleObjects->children() as $roleObject)
			{
				$tempKey='';
				if(stristr((string)$roleObject->type,'Reference') && ((strpos($roleObject->value, '#'))===0))
				{

					$refClassIdentifier  = ltrim((string)$roleObject->value,'#');//(string)substr($roleObject->value, 1, -1);
					$dtoIdentifier = (string)$dataTransferObject->identifier;
					$relatedClassName = (string)$roleObject->relatedClassName;

					$key = "'".$dtoIdentifier."'";
	//				$value = '<div class="x-panel-header x-accordion-hd" style="cursor:pointer"><a href="#" style="cursor:pointer;text-decoration:none" onClick=this.displayRleatedClassGrid(\''.$refClassIdentifier.'\',\''.$dtoIdentifier.'\',\''.$relatedClassName.'\')>'.$relatedClassName.'</a></div>';

					//$value = '<span href="#" style="cursor:pointer;text-decoration:none">'.$relatedClassName.'</span>';
					
					$value = $relatedClassName;
					if(isset($tempClassReferenceArray[$key])){
						$tempClassReferenceArray[$key] = ''.$value.''.$tempClassReferenceArray[$key];
						$relatedClassList[]=array("name"=>$value,"reference"=>$refClassIdentifier,'identifier'=>$key);
					}
					else{
						$tempClassReferenceArray[$key]=$value;
						$relatedClassList[]=array("name"=>$value,"reference"=>$refClassIdentifier,'identifier'=>$key);

					}
				}
				if(stristr($roleObject->type,'Property'))
				{
					$tempRoleObjectNameArray[]="$roleObject->name";
					$tempKey = (string)$templateObject->name.'.'.(string)$roleObject->name;

					$spanColor='';
					switch (strtolower((string)$dataTransferObject->transferType))
					{
						case "add":
							$spanColor='red';
							break;
						case "change":
							$spanColor='blue';
							break;
						case "delete":
							$spanColor='green';
							break;
						case "sync":
							$spanColor='black';
							break;
					}

									// We are adding custom keys to the array
					if($this->nodeType=='exchanges'){
						$tempRoleValueArray['TransferType']='<span style="cursor:pointer;color:'.$spanColor.'">'.(string)$dataTransferObject->transferType.'</span>';
						$tempRoleValueArray['Identifier']  ='<span style="color:'.$spanColor.';cursor: pointer;text-decoration: underline">'.(string)$dataTransferObject->identifier.'</span>';
					}

					// condition to check if the transferType is change for role->type
					if($dataTransferObject->transferType=='Change')
					{
						$value='';
						$oldvalue='';
						$convertedvalue='';
						$convertedoldValue='';

						if(isset($roleObject->value)){
							$value=(string)$roleObject->value;
						}
						if(isset($roleObject->oldValue)){
							$oldvalue=(string)$roleObject->oldValue;
						}

						// call and store the rdl values by checking the value contains rdl:
						$convertedvalue = (stristr($value,'rdl:')) ? $this->stroreRdlValues($value,substr($value,4)):$value;
						$convertedoldValue = (stristr($oldvalue,'rdl:')) ? $this->stroreRdlValues($oldvalue,substr($oldvalue,4)):$oldvalue;

						// if there is any difference between old and new then represent as old->new
						if($convertedoldValue!=$convertedvalue){
							//if($oldvalue!='' && $value!=''){
							$tempRoleValueArray[$tempKey]='<span style="cursor:pointer;color:'.$spanColor.'">'.$convertedoldValue.'->'.$convertedvalue.'</span>';
															//}else{
																	//$tempRoleValueArray[$tempKey]='<span style="color:'.$spanColor.'">'.$oldvalue.$value.'</span>';
															//}

						}else{
							$tempRoleValueArray[$tempKey]=$convertedoldValue;
						}
					}else{
						$convertedvalue = (stristr((string)$roleObject->value,'rdl:')) ? $this->stroreRdlValues((string)$roleObject->value,substr((string)$roleObject->value,4)):(string)$roleObject->value;
						$tempRoleValueArray[$tempKey]='<span style="cursor:pointer;color:'.$spanColor.'">'.$convertedvalue.'</span>';
					}
					unset($tempKey);
				}
			}

					// condition to make the Header & Row
			if(count($tempRoleObjectNameArray)>1){
				foreach($tempRoleObjectNameArray as $key=>$val){
					$headerNamesArray[]=(string)$templateObject->name.'.'.$val;
				}
			}else if(count($tempRoleObjectNameArray)==1){

				$headerNamesArray[]=(string)$templateObject->name;

							/*
                                    Check the $tempRoleValueArray that store the value of role with "concanated key"
                                    We need to modify that "concanated key" with originalheader
                             */
				foreach($tempRoleObjectNameArray as $key=>$val){
					$keyname = (string)$templateObject->name.'.'.$val;
					if(array_key_exists($keyname,$tempRoleValueArray)){
						$tempRoleValueArray["$templateObject->name"]=$tempRoleValueArray[$keyname];
						unset($tempRoleValueArray[$keyname]);
					}
				}

			}
			unset($tempRoleObjectNameArray);
		}


		$key="'".(string)$dataTransferObject->identifier."'";

		
		
		if(isset($tempClassReferenceArray["'".$dataTransferObject->identifier."'"])){
			$classReferenceLabelArray=array("identifier"=>(string)$dataTransferObject->identifier,
											"label"=>json_encode($relatedClassList),
											"text"=>$tempClassReferenceArray["'".$dataTransferObject->identifier."'"]);
		}else{
			$classReferenceLabelArray=array("identifier"=>(string)$dataTransferObject->identifier,
											"label"=>json_encode($relatedClassList),
											"text"=>'');
		}

		//"label":"{\"'90002-RV'\":[\"PLANT AREA\",\"P AND I DIAGRAM\"]}",
		
		$returnArray=array('headerNamesArray'=>$headerNamesArray,'tempRoleValueArray'=>$tempRoleValueArray,'classReferenceArray' =>$classReferenceLabelArray);

		return $returnArray;
	}
	
	private function createJSONDataFormat($fetchedData){
		$xmlIterator = new SimpleXMLIterator($fetchedData);
		$resultArr="";

		//echo '<pre>';
		//print_r($xmlIterator);
		
		//$dataTransferObjects = $xmlIterator->dataTransferObject;
		$dataTransferObjects = $xmlIterator->dataTransferObjectList->dataTransferObject;
		// Total no of datatransferobject elements found
		$dtoCounts  = count($dataTransferObjects);

		$headerNamesArray=array();
		$headerListDataArray = array();
		$rowsArray=array();
		
		/* This array $gridRowsArray will store all rows of a dto-response
		 * This will be stored as session variable
		*/
		$gridRowsArray = array();
		$gridFilterArray = array();
		$classReferenceArray=array();
        $classObjectName='';

		$dtoCnter=0;
		foreach($dataTransferObjects as $dataTransferObject)
		{
			$i=0;
			foreach($dataTransferObject->classObjects->children() as $classObject)
			{
				$i++;
                                
				// Main class object
				if($i==1)
				{
                                    $classObjectName = (string)$classObject->name;
                                    $mainClassObject = 0;
                                    $returnArray = $this->classObjectsTraverse($dataTransferObject,$classObject,$mainClassObject);
                                    $tempRoleValueArray = $returnArray['tempRoleValueArray'];
                                    $headerNamesArray = $returnArray['headerNamesArray'];
                                    $classReferenceArray[] = $returnArray['classReferenceArray'];
                                    

				}else
				{
					// Skip the indirect classObject
				}
			}

			$rowsArray[]=$tempRoleValueArray;
			unset($tempRoleValueArray);
		}

		
		if($this->nodeType=='exchanges'){
			$customListArray=array('TransferType','Identifier');
			$sortBy="TransferType";
		}else{
			$customListArray=array();
			$sortBy=DEFAULT_SORT_COLUMN;
		}
		$headerArrayList = array_merge($customListArray,array_values((array_unique($headerNamesArray))));
		unset($customListArray);
		unset($headerNamesArray);
		$rowsDataArray = array();
		$columnsDataArray = array();

		/*
		 Loop over each row to genrate the json format required to display
		 rows in grid
		*/
		
		for($i=0;$i<count($rowsArray);$i++){
			for($j=0;$j<count($headerArrayList);$j++){
				$headerName = $headerArrayList[$j];
				
				if(array_key_exists($headerName,$rowsArray[$i])){
					$rowsDataArray[$i][]=$rowsArray[$i][$headerName];
					/* The required format for JSON Reader is underscore supported so we are changing . with _ */
					/* We are striping as we need it for sorting purpose*/
					$gridRowsArray[$i][str_replace(".", "_", $headerName)]=strip_tags(trim($rowsArray[$i][$headerName]));
				}else{
					$rowsDataArray[$i][]='';
					/* The required format for JSON Reader is underscore supported so we are changing . with _ */
					$gridRowsArray[$i][str_replace(".", "_", $headerName)]='';
				}
				
			}
		}

		/* Store the $gridRowsArray that contains all rows for a dto inside session. This will be used to fetch the records page wise in getPageData() function  */
		if(is_array($rowsDataArray) && !empty($rowsDataArray)){
			@session_start();
			//$_SESSION['Page_no']=$gridRowsArray;
			//echo '<br>key:  '. $this->dtocacheKey;
			
			if(!isset($_SESSION[$this->dtocacheKey])){
				$_SESSION[$this->dtocacheKey]=$gridRowsArray;
				$_SESSION[$this->dtocacheKey.'_dtoCounts']=$dtoCounts;
			}
			unset($gridRowsArray);
		}
		
		if($this->nodeType=='exchanges'){
			$headerListDataArray[]=array('name'=>'Identifier','name'=>'TransferType');

		}
		
		foreach($headerArrayList as $key =>$val){
			$headerListDataArray[]=array('name'=>str_replace(".", "_", $val));
			//*** $columnsDataArray[]=array('id'=>str_replace(".", "_", $val),'header'=>$val,'width'=>(strlen($val)<20)?110:strlen($val)+130,"renderer"=>"genre_name",'sortable'=>'true','dataIndex'=>str_replace(".", "_", $val));
			$columnsDataArray[]=array('id'=>str_replace(".", "_", $val),'header'=>$val,'width'=>(strlen($val)<20)?110:strlen($val)+130,'sortable'=>'true','dataIndex'=>str_replace(".", "_", $val));
			$gridFilterArray[]=array('type'=> 'string','dataIndex'=>str_replace(".", "_", $val));
		}

		//**** echo json_encode(array("classObjName"=>$classObjectName,'relatedClasses'=>$classReferenceArray,"success"=>"true","cacheData"=>$this->cacheDTI,"rowData"=>json_encode($rowsDataArray),"columnsData"=>json_encode($columnsDataArray),"headersList"=>(json_encode($headerListDataArray))));

		echo json_encode(array("sortBy"=>$sortBy,"sortOrder"=>DEFAULT_SORT_ORDER,"pageSize"=>PAGESIZE,"filterSet"=>$gridFilterArray,"classObjName"=>$classObjectName,'relatedClasses'=>$classReferenceArray,"success"=>"true","cacheData"=>$this->cacheDTI,"columnsData"=>json_encode($columnsDataArray),"headersList"=>(json_encode($headerListDataArray))));
		unset($jsonrowsArray);
		unset($rowsArray);
		unset($headerArrayList);
		exit;

	}

	/* Filter function for Pageof Data
	 * Marshall the dto array stored in session which is passed as parameter @$gridArray
	 * Create the filter array based on @$filters passed 
	 * Count the total rows matched
	 * Make the grid in json format
	 * send the response to CS 
	 */
	private function getFilterPageData($params,$start,$limit,$identifier,$refClassIdentifier,$gridArray,$filters){
		if(is_array($gridArray)){
			// GridFilters sends filters as an Array if not json encoded
			if (is_array($filters)) {
				$encoded = false;
			} else {
				$encoded = true;
				$filters = json_decode($filters);
			}

			// initialize variables
			//*** $filterQs = '';
			//*** $qs = '';
			//echo '<pre>';
			//print_r($filters);

			$filterFlag=0;
			// loop through filters sent by client
			if (is_array($filters)) {

				$filterArray=array();

				for ($i=0;$i<count($filters);$i++){
					$filter = $filters[$i];
					// assign filter data (location depends if encoded or not)
					if ($encoded) {
						$field = $filter->field;
						$value = $filter->value;
						$compare = isset($filter->comparison) ? $filter->comparison : null;
						$filterType = $filter->type;
					} else {
						$field = $filter['field'];
						$value = $filter['data']['value'];
						$compare = isset($filter['data']['comparison']) ? $filter['data']['comparison'] : null;
						$filterType = $filter['data']['type'];
					}

					switch($filterType){
						  case 'string' :
							  //*** $qs .= "/".$field."/".$value."";
							  $filterArray[$field]=$value;
							  Break;
						/*case 'list' :
							if (strstr($value,',')){
								$fi = explode(',',$value);
								for ($q=0;$q<count($fi);$q++){
									$fi[$q] = "'".$fi[$q]."'";
								}
								$value = implode(',',$fi);
								$qs .= " AND ".$field." IN (".$value.")";
							}else{
								$qs .= " AND ".$field." = '".$value."'";
							}
							Break;*/
						/*case 'boolean' : $qs .= " AND ".$field." = ".($value); Break;
						case 'numeric' :
							switch ($compare) {
								case 'eq' : $qs .= " AND ".$field." = ".$value; Break;
								case 'lt' : $qs .= " AND ".$field." < ".$value; Break;
								case 'gt' : $qs .= " AND ".$field." > ".$value; Break;
							}
							Break;
						case 'date' :
							switch ($compare) {
								case 'eq' : $qs .= " AND ".$field." = '".date('Y-m-d',strtotime($value))."'"; Break;
								case 'lt' : $qs .= " AND ".$field." < '".date('Y-m-d',strtotime($value))."'"; Break;
								case 'gt' : $qs .= " AND ".$field." > '".date('Y-m-d',strtotime($value))."'"; Break;
							}
							Break;*/
					}
				}
				//**** $filterQs .= $qs;
			}

			/*echo $filterQs.'<br/>';
			echo '<pre>';
			print_r($filterArray);
			*/

			$totalCnt=0;
			foreach($gridArray as $gridValues){
				$matchCntr=0;
				foreach($gridValues as $key=>$val){
					// checks matched key 
					if(array_key_exists($key,$filterArray)){
						$filterValue = $filterArray[$key];
						$originalValue= strip_tags($val);
						if(@stristr($filterValue,$originalValue)){
							$matchCntr++;
						}
					}
					
				}
				// when everything matched then store it for gridview
				if($matchCntr==count($filterArray)){
					$totalCnt++;
					$gridResult[]=$gridValues;
				}
			}
			for($i=$start;$i<$start+$limit;$i++){
				if(isset($gridResult[$i])){
					$result[]=$gridResult[$i];
				}
			}
			//echo "<br>total count: ".$totalCnt;
			//echo "<br>json format: ".json_encode($result);
			$responseArray = array("success"=>"true","total"=>$totalCnt,"data"=>$result);

			
		}else{
			// No dto stored in session variable
			$responseArray = array("success"=>"false");
		}

		return json_encode($responseArray);
	}

	/* This function is used to return the json formated data to generate the grid using start and limit for Pagingtool
	 * We have already stored all records as an array inside session variables during the getDataObject() call.
	 * Depending on the start and limit parameters we will fetch the records from the  array that already stored inside a session
	 * As we have stored the records with unique key for each dto we should fetch the corresponding key and that can happen using  $this->setCacheKey($params) function
	 * setCacheKey($params) function builds and assigns the key using @params 
	 */

	function getPageData($params,$start,$limit,$identifier,$refClassIdentifier,$filters,$sortfield,$dir){
		// call the function  setCacheKey to get the dtocacheKey
		$this->setCacheKey($params);
		//echo '<br>key: '.$this->dtocacheKey;
		@session_start();

		if($identifier!=0 && $refClassIdentifier!=0){
			$gridArray = $_SESSION['rel_'.$this->dtocacheKey.'_'.$identifier.'_'.$refClassIdentifier];
			$total = $_SESSION['rel_'.$this->dtocacheKey.'_'.$identifier.'_'.$refClassIdentifier.'_dtoCounts'];
		}else{
			$gridArray = $_SESSION[$this->dtocacheKey];
			$total = $_SESSION[$this->dtocacheKey.'_dtoCounts'];
		}

		// if there is some Filter parameters available then just call the getFilterPageData()
		if($filters){
			return $this->getFilterPageData($params,$start,$limit,$identifier,$refClassIdentifier,$gridArray,$filters);
		}else{
		if(is_array($gridArray)){

			/*  Sorting started */
			foreach($gridArray as $key => $row){
				if(isset($sortfield)&&($sortfield!='')){
					$sortArray[$key]  = $row[$sortfield];
				}
			}

			if(isset($sortArray)){
				if($dir=='ASC'){
					array_multisort($sortArray, SORT_ASC, $gridArray);
				}else if($dir=='DESC'){
					array_multisort($sortArray, SORT_DESC,SORT_STRING, $gridArray);
				}
			}
			/*  Sorting end */
			for($i=$start;$i<$start+$limit;$i++){
				if(isset($gridArray[$i])){
					$result[]=$gridArray[$i];
				}
			}
			$responseArray = array("success"=>"true","total"=>$total,"data"=>$result);
		}else{
			$responseArray = array("success"=>"false");
		}
		return json_encode($responseArray);//strip_tags(json_encode($_SESSION['dto_detail']['Page_'.$pageno]));
		}
	}
	
	function deleteDataObjects($params)
	{
		$this->buildWSUri($params);
		if($this->removeDtiCache()!=true){
			echo json_encode(array("success"=>"false"));
		}else{
			echo json_encode(array("success"=>"true"));
		}
	}

	private function stroreRdlValues($originalvalue,$value){
		if(array_key_exists($originalvalue,$this->rdlArray)){
			return $this->rdlArray[$originalvalue];
		}else{
			$this->rdlArray[$originalvalue] = $this->getRdlValue($value);
			return $this->rdlArray[$originalvalue];
		}
	}

	private function getRdlValue($value){
		$rdlUri = RDL_REQUEST_URI.'/'.$value.'/label';
		$curlObj = new curl($rdlUri);
		$curlObj->setopt(CURLOPT_USERPWD,AUTH_USERID.":".AUTH_USERPWD);
		$fetchedData = $curlObj->exec();
		$curlObj->close();
		return $this->validateResponseCode($curlObj,$fetchedData);
	}

     /**
	 * Convert XML to required JSON format for the Related Items Grid.
	 *
	 * @param two(String,String)
	 * @returns JSON string
	 * @access public
	 */
    function getRelatedDataObjects($params){
                $identifier=$params['dtoIdentifier'];
                $reference =$params['referenceClassIdentifier'];
                $this->buildWSUri($params);
                $this->dtiXMLData = $this->getCacheData();
                $found =0;      // used to exit from the classObjects loop
				$headerNamesArray=array();
				$headerListDataArray = array();
				$rowsArray=array();
				$gridRowsArray = array();

                 /*
				 * check the dto exists in cache
                 */

            if(($this->dtiXMLData!=false)&&(!empty($this->dtiXMLData))){
			$dxoResponse = $this->getDXOInfo($this->uriParams,$this->dtiXMLData);
			if(($dxoResponse!=false) && ($dxoResponse!='')){

		  $xmlIterator = new SimpleXMLIterator($dxoResponse);
		  $resultArr="";
		  $dataTransferObjects = $xmlIterator->dataTransferObjectList->dataTransferObject;
		  $dtoCounts  = 1;//count($dataTransferObjects);

          /*
          * start processing the xml file from cache
          * marchalling for generate the requiered grid.
          */
          foreach($dataTransferObjects as $dataTransferObject)
          {
             if($found==1)  break; // if identifier of classObjects found then there is no need to traverse further

              // pick the <datatransferobject> that has same $identifier
              if($dataTransferObject->identifier == $identifier && $found==0){


                  $i=0;
                  foreach($dataTransferObject->classObjects->children() as $classObject)
                  {
					  if($found==1)  break; // if identifier of classObjects found then there is no need to traverse further
					  $i++;


					  if($i>1 && $classObject->identifier == $reference){

						  $found=1;

                   // Traverse each templateObjects under the classObject rather than Main
                foreach($classObject->templateObjects->templateObject as $templateObject)
                {

                    // iterate Role objects under each template

                    $tempRoleObjectNameArray = array();

                    foreach($templateObject->roleObjects->children() as $roleObject)
                    {

                 $tempKey='';

                 if(stristr($roleObject->type,'Property'))
                 {
                         $tempRoleObjectNameArray[]="$roleObject->name";
                         $tempKey = (string)$templateObject->name.'.'.(string)$roleObject->name;

                         $spanColor='';
                         switch (strtolower((string)$dataTransferObject->transferType))
                         {
                      case "add":
                              $spanColor='red';
                              break;
                      case "change":
                              $spanColor='blue';
                              break;
                      case "delete":
                              $spanColor='green';
                              break;
                      case "sync":
                              $spanColor='black';
                              break;
                         }

                         // We are adding custom keys to the array
                         if($this->nodeType=='exchanges'){
                      $tempRoleValueArray['TransferType']='<span style="cursor:pointer;color:'.$spanColor.'">'.(string)$dataTransferObject->transferType.'</span>';
                         }

                         // condition to check if the transferType is change for role->type
                         if($dataTransferObject->transferType=='Change')
                         {
                      $value='';
                      $oldvalue='';
                      $convertedvalue='';
                      $convertedoldValue='';

                      if(isset($roleObject->value)){
                              $value=(string)$roleObject->value;
                      }
                      if(isset($roleObject->oldValue)){
                              $oldvalue=(string)$roleObject->oldValue;
                      }

                      // call and store the rdl values by checking the value contains rdl:
                      $convertedvalue = (stristr($value,'rdl:')) ? $this->stroreRdlValues($value,substr($value,4)):$value;
                      $convertedoldValue = (stristr($oldvalue,'rdl:')) ? $this->stroreRdlValues($oldvalue,substr($oldvalue,4)):$oldvalue;

                      // if there is any difference between old and new then represent as old->new

                      if($convertedoldValue!=$convertedvalue){
                           //if($oldvalue!='' && $value!=''){
                              $tempRoleValueArray[$tempKey]='<span style="cursor:pointer;color:'.$spanColor.'">'.$convertedoldValue.'->'.$convertedvalue.'</span>';
                           //}else{
                        //$tempRoleValueArray[$tempKey]='<span style="color:'.$spanColor.'">'.$oldvalue.$value.'</span>';
                           //}

                      }else{
                              $tempRoleValueArray[$tempKey]=$convertedoldValue;
                      }
                         }else{
                      $convertedvalue = (stristr((string)$roleObject->value,'rdl:')) ? $this->stroreRdlValues((string)$roleObject->value,substr((string)$roleObject->value,4)):(string)$roleObject->value;
                      $tempRoleValueArray[$tempKey]='<span style="cursor:pointer;color:'.$spanColor.'">'.$convertedvalue.'</span>';
                         }
                         unset($tempKey);
                 }
                    }

                    // condition to make the Header & Row
                    if(count($tempRoleObjectNameArray)>1){
                 foreach($tempRoleObjectNameArray as $key=>$val){
                         $headerNamesArray[]=(string)$templateObject->name.'.'.$val;
                 }
                    }else if(count($tempRoleObjectNameArray)==1){

                 $headerNamesArray[]=(string)$templateObject->name;

                 /*
                         Check the $tempRoleValueArray that store the value of role with "concanated key"
                         We need to modify that "concanated key" with originalheader
                  */
                 foreach($tempRoleObjectNameArray as $key=>$val){
                         $keyname = (string)$templateObject->name.'.'.$val;
                         if(array_key_exists($keyname,$tempRoleValueArray)){
                      $tempRoleValueArray["$templateObject->name"]=$tempRoleValueArray[$keyname];
                      unset($tempRoleValueArray[$keyname]);
                         }
                 }

                    }
                    unset($tempRoleObjectNameArray);
            }
            $rowsArray[]=$tempRoleValueArray;
            unset($tempRoleValueArray);
            break;
               }
                  }
              }
          }
          
          if($this->nodeType=='exchanges'){
                  $customListArray=array('TransferType');
          }else{
                  $customListArray=array();
          }

          $headerArrayList = array_merge($customListArray,array_values((array_unique($headerNamesArray))));
          
          unset($customListArray);
          unset($headerNamesArray);

          $rowsDataArray = array();
          $columnsDataArray = array();

          /*
           * Loop over each row to genrate the json format required to display rows in grid
          */

          
          for($i=0;$i<count($rowsArray);$i++){
                  for($j=0;$j<count($headerArrayList);$j++){
               $headerName = $headerArrayList[$j];

               if(array_key_exists($headerName,$rowsArray[$i])){
                   $rowsDataArray[$i][]=$rowsArray[$i][$headerName];
				   /* The required format for JSON Reader is underscore supported so we are changing . with _ */
				   $gridRowsArray[$i][str_replace(".", "_", $headerName)]=$rowsArray[$i][$headerName];
               }else{
				   $rowsDataArray[$i][]='';
				   /* The required format for JSON Reader is underscore supported so we are changing . with _ */
				   $gridRowsArray[$i][str_replace(".", "_", $headerName)]='';
														
               }
              }
          }

            /* Store the $gridRowsArray that contains all rows for a dto inside session. This will be used to fetch the records page wise in getPageData() function  */
            if(is_array($rowsDataArray) && !empty($rowsDataArray)){
                    @session_start();
                    //$_SESSION['Page_no']=$gridRowsArray;
                    //echo '<br>key:  '. $this->dtocacheKey;
                    if(isset($_SESSION['rel_'.$this->dtocacheKey.'_'.$identifier.'_'.$reference])){
                    unset($_SESSION['rel_'.$this->dtocacheKey.'_'.$identifier.'_'.$reference]);
                    unset($_SESSION['rel_'.$this->dtocacheKey.'_'.$identifier.'_'.$reference.'_dtoCounts']);
                    }

                    if(!isset($_SESSION['rel_'.$this->dtocacheKey.'_'.$identifier.'_'.$reference])){
                            $_SESSION['rel_'.$this->dtocacheKey.'_'.$identifier.'_'.$reference]=$gridRowsArray;
                            $_SESSION['rel_'.$this->dtocacheKey.'_'.$identifier.'_'.$reference.'_dtoCounts']=$dtoCounts;
                    }
                    unset($gridRowsArray);
            }

          if($this->nodeType=='exchanges'){
          $headerListDataArray[]=array('name'=>'TransferType');
          }

          foreach($headerArrayList as $key =>$val){
                  $headerListDataArray[]=array('name'=>str_replace(".", "_", $val));
                  $columnsDataArray[]=array('id'=>str_replace(".", "_", $val),'header'=>$val,'width'=>(strlen($val)<20)?100:strlen($val)+120,'sortable'=>'true','dataIndex'=>str_replace(".", "_", $val));
          }


          echo json_encode(array("success"=>"true","cacheData"=>$this->cacheDTI,"relatedClasses"=>"","columnsData"=>json_encode($columnsDataArray),"headersList"=>(json_encode($headerListDataArray))));
          unset($jsonrowsArray);
          unset($rowsArray);
          unset($headerArrayList);
          exit;
			}else{
				echo json_encode(array("success"=>"false"));
			}
		}else{
			echo json_encode(array("success"=>"false"));
		}       

        }

        /**
	 * Send the xml request to Server and get the required xml resonse
           * It will call 'getHistoryJSONData()' function to convert xml to JSON
	 *
	 * @param one(Associative Array[nodetype','scope','exchangeID,'exchangeAction'])
	 * @returns JSON string
	 * @access public
	 */
        function getHistory($params){

            // Buid a WebService request URI($this->dtiHistoryUrl)
                $this->buildWSUri($params);

                $this->removeDtiCache();

                // Make the DTI format that need to be post

                // POST the History DTI to Server using CURL and get back the response in xml format

                $curlObj = new curl($this->dtiHistoryUrl);
                $fetchedData = $curlObj->exec();
                $curlObj->close();
                if(!empty($fetchedData)){
                    echo $this->getHistoryJSONData($fetchedData);

                }else{
                     echo json_encode(array("success"=>"false","response"=>"Server not responding"));
                }


            // call getHistoryJSONData() function to get the JSON data

            // return the JSON String
            //$result = 'Done: Test getHistory() &' . $this->getHistoryJSONData($fetchedData);
            //return $result;




        }


         /**
	 * This function used to Marshalling the XML to JSON
          * for exchange history grid.
	 *
	 * @param one(Object)
	 * @returns JSON string
	 * @access public
	 */
        function getHistoryJSONData($fetchedData){

           // Marshalling XML data to JSON
          //echo "<pre>";
            $xmlIterator = new SimpleXMLIterator($fetchedData);
            $resultArr=array();
            $statusListArray = array();
            $statusList = $xmlIterator->statusList;

            $rowDataArr = array();
            //print_r($xmlIterator);
            $i=0;
            $hst_ID='';
            foreach($xmlIterator->response as $response)
            {
                $hst_ID= 'hstID_'.$i;
                $spanColor='';
                 switch ((string)$response->level)
                 {
                      case "Success":
                              $spanColor='green';
                              break;
                      case "Error":
                              $spanColor='red';
                              break;
                      case "Warning":
                              $spanColor='orange';
                              break;
                  }

                $level = '<span title="Click on row to see History Detail" style="cursor:pointer ;color:'.$spanColor.'">'.(string)$response->level.'</span>';
                $rowDataArr[] = array(
                                      $hst_ID,
                                      $level,
                                      (string)$response->startTimeStamp,
                                      $response->statusList->status->count(),
                                      (string)$response->senderUri,
                                      (string)$response->receiverUri);


                foreach($response->statusList->children() as $status)
                {
                    $identifier = (string)$status->identifier;
                    $messagestr='';
                        
                        foreach($status->messages as $message)
                        {
                            $messagestr = (string)$message->message;
                            $statusListArray[]=array($identifier,$messagestr);
                        }
                }

                /* Store the history $statusListArray to SESSION. This will be used to display grid  function  */
                if(is_array($statusListArray) && !empty($statusListArray)){
                    @session_start();
                    //echo '<br>key:  '. $this->historyCacheKey;
                    if(isset($_SESSION[$hst_ID][$this->historyCacheKey])){
                        unset($_SESSION[$hst_ID][$this->historyCacheKey]);
                    }

                    if(!isset($_SESSION[$hst_ID][$this->historyCacheKey])){
                            $_SESSION[$hst_ID][$this->historyCacheKey]=$statusListArray;
                   }
                    unset($statusListArray);
            }


               $i++;
            }
//print_r($statusListArray);
             

            $columnsDataArray = array(
                                    array('id'=>'hstID','header'=>'hstID','dataIndex'=>'hstID','hidden'=>'true'),
                                    array('id'=>'Level','header'=>'Level','dataIndex'=>'Level','sortable'=>'true'),
                                    array('id'=>'startTimeStamp','header'=>'startTimeStamp','sortable'=>'true','dataIndex'=>'startTimeStamp'),
                                    array('id'=>'rowCount','header'=>'rowCount','sortable'=>'true','dataIndex'=>'rowCount'),
                                    array('id'=>'senderUri','header'=>'senderUri','sortable'=>'true','dataIndex'=>'senderUri'),
                                    array('id'=>'receiverUri','header'=>'receiverUri','sortable'=>'true','dataIndex'=>'receiverUri')
                                );
            $headerListDataArray=array(
                                    array('name'=>'hstID'),
                                    array('name'=>'Level'),
                                    array('name'=>'startTimeStamp'),
                                    array('name'=>'rowCount'),
                                    array('name'=>'senderUri'),
                                    array('name'=>'receiverUri')
                );

            if(!empty($rowDataArr)){
                        $rowDataArr = $rowDataArr;
            }else{
                $rowDataArr = array("<font color='green'><b>Error</b></font>","<font color='green'><b>Error</b></font>");
            }           
           // echo "<pre>"; print_r($_SESSION);
            return json_encode(array("success"=>"true",
                                   "headersList"=>(json_encode($headerListDataArray)),
                                   "rowData"=>json_encode($rowDataArr),
                                   "columnsData"=>json_encode($columnsDataArray),
                                   "historyCacheKey"=>$this->historyCacheKey)
                            );
            /*$level = (string)$xmlIterator->level;
            $startTimeStamp = (string)$xmlIterator->startTimeStamp;
            $rowCount= $statusList->status->count();
            $senderUri= (string)$xmlIterator->senderUri;
            $receiverUri=(string)$xmlIterator->receiverUri;


            foreach($statusList->status as $status)
            {
                $identifier = (string)$status->identifier;

                foreach($status->messages as $message)
                {
                        $messagestr = (string)$message->message;
                        $resultArr[$identifier]=$messagestr;
                }

            }

            if(!empty($resultArr)){
                foreach($resultArr as $key =>$val){
                        $rowdataArray[]=array($key,$val);
                 }
            }else{
                $rowdataArray[] = array("<font color='green'><b>Error</b></font>","<font color='green'><b>Error</b></font>");
            }

            $columnsDataArray = array(array('id'=>'Identifier','header'=>'Identifier','dataIndex'=>'Identifier'),array('id'=>'Message','header'=>'Message','sortable'=>'true','width'=>350,'dataIndex'=>'Message'));
            $headerListDataArray=array(array('name'=>'Identifier'),array('name'=>'Message'));
            return json_encode(array("success"=>"true",
                                    "level" => $level,
                                    "startTimeStamp"=> $startTimeStamp,
                                    "rowCount"=> $rowCount,
                                    "senderUri"=> $senderUri,
                                    "receiverUri"=> $receiverUri,
                                   "headersList"=>(json_encode($headerListDataArray)),
                                   "rowData"=>json_encode($rowdataArray),
                                   "columnsData"=>json_encode($columnsDataArray))
                            );

                */
            
        }

         /**
	 * This function used to get the data form SESSION
          * for display status-list grid in history detail pop-up box.
	 *
	 * @param one(string)
	 * @returns JSON string
	 * @access public
	 */
        function getHistoryStatusListData($params){
             @session_start();
            $historyCacheKey = $params['historyCacheKey'];
            $hstID           = $params['hstID'];

            $rowdataArray = $_SESSION[$hstID][$historyCacheKey];
//echo "<pre>"; print_r($_SESSION);
            $columnsDataArray = array(array(
                                            'id'=>'Identifier',
                                            'header'=>'Identifier',
                                            'sortable'=>'true',
                                            'width'=>'300',
                                            'dataIndex'=>'Identifier'
                                      ),array(
                                            'id'=>'Message',
                                            'header'=>'Message',
                                            'sortable'=>'true',
                                            'width'=>'100%',
                                            'dataIndex'=>'Message')
                                 );
            $headerListDataArray=array(array(
                                            'name'=>'Identifier'
                                            ),
                                        array(
                                            'name'=>'Message')
                                );
            
            return json_encode(array("success"=>"true",
                                   "headersList"=>(json_encode($headerListDataArray)),
                                   "rowData"=>json_encode($rowdataArray),
                                   "columnsData"=>json_encode($columnsDataArray))
                            );
        }
}
?>