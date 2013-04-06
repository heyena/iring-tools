﻿USE [ABC]

--SELECT 'INSERT INTO [EQUIPMENT] VALUES('''+ISNULL([TAG],'')+''','''+ISNULL([INTERNAL_TAG],'')+''','''+ISNULL([ID],'')+''','''+ISNULL([AREA],'')+''','''+ISNULL([TRAINNUMBER],'')+''','''+ISNULL([EQTYPE],'')+''','''+ISNULL([EQPPREFIX],'')+''','''+ISNULL([EQSEQNO],'')+''','''+ISNULL([EQPSUFF],'')+''','''+ISNULL([EQUIPDESC1],'')+''','''+ISNULL([EQUIPDESC2],'')+''','''+ISNULL([CONSTTYPE],'')+''','''+ISNULL([EWP],'')+''','''+ISNULL([USER1],'')+''','''+ISNULL([USER2],'')+''','''+ISNULL([USER3],'')+''','''+ISNULL([TAGSTATUS],'')+''','''+ISNULL([COMMODITY],'')+''')' FROM [EQUIPMENT]
DELETE FROM [EQUIPMENT]
INSERT INTO [EQUIPMENT] VALUES('C-9006','90-C-06','0100000009','90','','C','','06','','','','NEW','***','','','','NEW','MECH')
INSERT INTO [EQUIPMENT] VALUES('C-9007','90-C-07','0100000006','90','','C','','07','','','','NEW','***','','','','NEW','MECH')
INSERT INTO [EQUIPMENT] VALUES('E-90002A','90-E-002A','0100000010','90','','E','','002','A','DEETHANISER','','NEW','***','','','','NEW','MECH')
INSERT INTO [EQUIPMENT] VALUES('E-9003','90-E-03','0100000005','90','','E','','03','','PROPANE DRYER','','NEW','***','','','','NEW','MECH')
INSERT INTO [EQUIPMENT] VALUES('R-9003','90-R-03','0100000008','90','','R','','03','','SPLITTER REBOILER','','NEW','***','','','','NEW','MECH')
INSERT INTO [EQUIPMENT] VALUES('R-9004','90-R-04','0100000007','90','','R','','04','','PROPANE DRYER','','NEW','***','','','','NEW','MECH')

--SELECT 'INSERT INTO [INSTRUMENTS] VALUES('''+ISNULL([KEYTAG],'')+''','''+ISNULL([TAG],'')+''','''+ISNULL([TAG_NO],'')+''','''+ISNULL([TAG_CODE],'')+''','''+ISNULL([ASSOC_EQ],'')+''','''+ISNULL([IAREA],'')+''','''+ISNULL([ITRAIN],'')+''','''+ISNULL([ITYP],'')+''','''+ISNULL([INUM],'')+''','''+ISNULL([ISUFFIX],'')+''','''+ISNULL([MODIFIER1],'')+''','''+ISNULL([MODIFIER2],'')+''','''+ISNULL([MODIFIER3],'')+''','''+ISNULL([MODIFIER4],'')+''','''+ISNULL([STD_DETAIL],'')+''','''+ISNULL([DESCRIPT],'')+''','''+ISNULL([TAG_TYPE],'')+''','''+ISNULL([CONST_TYPE],'')+''','''+ISNULL([COMP_ID],'')+''','''+ISNULL([PROJ_STAT],'')+''','''+ISNULL([PID_NO],'')+''','''+ISNULL([LINE_NO],'')+''')' FROM [INSTRUMENTS]
DELETE FROM [INSTRUMENTS]
INSERT INTO [INSTRUMENTS] VALUES('0100000098','90-FC-004A','090-FC-004A','A-T-N''U','','090','','FC','004','A','','','','','','','AT_INST_DCS','NEW','560257021_17981','REN','90-AO5676','66015-O')
INSERT INTO [INSTRUMENTS] VALUES('0100000264','90-FC-004B','090-FC-004B','A-T-N''U','','090','','FC','004','B','','','','','','','AT_INST_DCS','NEW','560257021_17999','NEW','90-AO5676','66015-O')
INSERT INTO [INSTRUMENTS] VALUES('0100000090','90-FC-007','090-FC-007','A-T-N','','090','','FC','007','','','','','','','','AT_INST_DCS','NEW','559983967_24310','NEW','90-AO5676','66015-O')
INSERT INTO [INSTRUMENTS] VALUES('0100000092','90-FD-007','090-FD-007','A-T-N','','090','','FD','007','','','','','','','','AT_INST_','NEW','559983967_19100','NEW','90-AO5676','66015-O')
INSERT INTO [INSTRUMENTS] VALUES('0100000080','90-FE-006','090-FE-006','A-T-N','','090','','FE','006','','','','','','','','AT_INST_','NEW','559983967_19933','NEW','90-AO5676','66015-O')
INSERT INTO [INSTRUMENTS] VALUES('0100000114','90-FI-008','090-FI-008','A-T-N','','090','','FI','008','','','','','','','','AT_INST_DCS','NEW','560257021_19118','NEW','90-AO5676','66015-O')
INSERT INTO [INSTRUMENTS] VALUES('0100000082','90-FIC-006','090-FIC-006','A-T-N','','090','','FIC','006','','','','','','','','AT_INST_DCS','NEW','559983967_17981','NEW','90-AO5676','90002-RV')

--SELECT 'INSERT INTO [LINES] VALUES ('''+ISNULL([TAG],'')+''','''+ISNULL([ID],'')+''','''+ISNULL([AREA],'')+''','''+ISNULL([TRAINNUMBER],'')+''','''+ISNULL([SPEC],'')+''','''+ISNULL([SYSTEM],'')+''','''+ISNULL([LINENO],'')+''','''+ISNULL([NOMDIAMETER],'')+''','''+ISNULL([INSULATIONTYPE],'')+''','''+ISNULL([HTRACED],'')+''','''+ISNULL([CONSTTYPE],'')+''','''+ISNULL([DESPRESSURE],'')+''','''+ISNULL([TESTPRESSURE],'')+''','''+ISNULL([PWHT],'')+''','''+ISNULL([TESTMEDIA],'')+''','''+ISNULL([MATLTYPE],'')+''','''+ISNULL([NDT],'')+''','''+ISNULL([NDE],'')+''','''+ISNULL([PIPECLASS],'')+''','''+ISNULL([PIDNUMBER],'')+''','''+ISNULL([DESTEMPERATURE],'')+''','''+ISNULL([PAINTSYSTEM],'')+''','''+ISNULL([DESIGNCODE],'')+''','''+ISNULL([COLOURCODE],'')+''','''+ISNULL([EWP],'')+''','''+ISNULL([USER1],'')+''','''+ISNULL([TAGSTATUS],'')+''','''+ISNULL([FULLLINE],'')+''','''+ISNULL([UOM_NOMDIAMETER],'')+''','''+ISNULL([UOM_DESPRESSURE],'')+''','''+ISNULL([UOM_DESTEMPERATURE],'')+''')' FROM [LINES]
DELETE FROM [LINES]
INSERT INTO [LINES] VALUES ('90009-O','0100000041','90','','AAB3','O','009',150,'IP','','NEW','5.2','','','','','','','','','163','','','','***','','NEW','90-O-009','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90010-O','0100000039','90','','AAB3','O','010',80,'IH','','NEW','5.2','','','','','','','','90-AO5676','163','','','','***','','NEW','90-O-010','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90011-O','0100000043','90','','AAB3','O','011',80,'IP','','NEW','5.2','','','','','','','','90-AO5676','150','','','','***','','NEW','90-O-011','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90012-O','0100000031','90','','AAB3','O','012',100,'IP','','NEW','5.2','','','','','','','','90-AO5676','163','','','','***','','NEW','90-O-012','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90013-O','0100000037','90','','AAB3','O','013',100,'N','','NEW','5.2','','','','','','','','90-AO5676','163','','','','***','','NEW','90-O-013','mm','kPag','Deg C')

--SELECT 'INSERT INTO [VALVES] VALUES ('''+ISNULL([KEYTAG],'')+''','''+ISNULL([TAG_NO],'')+''','''+ISNULL([VAREA],'')+''','''+ISNULL([VTYP],'')+''','''+ISNULL([VTRAIN],'')+''','''+ISNULL([VNUM],'')+''','''+ISNULL([VSUFFIX],'')+''','''+ISNULL([TAG_TYPE],'')+''','''+ISNULL([CONST_TYPE],'')+''','''+ISNULL([COMP_ID],'')+''','''+ISNULL([VSIZE],'')+''','''+ISNULL([UOM_VSIZE],'')+''','''+ISNULL([VSPEC_TYPE],'')+''','''+ISNULL([VSPEC_NUM],'')+''','''+ISNULL([VPRESRATE],'')+''','''+ISNULL([VCONDITION],'')+''','''+ISNULL([PID_NO],'')+''','''+ISNULL([PROJ_STAT],'')+''')' FROM [VALVES]
DELETE FROM [VALVES]
INSERT INTO [VALVES] VALUES ('0100000116','90-HV-801','90','HV','','801','','AT_HVALVE','NEW','559983967_20214','80','mm','VBA','001','','','90-AO5678','NEW')
INSERT INTO [VALVES] VALUES ('0100000117','90-HV-802','90','HV','','802','','AT_HVALVE','NEW','559983967_20223','80','mm','VBA','101','','','90-AO5678','NEW')
INSERT INTO [VALVES] VALUES ('0100000118','90-HV-803','90','HV','','803','','AT_HVALVE','NEW','559983967_24079','50','mm','VGA','001','','','90-AO5678','NEW')