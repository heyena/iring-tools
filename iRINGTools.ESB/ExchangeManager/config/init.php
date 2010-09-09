<?php
/** Configuration Variables **/
define('ROOT_DIR', dirname(getcwd()) . '/');
define('MVC_DIR', ROOT_DIR . 'src/');
define('VIEW_DIR', MVC_DIR . 'view/');
define('MODEL_DIR', MVC_DIR . 'model/');
define('CONTROLLER_DIR', MVC_DIR . 'controller/');
define('AJAX_HANDLER_DIR', MVC_DIR . 'AJAXhandlers/');
define('CONFIG_DIR', ROOT_DIR . 'config/');

define('DIRECTORY_REQUEST_URL','http://localhost:81/projects/iRINGTools.ESB/ExchangeManager/directory.xml');
define('DXI_REQUEST_URL', 'http://localhost:81/projects/iRINGTools.ESB/ExchangeManager/dxo.xml');
define('DXO_REQUEST_URL', 'http://localhost:81/projects/iRINGTools.ESB/ExchangeManager/dxi.xml');
?>