<?php
/**
 * @author Aswini Nayak (aknayak@bechtel.com)
*/

include_once('model/RestfulService/curl.class.php');

class dataObjectsModel{
	private $exchangeUrl;
	private $dxiUrl;
	private $dxoUrl;
	//private $identifiersList;
	private $dtiXMLData;
	private $nodeType,$uriParams;
	
	function __construct(){
		$this->dxiUrl = DXI_REQUEST_URL;
		$this->dxoUrl = DXO_REQUEST_URL;
	}

	private function buildWSUri($params){
		$this->nodeType=$params['nodetype'];
		$this->uriParams=implode('/',$params);
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
		if($this->dtiXMLData!=''){
		return json_encode($this->createJSONDataFormat($this->getDXOInfo($this->uriParams,$this->dtiXMLData)));
		}else{
			echo json_encode(array("success"=>"false"));
		}
		
	}
	
	
	/**
		@params
		get the Data transfer indexes
		// http://localhost:8080/iringtools/directoryservice/{scope}/exchanges/{exchangeid}/index
		// http://labst9413:8080/iringtools/services/esbsvc/dto/1
	 */

	private function getDtiInfo($params){
		//$this->dxiUrl = $this->dxiUrl.$this->uriParams;
		$this->dxiUrl = $this->dxiUrl;
		$curlObj = new curl($this->dxiUrl);
		$fetchedData = $curlObj->exec();
		return $fetchedData;
	}

	private function getDXOInfo($exchangeID,$postParams){
		//***$this->dxoUrl = $this->dxoUrl.$this->uriParams;
		$curlObj = new curl($this->dxoUrl);
		$curlObj->setopt(CURLOPT_POST, 1);
		$curlObj->setopt(CURLOPT_HTTPHEADER, Array("Content-Type: application/xml"));
		$curlObj->setopt(CURLOPT_POSTFIELDS,$postParams);
		$curlObj->setopt(CURLOPT_HEADER, false);
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
									$tempRoleValueArray['TransferType']='<span style="color:'.$spanColor.'">'.(string)$dataTransferObject->transferType.'</span>';

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

		$customListArray=array('TransferType');
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

		 
		$headerListDataArray[]=array('name'=>'TransferType');
		 foreach($headerArrayList as $key =>$val){
			$headerListDataArray[]=array('name'=>str_replace(".", "_", $val));
			$columnsDataArray[]=array('id'=>str_replace(".", "_", $val),'header'=>$val,'width'=>(strlen($val)<20)?100:strlen($val)+120,'sortable'=>'true','dataIndex'=>str_replace(".", "_", $val));
		}


		/*echo '<pre>';
		print_r($columnsDataArray);
		exit;
		*/
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