<?php
/**
 * @author Aswini Nayak (aknayak@bechtel.com)
*/
include_once('/model/RestfulService/curl.class.php');
include_once('/model/ExchangeModel.class.php');

class ReviewGeneratorModel extends ExchangeModel{
	private $exchangeUrl;
	private $dxiObject;
	private $dxoObject;
	private $identifiersList;

	function __construct(){
		$this->dxiObject=ExchangeModel::useMod('DXIReader');
		$this->dxoObject=ExchangeModel::useMod('DXOReader');
	}

	function getReviewDatas($params)
	{
	 /**
	  * Will use the DXIObject to get the identifiers List with the particular exchangeID
	 */
		$this->identifiersList = $this->dxiObject->getDXIInfo($params); // Tag-1,Tag-2,Tag-3
		$totalTagCount = count(explode(',',$this->identifiersList));
		$perPageTagCount=5; // configurable
		$totalPages = ceil($totalTagCount/$perPageTagCount);
		$this->dxoObject->getDXOInfo($params,$this->identifiersList);
		
	}
}
?>