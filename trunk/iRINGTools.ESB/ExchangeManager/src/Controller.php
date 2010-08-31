<?php
   /**
    * @author Aswini Nayak (aknayak@bechtel.com)
	*/
// check the class Name
// create the instance of that class
// call the method with parameters
// include('');
// echo '<pre>';
// print_r($_GET);

if(isset($_GET['cnmae'])&&(!empty($_GET['cnmae'])))
{
   $controllerName=$_GET['cnmae'];
   $controllerPath='controller/'.$controllerName.'/'.$controllerName.'.class.php';
}
require_once($controllerPath);
$obj = new $controllerName($_GET['mparameter']);
$mname = $_GET['mname'];
$obj->$mname($_GET['mparameter']);

?>