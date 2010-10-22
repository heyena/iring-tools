<?php
/** Configuration Variables **/
define('ROOT_DIR', dirname(getcwd()) . '/');
define('MVC_DIR', ROOT_DIR . 'src/');
define('VIEW_DIR', MVC_DIR . 'view/');
define('MODEL_DIR', MVC_DIR . 'model/');
define('CONTROLLER_DIR', MVC_DIR . 'controller/');
define('AJAX_HANDLER_DIR', MVC_DIR . 'AJAXhandlers/');
define('CONFIG_DIR', ROOT_DIR . 'config/');

define('DXI_REQUEST_URL', 'http://labst9413:8080/iringtools/services/esb');
define('DXO_REQUEST_URL', 'http://labst9413:8080/iringtools/services/esb');
define('APP_REQUEST_URI','http://labst9413/Services/dto');
define('DIRECTORY_REQUEST_URL','http://labst9413:8080/iringtools/services/esb/directory');
define('RDL_REQUEST_URI','http://labst9413/Services/RefDataService/Classes');

?>