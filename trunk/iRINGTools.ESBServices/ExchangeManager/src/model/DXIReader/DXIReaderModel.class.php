<?php
/**
 * @author Aswini Nayak (aknayak@bechtel.com)
*/
include_once('/model/RestfulService/curl.class.php');

class DXIReaderModel{
	private $dxiUrl;

	function __construct(){
		// URL from configfile
		$this->dxiUrl = DXI_REQUEST_URL;
	}

function getDXIInfo($exchangeID){
	if(is_array($exchangeID)){
		$this->dxiUrl = $this->dxiUrl.'?'.http_build_query($exchangeID);
	}
	$curlObj = new curl($this->dxiUrl);
	$fetchedData = $curlObj->exec();
	$xmlIterator = new SimpleXMLIterator($fetchedData);
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
}
}
?>