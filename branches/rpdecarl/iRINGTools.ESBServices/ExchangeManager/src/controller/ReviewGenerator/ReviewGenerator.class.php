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

   class ReviewGenerator extends BaseController{

	private $defaultGrid = array();
	private $treeData;
	private $keyparams=array('exchangeID');
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
		echo $this->modelObj->getReviewDatas(array_combine($this->keyparams,$params));
	}

	function setReview($params){
	}
}
   
?>