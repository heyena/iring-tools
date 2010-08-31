<?php
/**
 * @author Aswini Nayak (aknayak@bechtel.com)
*/

include_once('/model/RestfulService/curl.class.php');
/*
$curlObj = new curl('http://localhost:81/test/PRM/projects.xml');
$fetchedData = $curlObj->exec();
echo $fetchedData;
*/
class ExchangeReaderModel{
	private $exchangeUrl;
	private $projectid;

	function __construct($id){
		$this->projectid=$id;
		$this->exchangeUrl = 'http://localhost:81/test/PRM/testxml.xml';
	}

	function readXML(){
		$curlObj = new curl($this->exchangeUrl);
		$fetchedData = $curlObj->exec();
		print_r($fetchedData);
		
		
	}

	function convertXMLtoJSON(){
		$curlObj = new curl($this->exchangeUrl);
		$fetchedData = $curlObj->exec();
		//echo json_encode($fetchedData);
		$elem = new SimpleXMLElement($fetchedData);
		$i=0;
		foreach($elem as $projects){
			$i++;
			$result[$i]['text']=$projects['description'].'['.$projects['name'].']';
			$result[$i]['cls']=	'folder';

			// if there is some application elements exists then create the children node
			if($projects->dataExchanges->commodity->count()>0)
			{
				// get the commoditity details
				if(isset($commodityNode)) unset($commodityNode);

				foreach($projects->dataExchanges->children() as $commodity)
				{
					if(isset($exchangeNodes)) unset($exchangeNodes);
					foreach($commodity->children() as $exchng){
						$exchangeNodes[]=array('text'=>(string)$exchng['name'],'id'=>(string)$exchng['id'],'icon'=>'resources/images/icons/graph.png','leaf'=>'true');
					}
					$commodityNode[]= array('text'=>(string)$commodity['name'],'children'=>$exchangeNodes);
				}

			}
			if($projects->applicationData->application->count()>0)
			{
				$j=-1;
				foreach($projects->applicationData->children() as $applicationArray)
				{
					if(isset($graphNodes)) unset($graphNodes);
					foreach($applicationArray->children() as $graphs)
					{
						$graphNodes[] = array('text'=>(string)$graphs['name'],'commodity'=>(string)$graphs['commodity'],'icon'=>'resources/images/icons/graph.png','leaf'=>'true');
					}
					$appNodes[]= array('text'=>(string)$applicationArray['name'],'desc'=>(string)$applicationArray['description'],'children'=>$graphNodes);
				}
				$result[$i]['children']=array(array('text'=>'Application Data','icon'=>'resources/images/iringlogo.png','children'=>$appNodes),$commodityNode);
			}

			unset($appNodes);
			unset($graphNodes);
		}
		echo json_encode($result);
	}
	
}
?>