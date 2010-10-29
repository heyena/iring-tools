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
	private $nodeType,$uriParams,$cacheKey,$cacheDTI;
	private $rdlArray=array();
	function __construct(){
	}

	private function buildWSUri($params){
           
		$this->nodeType=$params['nodetype'];
		switch($this->nodeType){
			case "exchanges":
				if(isset($params['hasreviewed'])){
					$append ='/submit';
					$this->dtiSubmitUrl = DXI_REQUEST_URL.'/'.$params['scope'].'/'.$params['nodetype'].'/'.$params['exchangeID'].$append;
					//$this->dtiSubmitUrl = APP_REQUEST_URI;
					
				}
				$this->dtiUrl = DXI_REQUEST_URL.'/'.$params['scope'].'/'.$params['nodetype'].'/'.$params['exchangeID'];
				$this->dtoUrl = DXO_REQUEST_URL.'/'.$params['scope'].'/'.$params['nodetype'].'/'.$params['exchangeID'];
				$this->cacheKey = $params['scope'].'_'.$params['nodetype'].'_'.$params['exchangeID'];
				break;

			case "graph":
				$this->dtiUrl = APP_REQUEST_URI.'/'.$params['scope'].'/'.$params['applname'].'/'.$params['graphs'];
				$this->dtoUrl = APP_REQUEST_URI.'/'.$params['scope'].'/'.$params['applname'].'/'.$params['graphs'].'/page';
				$this->cacheKey = $params['scope'].'_'.$params['applname'].'_'.$params['graphs'];
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
				$replaceString = str_replace('<dataTransferIndices xmlns="http://iringtools.org/adapter/dti">','<xr:exchangeRequest xmlns="http://iringtools.org/adapter/dti" xmlns:xr="http://iringtools.org/common/request"><xr:dataTransferIndices>',$this->dtiXMLData);
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
	 * @params : exchangeID
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

	private function createJSONDataFormat($fetchedData){
		$xmlIterator = new SimpleXMLIterator($fetchedData);
		$resultArr="";
		$dataTransferObjects = $xmlIterator->dataTransferObject;

		// Total no of datatransferobject elements found
		$dtoCounts  = count($dataTransferObjects);

		$headerNamesArray=array();
		$headerListDataArray = array();
		$rowsArray=array();
		$classReferenceArray=array();
                $classObjectName='';

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
                                    $returnArray = $this -> classObjectsTraverse($dataTransferObject,$classObject,$mainClassObject);
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
		}else{
			$customListArray=array();
		}

		$headerArrayList = array_merge($customListArray,array_values((array_unique($headerNamesArray))));
		
		unset($customListArray);
		unset($headerNamesArray);

		$rowsDataArray = array();
		$columnsDataArray = array();

		/*
		 Loop over each row to genrate the json format required to display rows in grid
		*/

		
		for($i=0;$i<count($rowsArray);$i++){

			for($j=0;$j<count($headerArrayList);$j++){
				$headerName = $headerArrayList[$j];

				if(array_key_exists($headerName,$rowsArray[$i])){
					$rowsDataArray[$i][]=$rowsArray[$i][$headerName];
				}else
				{
					$rowsDataArray[$i][]='';
				}
			}
		}


		if($this->nodeType=='exchanges'){
		$headerListDataArray[]=array('name'=>'Identifier','name'=>'TransferType');
		}
		
		foreach($headerArrayList as $key =>$val){
			$headerListDataArray[]=array('name'=>str_replace(".", "_", $val));
			$columnsDataArray[]=array('id'=>str_replace(".", "_", $val),'header'=>$val,'width'=>(strlen($val)<20)?100:strlen($val)+120,'sortable'=>'true','dataIndex'=>str_replace(".", "_", $val));
		}

                //$relatedClasses = array('Listing'=>$classReferenceArray);
		echo json_encode(array("classObjName"=>$classObjectName,'relatedClasses'=>$classReferenceArray,"success"=>"true","cacheData"=>$this->cacheDTI,"rowData"=>json_encode($rowsDataArray),"columnsData"=>json_encode($columnsDataArray),"headersList"=>(json_encode($headerListDataArray))));
		unset($jsonrowsArray);
		unset($rowsArray);
		unset($headerArrayList);
		exit;

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

                /*
		* check the dto exists in cache
                */

                if(($this->dtiXMLData!=false)&&(!empty($this->dtiXMLData))){
			$dxoResponse = $this->getDXOInfo($this->uriParams,$this->dtiXMLData);
			if(($dxoResponse!=false) && ($dxoResponse!='')){
				
                            $xmlIterator = new SimpleXMLIterator($dxoResponse);
                            $resultArr="";
                            $dataTransferObjects = $xmlIterator->dataTransferObject;

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


                                                if($i>1 && $classObject->identifier == $reference)
                                                {

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

                                        //$rowsArray[]=$tempRoleValueArray;
                                        //unset($tempRoleValueArray);

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
                                                }else
                                                {
                                                        $rowsDataArray[$i][]='';
                                                }
                                        }
                                }


                                if($this->nodeType=='exchanges'){
                                $headerListDataArray[]=array('name'=>'TransferType');
                                }

                                foreach($headerArrayList as $key =>$val){
                                        $headerListDataArray[]=array('name'=>str_replace(".", "_", $val));
                                        $columnsDataArray[]=array('id'=>str_replace(".", "_", $val),'header'=>$val,'width'=>(strlen($val)<20)?100:strlen($val)+120,'sortable'=>'true','dataIndex'=>str_replace(".", "_", $val));
                                }


                                echo json_encode(array("success"=>"true","cacheData"=>$this->cacheDTI,"relatedClasses"=>"","rowData"=>json_encode($rowsDataArray),"columnsData"=>json_encode($columnsDataArray),"headersList"=>(json_encode($headerListDataArray))));
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
                                     $value = '<div class="x-panel-header x-accordion-hd" style="cursor:pointer"><a href="#" style="cursor:pointer;text-decoration:none" onClick="displayRleatedClassGrid(\''.$refClassIdentifier.'\',\''.$dtoIdentifier.'\',\''.$relatedClassName.'\')">'.$relatedClassName.'</a></div>';
                                     if(isset($tempClassReferenceArray[$key])){
                                        $tempClassReferenceArray[$key] = ''.$value.''.$tempClassReferenceArray[$key];
                                     }
                                     else{
                                             $tempClassReferenceArray[$key]=$value;
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
				$classReferenceLabelArray=array("identifier"=>(string)$dataTransferObject->identifier,"text"=>$tempClassReferenceArray["'".$dataTransferObject->identifier."'"]);
			}else{
				$classReferenceLabelArray=array("identifier"=>(string)$dataTransferObject->identifier,"text"=>'');
			}
			$returnArray=array('headerNamesArray'=>$headerNamesArray,'tempRoleValueArray'=>$tempRoleValueArray,'classReferenceArray' =>$classReferenceLabelArray);

            return $returnArray;
        }
}
?>