<?php
/**
 * @author Aswini Nayak (aknayak@bechtel.com)
*/
include_once('/model/RestfulService/curl.class.php');

class DXOReaderModel{
	private $dxoUrl;
	function __construct(){
		$this->dxoUrl = DXO_REQUEST_URL;
	}

function getDXOInfo($exchangeID,$postParams){
	if(is_array($exchangeID)){
		$this->dxoUrl = $this->dxoUrl.'?'.http_build_query($exchangeID);
		// http://localhost:81/iRINGTools.ESB/ExchangeManager/src/dto.xml?exchangeID=1
	}

	$curlObj = new curl($this->dxoUrl);
	$curlObj->setopt(CURLOPT_POST, 1);
	$curlObj->setopt(CURLOPT_POSTFIELDS,'TagLists='.$postParams);
	$fetchedData = $curlObj->exec();

	print_r($fetchedData);
	
	
	/*echo '<pre>';
	var_dump($curlObj);
	echo '</pre>';
	*/
	/*$xmlIterator = new SimpleXMLIterator($fetchedData);
	$resultArr="" ;
	foreach($xmlIterator  as $dataTransferIndices)
	{
		if($resultArr==''){
		$resultArr=$dataTransferIndices->identifier;
		}else{
		$resultArr=$resultArr.','.$dataTransferIndices->identifier;
		}
	}
	return $resultArr;
	*/
}
}
?>
