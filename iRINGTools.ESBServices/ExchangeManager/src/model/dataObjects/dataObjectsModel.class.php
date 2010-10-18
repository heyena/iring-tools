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

	function __construct(){
	}

	private function buildWSUri($params){
		$this->nodeType=$params['nodetype'];
		switch($this->nodeType){
			case "exchanges":
				$this->dtiUrl = DXI_REQUEST_URL.'/'.$params['scope'].'/'.$params['nodetype'].'/'.$params['exchangeID'];
				$this->dtoUrl = DXO_REQUEST_URL.'/'.$params['scope'].'/'.$params['nodetype'].'/'.$params['exchangeID'];
				break;

			case "graph":
				$this->dtiUrl = APP_REQUEST_URI.'/'.$params['scope'].'/'.$params['applname'].'/'.$params['graphs'];
				$this->dtoUrl = APP_REQUEST_URI.'/'.$params['scope'].'/'.$params['applname'].'/'.$params['graphs'].'/page';
				break;
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
		$this->buildWSUri($params);
		$this->dtiXMLData = $this->getDtiInfo($params);

		if(($this->dtiXMLData!=false)&&($this->dtiXMLData!='')){
			
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
		$curlObj->setopt(CURLOPT_USERPWD,"aknayak:povuxitu722S!");
		$fetchedData = $curlObj->exec();
		$curlObj->close();
		return $this->validateResponseCode($curlObj,$fetchedData);

		/*// check the response code for curl
		if($this->validateResponseCode($curlObj)){
			return $fetchedData;
		}else{
			return false;
		}*/
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
		$curlObj->setopt(CURLOPT_USERPWD,"aknayak:povuxitu722S!");
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
					// Traverse each templateObjects under the Main classObject
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
									$tempRoleValueArray['TransferType']='<span style="color:'.$spanColor.'">'.(string)$dataTransferObject->transferType.'</span>';
								}
								
									// condition to check if the transferType is change for role->type
								if($dataTransferObject->transferType=='Change')
								{
									$value='';
									$oldvalue='';

									if(isset($roleObject->value)){
										$value=(string)$roleObject->value;
									}
									if(isset($roleObject->oldValue)){
										$oldvalue=(string)$roleObject->oldValue;
									}
										// if there is any difference between old and new then represent as old->new

									if($oldvalue!=$value){

											//if($oldvalue!='' && $value!=''){
										$tempRoleValueArray[$tempKey]='<span style="color:'.$spanColor.'">'.$oldvalue.'->'.$value.'</span>';
											//}else{
												//$tempRoleValueArray[$tempKey]='<span style="color:'.$spanColor.'">'.$oldvalue.$value.'</span>';
											//}

									}
									else{
										$tempRoleValueArray[$tempKey]=(string)$roleObject->oldValue;
									}
								}else{
									$tempRoleValueArray[$tempKey]='<span style="color:'.$spanColor.'">'.(string)$roleObject->value.'</span>';

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
		exit;
		*/
		//echo '<b><h1>Header Array</h1></b>';
		//echo '<pre>';

		if($this->nodeType=='exchanges'){
			$customListArray=array('TransferType');
		}else{
			$customListArray=array();
		}
		
		$headerArrayList = array_merge($customListArray,array_values((array_unique($headerNamesArray))));
		//$headerArrayList = array_values((array_unique($headerNamesArray)));
		unset($customListArray);
		unset($headerNamesArray);

		$rowsDataArray = array();
		$columnsDataArray = array();

		/*
		 Loop over each row to genrate the json format required to display rows in grid
		*/

		/*echo '<pre>';
		print_r($rowsArray);
		print_r($headerArrayList);
		exit;*/

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


		/*echo '<pre>';
		print_r($columnsDataArray);
		exit;
		*/

		echo json_encode(array("success"=>"true","classObjName"=>$classObjectName,"rowData"=>json_encode($rowsDataArray),
							   "columnsData"=>json_encode($columnsDataArray),
							   "headersList"=>(json_encode($headerListDataArray))));
		unset($jsonrowsArray);
		unset($rowsArray);
		unset($headerArrayList);
		exit;

	}
}
?>