USE [ABC]

--SELECT 'INSERT INTO [EQUIPMENT] ('''+ISNULL([TAG],'')+''','''+ISNULL([INTERNAL_TAG],'')+''','''+ISNULL([ID],'')+''','''+ISNULL([AREA],'')+''','''+ISNULL([TRAINNUMBER],'')+''','''+ISNULL([EQTYPE],'')+''','''+ISNULL([EQPPREFIX],'')+''','''+ISNULL([EQSEQNO],'')+''','''+ISNULL([EQPSUFF],'')+''','''+ISNULL([EQUIPDESC1],'')+''','''+ISNULL([EQUIPDESC2],'')+''','''+ISNULL([CONSTTYPE],'')+''','''+ISNULL([EWP],'')+''','''+ISNULL([USER1],'')+''','''+ISNULL([USER2],'')+''','''+ISNULL([USER3],'')+''','''+ISNULL([TAGSTATUS],'')+''','''+ISNULL([COMMODITY],'')+''')' FROM [EQUIPMENT]
DELETE FROM [EQUIPMENT]
INSERT INTO [EQUIPMENT] ('C-9006','90-C-06','0100000009','90','','C','','06','','','','NEW','***','','','','NEW','MECH')
INSERT INTO [EQUIPMENT] ('C-9007','90-C-07','0100000006','90','','C','','07','','','','NEW','***','','','','NEW','MECH')
INSERT INTO [EQUIPMENT] ('E-90002A','90-E-002A','0100000010','90','','E','','002','A','DEETHANISER','','NEW','***','','','','NEW','MECH')
INSERT INTO [EQUIPMENT] ('E-9003','90-E-03','0100000005','90','','E','','03','','PROPANE DRYER','','NEW','***','','','','NEW','MECH')
INSERT INTO [EQUIPMENT] ('R-9003','90-R-03','0100000008','90','','R','','03','','SPLITTER REBOILER','','NEW','***','','','','NEW','MECH')
INSERT INTO [EQUIPMENT] ('R-9004','90-R-04','0100000007','90','','R','','04','','PROPANE DRYER','','NEW','***','','','','NEW','MECH')

--SELECT 'INSERT INTO [INSTRUMENTS] ('''+ISNULL([KEYTAG],'')+''','''+ISNULL([TAG],'')+''','''+ISNULL([TAG_NO],'')+''','''+ISNULL([TAG_CODE],'')+''','''+ISNULL([ASSOC_EQ],'')+''','''+ISNULL([IAREA],'')+''','''+ISNULL([ITRAIN],'')+''','''+ISNULL([ITYP],'')+''','''+ISNULL([INUM],'')+''','''+ISNULL([ISUFFIX],'')+''','''+ISNULL([MODIFIER1],'')+''','''+ISNULL([MODIFIER2],'')+''','''+ISNULL([MODIFIER3],'')+''','''+ISNULL([MODIFIER4],'')+''','''+ISNULL([STD_DETAIL],'')+''','''+ISNULL([DESCRIPT],'')+''','''+ISNULL([TAG_TYPE],'')+''','''+ISNULL([CONST_TYPE],'')+''','''+ISNULL([COMP_ID],'')+''','''+ISNULL([PROJ_STAT],'')+''','''+ISNULL([PID_NO],'')+''','''+ISNULL([LINE_NO],'')+''')' FROM [INSTRUMENTS]
DELETE FROM [INSTRUMENTS]
INSERT INTO [INSTRUMENTS] ('0100000098','90-FC-004A','090-FC-004A','A-T-N''U','','090','','FC','004','A','','','','','','','AT_INST_DCS','NEW','560257021_17981','REN','90-AO5676','66015-O')
INSERT INTO [INSTRUMENTS] ('0100000264','90-FC-004B','090-FC-004B','A-T-N''U','','090','','FC','004','B','','','','','','','AT_INST_DCS','NEW','560257021_17999','NEW','90-AO5676','66015-O')
INSERT INTO [INSTRUMENTS] ('0100000090','90-FC-007','090-FC-007','A-T-N','','090','','FC','007','','','','','','','','AT_INST_DCS','NEW','559983967_24310','NEW','90-AO5676','66015-O')
INSERT INTO [INSTRUMENTS] ('0100000092','90-FD-007','090-FD-007','A-T-N','','090','','FD','007','','','','','','','','AT_INST_','NEW','559983967_19100','NEW','90-AO5676','66015-O')
INSERT INTO [INSTRUMENTS] ('0100000080','90-FE-006','090-FE-006','A-T-N','','090','','FE','006','','','','','','','','AT_INST_','NEW','559983967_19933','NEW','90-AO5676','66015-O')
INSERT INTO [INSTRUMENTS] ('0100000114','90-FI-008','090-FI-008','A-T-N','','090','','FI','008','','','','','','','','AT_INST_DCS','NEW','560257021_19118','NEW','90-AO5676','66015-O')
INSERT INTO [INSTRUMENTS] ('0100000082','90-FIC-006','090-FIC-006','A-T-N','','090','','FIC','006','','','','','','','','AT_INST_DCS','NEW','559983967_17981','NEW','90-AO5676','90002-RV')
INSERT INTO [INSTRUMENTS] ('0100000099','90-FT-004','090-FT-004','A-T-N','','090','','FT','004','','','','','','','','AT_INST_','NEW','560257021_17962','NEW','90-AO5676','90002-RV')
INSERT INTO [INSTRUMENTS] ('0100000081','90-FT-006','090-FT-006','A-T-N','','090','','FT','006','','','','','','','','AT_INST_','NEW','559983967_17962','NEW','90-AO5676','90002-RV')
INSERT INTO [INSTRUMENTS] ('0100000091','90-FT-007','090-FT-007','A-T-N','','090','','FT','007','','','','','','','','AT_INST_','NEW','559983967_18795','NEW','90-AO5676','90002-RV')
INSERT INTO [INSTRUMENTS] ('0100000113','90-FT-008','090-FT-008','A-T-N','','090','','FT','008','','','','','','','','AT_INST_','NEW','560257021_19100','NEW','90-AO5676','90002-RV')
INSERT INTO [INSTRUMENTS] ('0100000101','90-FV-004','090-FV-004','A-T-N','','090','','FV','004','','','','','','','','AT_INST_','NEW','560257021_19720','NEW','90-AO5676','90002-RV')
INSERT INTO [INSTRUMENTS] ('0100000084','90-FV-006','090-FV-006','A-T-N','','090','','FV','006','','','','','','','','AT_INST_','NEW','559983967_19720','NEW','90-AO5676','90002-SW')
INSERT INTO [INSTRUMENTS] ('0100000096','90-FV-007','090-FV-007','A-T-N','','090','','FV','007','','','','','','','','AT_INST_','NEW','559983967_19263','NEW','90-AO5676','90002-SW')
INSERT INTO [INSTRUMENTS] ('0100000100','90-FY-004','090-FY-004','A-T-N','','090','','FY','004','','','','','','','','AT_INST_','NEW','560257021_19701','NEW','90-AO5676','90003-SC')
INSERT INTO [INSTRUMENTS] ('0100000083','90-FY-006','090-FY-006','A-T-N','','090','','FY','006','','','','','','','','AT_INST_','NEW','559983967_19701','NEW','90-AO5676','90003-SL')
INSERT INTO [INSTRUMENTS] ('0100000115','90-H-417','090-H-417','A-T-N','','090','','H','417','','','','','','','','AT_INST_','NEW','560257021_19031','NEW','90-AO5676','90003-V')
INSERT INTO [INSTRUMENTS] ('0100000106','90-LC-008','090-LC-008','A-T-N','','090','','LC','008','','','','','','','','AT_INST_DCS','NEW','560257021_19305','NEW','90-AO5676','90004-SC')
INSERT INTO [INSTRUMENTS] ('0100000104','90-LG-008','090-LG-008','A-T-N','','090','','LG','008','','','','','','','','AT_INST_','NEW','560257021_18875','NEW','90-AO5676','90004-SL')
INSERT INTO [INSTRUMENTS] ('0100000087','90-LG-607A','090-LG-607A','A-T-N''U','','090','','LG','607','A','','','','','','','AT_INST_','NEW','559983967_18875','NEW','90-AO5676','90005-RV')
INSERT INTO [INSTRUMENTS] ('0100000086','90-LG-607B','090-LG-607B','A-T-N''U','','090','','LG','607','B','','','','','','','AT_INST_','NEW','559983967_20397','NEW','90-AO5676','90006-O')
INSERT INTO [INSTRUMENTS] ('0100000085','90-LG-607C','090-LG-607C','A-T-N''U','','090','','LG','607','C','','','','','','','AT_INST_','NEW','559983967_20415','NEW','90-AO5676','90006-SL')
INSERT INTO [INSTRUMENTS] ('0100000089','90-LIC-007','090-LIC-007','A-T-N','','090','','LIC','007','','','','','','','','AT_INST_DCS','NEW','559983967_19305','NEW','90-AO5676','90007-SL')
INSERT INTO [INSTRUMENTS] ('0100000088','90-LT-007','090-LT-007','A-T-N','','090','','LT','007','','','','','','','','AT_INST_','NEW','559983967_18906','NEW','90-AO5676','90008-RV')
INSERT INTO [INSTRUMENTS] ('0100000105','90-LT-008','090-LT-008','A-T-N','','090','','LT','008','','','','','','','','AT_INST_','NEW','560257021_18906','NEW','90-AO5676','90009-O')
INSERT INTO [INSTRUMENTS] ('0100000076','90-LV-005','090-LV-005','A-T-N','','090','','LV','005','','','','','','','','AT_INST_','NEW','559983967_20315','NEW','90-AO5676','90010-O')
INSERT INTO [INSTRUMENTS] ('0100000108','90-LV-008','090-LV-008','A-T-N','','090','','LV','008','','','','','','','','AT_INST_','NEW','560257021_19263','NEW','90-AO5676','90011-O')
INSERT INTO [INSTRUMENTS] ('0100000107','90-LY-008','090-LY-008','A-T-N','','090','','LY','008','','','','','','','','AT_INST_','NEW','560257021_19283','NEW','90-AO5676','90012-O')
INSERT INTO [INSTRUMENTS] ('0100000109','90-PG-304','090-PG-304','A-T-N','','090','','PG','304','','','','','','','','AT_INST_','NEW','','NEW','90-AO5676','90013-O')
INSERT INTO [INSTRUMENTS] ('0100000110','90-PG-305','090-PG-305','A-T-N','','090','','PG','305','','','','','','','','AT_INST_','NEW','','NEW','90-AO5676','90014-RV')
INSERT INTO [INSTRUMENTS] ('0100000102','90-PG-605','090-PG-605','A-T-N','','090','','PG','605','','','','','','','','AT_INST_','NEW','','NEW','90-AO5676','90020-O')
INSERT INTO [INSTRUMENTS] ('0100000078','90-PG-609','090-PG-609','A-T-N','','090','','PG','609','','','','','','','','AT_INST_','NEW','559983967_18338','NEW','90-AO5676','90024-RV')
INSERT INTO [INSTRUMENTS] ('0100000094','90-QT-130','090-QT-130','A-T-N','','090','','QT','130','','','','','','','','AT_INST_','NEW','559983967_20691','NEW','90-AO5676','90025-SW')
INSERT INTO [INSTRUMENTS] ('0100000077','90-RO-602','090-RO-602','A-T-N','','090','','RO','602','','','','','','','','AT_INST_','NEW','559983967_19961','NEW','90-AO5676','90026-RV')
INSERT INTO [INSTRUMENTS] ('0100000093','90-RV-103','090-RV-103','A-T-N','','090','','RV','103','','','','','','','','AT_PSV','NEW','559983967_20985','NEW','90-AO5676','90028-RV')
INSERT INTO [INSTRUMENTS] ('0100000097','90-RV-104','090-RV-104','A-T-N','','090','','RV','104','','','','','','','','AT_PSV','NEW','559983967_21003','NEW','90-AO5676','90030-O')
INSERT INTO [INSTRUMENTS] ('0100000079','90-RV-107','090-RV-107','A-T-N','','090','','RV','107','','','','','','','','AT_PSV','NEW','559983967_20567','NEW','90-AO5676','90031-O')
INSERT INTO [INSTRUMENTS] ('0100000111','90-TC-602','090-TC-602','A-T-N','','090','','TC','602','','','','','','','','AT_INST_','NEW','560257021_18795','NEW','90-AO5676','90032-O')
INSERT INTO [INSTRUMENTS] ('0100000112','90-TC-632','090-TC-632','A-T-N','','090','','TC','632','','','','','','','','AT_INST_','NEW','560257021_18776','NEW','90-AO5676','90033-O')
INSERT INTO [INSTRUMENTS] ('0100000103','90-TE-004','090-TE-004','A-T-N','','090','','TE','004','','','','','','','','AT_INST_','NEW','','NEW','90-AO5676','90041-RV')
INSERT INTO [INSTRUMENTS] ('0100000095','90-TE-114','090-TE-114','A-T-N','','090','','TE','114','','','','','','','','AT_INST_','NEW','559983967_20672','NEW','90-AO5676','90042-RV')
INSERT INTO [INSTRUMENTS] ('0100000004','90-TE-807','090-TE-807','A-T-N','','090','','TE','807','','','','','','','','AT_INST_','NEW','560257021_18385','NEW','90-AO5676','90043-RV')
INSERT INTO [INSTRUMENTS] ('0100000075','90-TI-111','090-TI-111','A-T-N','','090','','TI','111','','','','','','','','AT_INST_','NEW','559983967_18460','NEW','90-AO5676','90058-O')

--SELECT 'INSERT INTO [LINES] VALUES ('''+ISNULL([TAG],'')+''','''+ISNULL([ID],'')+''','''+ISNULL([AREA],'')+''','''+ISNULL([TRAINNUMBER],'')+''','''+ISNULL([SPEC],'')+''','''+ISNULL([SYSTEM],'')+''','''+ISNULL([LINENO],'')+''','''+ISNULL([NOMDIAMETER],'')+''','''+ISNULL([INSULATIONTYPE],'')+''','''+ISNULL([HTRACED],'')+''','''+ISNULL([CONSTTYPE],'')+''','''+ISNULL([DESPRESSURE],'')+''','''+ISNULL([TESTPRESSURE],'')+''','''+ISNULL([PWHT],'')+''','''+ISNULL([TESTMEDIA],'')+''','''+ISNULL([MATLTYPE],'')+''','''+ISNULL([NDT],'')+''','''+ISNULL([NDE],'')+''','''+ISNULL([PIPECLASS],'')+''','''+ISNULL([PIDNUMBER],'')+''','''+ISNULL([DESTEMPERATURE],'')+''','''+ISNULL([PAINTSYSTEM],'')+''','''+ISNULL([DESIGNCODE],'')+''','''+ISNULL([COLOURCODE],'')+''','''+ISNULL([EWP],'')+''','''+ISNULL([USER1],'')+''','''+ISNULL([TAGSTATUS],'')+''','''+ISNULL([FULLLINE],'')+''','''+ISNULL([UOM_NOMDIAMETER],'')+''','''+ISNULL([UOM_DESPRESSURE],'')+''','''+ISNULL([UOM_DESTEMPERATURE],'')+''')' FROM [LINES]
DELETE FROM [LINES]
INSERT INTO [LINES] VALUES ('66015-O','0100000071','66','','','O','015','','','','NEW','34.5','','','','','','','','','147','','','','***','','NEW','66-O-015','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90002-RV','0100000025','90','','AAA3','RV','002','40','IP','','NEW','5.2','','','','','','','','90-AO5676','163','','','','***','','NEW','90-RV-002','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90002-SW','0100000017','90','','BBA3','SW','002','40','N','','NEW','5.2','','','','','','','','','163','','','','***','','NEW','90-SW-002','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90003-SC','0100000015','90','','AAA3','SC','003','80','N','','NEW','5.2','','','','','','','','90-AO5676','163','','','','***','','NEW','90-SC-003','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90003-SL','0100000011','90','','AAA3','SL','003','150','IH','','NEW','5.2','','','','','','','','','163','','','','***','','NEW','90-SL-003','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90003-V','0100000059','90','','AAA3','V','003','40','IP','','NEW','5.2','','','','','','','','90-AO5678','163','','','','***','','NEW','90-V-003','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90004-SC','0100000049','90','','AAA3','SC','004','40','IH','','NEW','5.2','','','','','','','','90-AO5678','163','','','','***','','NEW','90-SC-004','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90004-SL','0100000013','90','','AAA3','SL','004','100','IH','','NEW','5.2','','','','','','','','90-AO5676','163','','','','***','','NEW','90-SL-004','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90005-RV','0100000067','90','','AAB3','RV','005','80','N','','NEW','34.5','','','','','','','','90-AO5678','147','','','','***','','NEW','90-RV-005','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90006-O','0100000045','90','','AAB3','O','006','250','IP','','NEW','34.5','','','','','','','','90-AO5676','147','','','','***','','NEW','90-O-006','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90006-SL','0100000021','90','','AAA3','SL','006','50','IH','','NEW','5.2','','','','','','','','','163','','','','***','','NEW','90-SL-006','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90007-SL','0100000053','90','','AAA3','SL','007','40','IH','','NEW','5.2','','','','','','','','','150','','','','***','','NEW','90-SL-007','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90008-RV','0100000047','90','','AFU3','RV','008','150','IH','','NEW','5.2','','','','','','','','90-AO5678','163','','','','***','','NEW','90-RV-008','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90009-O','0100000041','90','','AAB3','O','009','150','IP','','NEW','5.2','','','','','','','','','163','','','','***','','NEW','90-O-009','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90010-O','0100000039','90','','AAB3','O','010','80','IH','','NEW','5.2','','','','','','','','90-AO5676','163','','','','***','','NEW','90-O-010','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90011-O','0100000043','90','','AAB3','O','011','80','IP','','NEW','5.2','','','','','','','','90-AO5676','150','','','','***','','NEW','90-O-011','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90012-O','0100000031','90','','AAB3','O','012','100','IP','','NEW','5.2','','','','','','','','90-AO5676','163','','','','***','','NEW','90-O-012','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90013-O','0100000037','90','','AAB3','O','013','100','N','','NEW','5.2','','','','','','','','90-AO5676','163','','','','***','','NEW','90-O-013','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90014-RV','0100000065','90','','AAA3','RV','014','80','N','','NEW','5.2','','','','','','','','90-AO5678','150','','','','***','','NEW','90-RV-014','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90020-O','0100000073','90','','BBA3','O','020','50','IH','','NEW','5.2','','','','','','','','','163','','','','***','','NEW','90-O-020','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90024-RV','0100000019','90','','BFU3','RV','024','25','N','','NEW','5.2','','','','','','','','90-AO5676','163','','','','***','','REN','90-RV-024','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90025-SW','0100000023','90','','AAA3','SW','025','50','N','','NEW','5.2','','','','','','','','90-AO5676','163','','','','***','','NEW','90-SW-025','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90026-RV','0100000033','90','','BFU3','RV','026','40','PP','','NEW','34.5','','','','','','','','','147','','','','***','','NEW','90-RV-026','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90028-RV','0100000035','90','','AAB3','RV','028','25','N','','NEW','34.5','','','','','','','','90-AO5676','163','','','','***','','NEW','90-RV-028','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90030-O','0100000027','90','','AAB3','O','030','300','IP','','NEW','5.2','','','','','','','','90-AO5676','163','','','','***','','NEW','90-O-030','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90031-O','0100000061','90','','AAB3','O','031','100','IH','','NEW','5.2','','','','','','','','90-AO5678','150','','','','***','','NEW','90-O-031','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90032-O','0100000029','90','','BBU3','O','032','350','IP','','NEW','5.2','','','','','','','','90-AO5676','163','','','','***','','NEW','90-O-032','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90033-O','0100000063','90','','AAA3','O','033','150','IH','','NEW','5.2','','','','','','','','90-AO5678','150','','','','***','','NEW','90-O-033','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90041-RV','0100000051','90','','BFU3','RV','041','25','N','','NEW','34.5','','','','','','','','90-AO5678','147','','','','***','','NEW','90-RV-041','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90042-RV','0100000055','90','','BFU3','RV','042','40','N','','NEW','5.2','','','','','','','','90-AO5678','150','','','','***','','NEW','90-RV-042','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90043-RV','0100000057','90','','BFU3','RV','043','40','PP','','NEW','5.2','','','','','','','','','150','','','','***','','NEW','90-RV-043','mm','kPag','Deg C')
INSERT INTO [LINES] VALUES ('90058-O','0100000069','90','','AAB3','O','058','80','N','','NEW','34.5','','','','','','','','90-AO5678','147','','','','***','','NEW','90-O-058','mm','kPag','Deg C')

--SELECT 'INSERT INTO [VALVES] VALUES ('''+ISNULL([KEYTAG],'')+''','''+ISNULL([TAG_NO],'')+''','''+ISNULL([VAREA],'')+''','''+ISNULL([VTYP],'')+''','''+ISNULL([VTRAIN],'')+''','''+ISNULL([VNUM],'')+''','''+ISNULL([VSUFFIX],'')+''','''+ISNULL([TAG_TYPE],'')+''','''+ISNULL([CONST_TYPE],'')+''','''+ISNULL([COMP_ID],'')+''','''+ISNULL([VSIZE],'')+''','''+ISNULL([UOM_VSIZE],'')+''','''+ISNULL([VSPEC_TYPE],'')+''','''+ISNULL([VSPEC_NUM],'')+''','''+ISNULL([VPRESRATE],'')+''','''+ISNULL([VCONDITION],'')+''','''+ISNULL([PID_NO],'')+''','''+ISNULL([PROJ_STAT],'')+''')' FROM [VALVES]
DELETE FROM [VALVES]
INSERT INTO [VALVES] VALUES ('0100000116','90-HV-801','90','HV','','801','','AT_HVALVE','NEW','559983967_20214','80','mm','VBA','001','','','90-AO5678','NEW')
INSERT INTO [VALVES] VALUES ('0100000117','90-HV-802','90','HV','','802','','AT_HVALVE','NEW','559983967_20223','80','mm','VBA','101','','','90-AO5678','NEW')
INSERT INTO [VALVES] VALUES ('0100000118','90-HV-803','90','HV','','803','','AT_HVALVE','NEW','559983967_24079','50','mm','VGA','001','','','90-AO5678','NEW')