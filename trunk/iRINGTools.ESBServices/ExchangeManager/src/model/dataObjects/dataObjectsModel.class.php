<?php
/**
 * @author Aswini Nayak (aknayak@bechtel.com)
*/
include_once('/model/RestfulService/curl.class.php');

class dataObjectsModel{
	private $exchangeUrl;
	private $dxiUrl;
	private $dxoUrl;
	private $identifiersList;
	
	function __construct(){
		$this->dxiUrl = DXI_REQUEST_URL;
		$this->dxoUrl = DXO_REQUEST_URL;
	}

	function getReviewDatas($params)
	{
	 /**
	  * Will use the DXIObject to get the identifiers List with the particular exchangeID
	 */

		$this->identifiersList = $this->getDXIInfo($params); // Tag-1,Tag-2,Tag-3
		$totalTagCount = count(explode(',',$this->identifiersList));
		$perPageTagCount=5; // configurable
		$totalPages = ceil($totalTagCount/$perPageTagCount);
		$this->getDXOInfo($params,$this->identifiersList);
	}

	/*
	 * @params : exchangeID
	 */

	function getDataObjects($params)
	{
	 /**
	  * Will use the DXIObject to get the identifiers List with the particular exchangeID
	 */
		$this->identifiersList = $this->getDXIInfo($params); // Tag-1,Tag-2,Tag-3
		$identifiersArray = explode(',',$this->identifiersList);
		return json_encode($this->createJSONDataFormat($this->getDXOInfo($params,$this->identifiersList)));
	}
	
	
	/**
		@params
		get the Data transfer indexes
		// http://localhost:8080/iringtools/directoryservice/{scope}/exchanges/{exchangeid}/index
	 */

	private function getDXIInfo($exchangeID){
		
		if(is_array($exchangeID)){
			$this->dxiUrl = $this->dxiUrl.'?'.http_build_query($exchangeID);
		}
		$curlObj = new curl($this->dxiUrl);
		$fetchedData = $curlObj->exec();
		$xmlIterator = new SimpleXMLIterator($fetchedData);
		$resultArr="";
		
		foreach($xmlIterator  as $dataTransferIndices)
		{
			if($resultArr==''){
				$resultArr=$dataTransferIndices->identifier;
			}else{
				$resultArr=$resultArr.','.$dataTransferIndices->identifier;
			}
		}
		unset($fetchedData);
		return $resultArr;
	}

	private function getDXOInfo($exchangeID,$postParams){
		if(is_array($exchangeID)){
			$this->dxoUrl = $this->dxoUrl.'?'.http_build_query($exchangeID);
		}
		$curlObj = new curl($this->dxoUrl);
		$curlObj->setopt(CURLOPT_POST, 1);
		$curlObj->setopt(CURLOPT_POSTFIELDS,'TagLists='.$postParams);
		$fetchedData = $curlObj->exec();
		return $fetchedData;
	}

	private function createJSONDataFormat($fetchedData){
		$xmlIterator = new SimpleXMLIterator($fetchedData);
		$resultArr="";
		$dataTransferObjects = $xmlIterator->dataTransferObject;

		// Total no of datatransferobject elements found
		$dtoCounts  = count($dataTransferObjects);

		$headerNamesArray=array();
		$rowsArray=array();

		$j=0;
		foreach($dataTransferObjects as $dataTransferObject)
		{
			//echo "<br><br>Looped for: ".$j++.'<br><br>';
			
			$i=0;

			foreach($dataTransferObject->classObjects->children() as $classObject)
			{
				$i++;

				// Main class object
				if($i==1)
				{
					// Traverse each templateObjects under the Main classObject
					foreach($classObject->templateObjects->templateObject as $templateObject)
					{
			
						// iterate Role objects under each template
						
						$tempRoleObjectNameArray = array();
						
						foreach($templateObject->roleObjects->children() as $roleObject)
						{
								$tempKey='';
								if($roleObject->type=='Property')
								{
									$tempRoleObjectNameArray[]="$roleObject->name";
									$tempKey = (string)$templateObject->name.'.'.(string)$roleObject->name;

									// condition to check if the transferType is change for role->type
									
									if($dataTransferObject->transferType=='Change')
									{
										// if there is any difference between old and new then represent as old->new
										if((string)$roleObject->value!=(string)$roleObject->oldValue){
											$tempRoleValueArray[$tempKey]=(string)$roleObject->oldValue.'->'.$roleObject->value;
										}else{
											$tempRoleValueArray[$tempKey]=(string)$roleObject->oldValue;

										}
									}else{
										$tempRoleValueArray[$tempKey]=(string)$roleObject->value;

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
				}else
				{
					// Skip the indirect classObject 

				}
			}

			$rowsArray[]=$tempRoleValueArray;
			unset($tempRoleValueArray);
		}

		/*echo '<b><h1>Rows Array</h1></b>';
		echo '<pre>';
		print_r($rowsArray);
		*/
		//echo '<b><h1>Header Array</h1></b>';
		//echo '<pre>';
		
		$headerArrayList = array_values((array_unique($headerNamesArray)));
		unset($headerNamesArray);
		
		$rowsDataArray = array();
		$columnsDataArray = array();

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

		 $headerListDataArray = array();

		 foreach($headerArrayList as $key =>$val){
			$headerListDataArray[]=array('name'=>str_replace(".", "_", $val));
			$columnsDataArray[]=array('header'=>$val,'dataIndex'=>str_replace(".", "_", $val));
			
		}

		echo json_encode(array("success"=>"true","rowData"=>json_encode($rowsDataArray),
							   "columnsData"=>json_encode($columnsDataArray),
							   "headersList"=>(json_encode($headerListDataArray))));
		unset($jsonrowsArray);
		unset($rowsArray);
		unset($headerArrayList);
		exit;
		
	}

}
?>