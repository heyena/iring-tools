USE [iRINGAgent]


DELETE FROM [SCHEDULECACHE]
INSERT INTO [SCHEDULECACHE] ([TASK_NAME],[PROJECT],[APP],[CACHE_PAGE_SIZE],[SSO_URL],[CLIENT_ID],[CLIENT_SECRET],[GRANT_TYPE],[APP_KEY],[ACCESS_TOKEN],[REQUEST_TIMEOUT],[START_TIME],[END_TIME],[Created_Date],[Created_By],[Occurance],[NextStart_Date_Time],[End_Date_Time],[STATUS],[ACTIVE] ) VALUES('99999_000.iw','99999_000','iw',2000,'https://sso.staging.mypsn.com/as/token.oauth2','IW_DataLayer','tmPhR7ZNngdXtsQrmEKhLhi7wa7mHhG0GX3Zn9HvillzCCwf5b/JDMC/a65QkeB6','client_credentials','wHKxvUyEqrLTNSvsVTPX1GJs02nAo5IF','TmMopozebXnR8ky6YgRnAV22ICOz',300000,'08/07/2014 06:00:00','','08/07/2014 07:00:00','Gakhar Hemant','Daily','','10/16/2014 06:00:00','Ready',1)
INSERT INTO [SCHEDULECACHE] ([TASK_NAME],[PROJECT],[APP],[CACHE_PAGE_SIZE],[SSO_URL],[CLIENT_ID],[CLIENT_SECRET],[GRANT_TYPE],[APP_KEY],[ACCESS_TOKEN],[REQUEST_TIMEOUT],[START_TIME],[END_TIME],[Created_Date],[Created_By],[Occurance],[NextStart_Date_Time],[End_Date_Time],[STATUS],[ACTIVE] ) VALUES('55555_000.iw','55555_000','iw',2000,'https://sso.staging.mypsn.com/as/token.oauth2','IW_DataLayer','tmPhR7ZNngdXtsQrmEKhLhi7wa7mHhG0GX3Zn9HvillzCCwf5b/JDMC/a65QkeB6','client_credentials','wHKxvUyEqrLTNSvsVTPX1GJs02nAo5IF','TmMopozebXnR8ky6YgRnAV22ICOz',300000,'08/07/2014 06:00:00','','08/07/2014 06:00:00','Gakhar Hemant','Daily','','09/26/2014 06:00:00','Ready',1)


DELETE FROM [SCHEDULECACHE]
INSERT INTO [SCHEDULECACHE] ([TASK_NAME],[PROJECT],[APP],[CACHE_PAGE_SIZE],[SSO_URL],[CLIENT_ID],[CLIENT_SECRET],[GRANT_TYPE],[APP_KEY],[ACCESS_TOKEN],[REQUEST_TIMEOUT],[START_TIME],[END_TIME],[Created_Date],[Created_By],[Occurance],[NextStart_Date_Time],[End_Date_Time],[STATUS],[ACTIVE] ) VALUES('agent.source','agent','source',2000,'https://sso.mypsn.com/as/token.oauth2','iRingTools','0Lvnvat5T5OJk5n6VwD4optFJoq7/0POq++NfYkIgHYtmy6Pluix3aGy7EAN1Jxp','client_credentials','wHKxvUyEqrLTNSvsVTPX1GJs02nAo5IF','TmMopozebXnR8ky6YgRnAV22ICOz',300000,'08/07/2014 06:00:00','','08/07/2014 07:00:00','Gakhar Hemant','Daily','','10/16/2014 06:00:00','Ready',1)

