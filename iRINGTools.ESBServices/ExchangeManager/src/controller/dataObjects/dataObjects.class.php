<?php
   require_once('/controller/BaseController.php');   
/** Controller with capabilities :
	to generate the tree for Review & Acceptance
	to send the data of Review & Acceptance for Exchange
	to get the status
	Will use DXI & DXO Controller
	Function getReview() @params exchangeID
    @returns the JSON for Tree view + default grid view + array to be used in nodes
*/

//*****123


  // *****a1239

   class dataObjects extends BaseController{
	private $defaultGrid = array();
	private $treeData;
	private $keyparams=array('scope','exchangeID');
	protected $modelObj;

	/**
	instantiate the model object 
	*/
	function __construct(){
		$this->modelObj = $this->useModel(get_class($this));
	}

	// This function will be called in AJAX Request like
	// http://localhost:81/iRINGTools.ESB/ExchangeManager/ReviewGenerator/getReview/1
	
	function getReview($params){
		$this->modelObj->getReviewDatas(array_combine($this->keyparams,$params));
	}

	function getDataObjects($params){
		$headerArray = $this->modelObj->getDataObjects(array_combine($this->keyparams,$params));

		echo ($headerArray);
		
	}

	function setReview($params){
	}
}
   
?>