<?php

function __autoload($className){

	require_once('./model/'.substr($className, 0, -5).'/'.$className.'.class.php');
		
}

class ExchangeModel
{
	static function useMod($className){
		$modelName = $className.'Model';
		return new $modelName();
	}
}
?>