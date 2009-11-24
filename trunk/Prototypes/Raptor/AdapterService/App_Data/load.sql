use Camelot
go

create table Line 
(
 tag varchar(255) not null,
 diameter float,
 uomDiameter varchar(255),
 system varchar(255),
 constraint PK_Line primary key (tag)
)
go

create table InLinePipingComponent 
(
 tag varchar(255) not null,
 componentType varchar(255) not null,
 diameter float,
 uomDiameter varchar(255),
 rating varchar(255),
 system varchar(255),
 unit varchar(255),
 projectNumber varchar(255),
 pid varchar(255),
 constraint PK_InLinePipingComponent primary key (tag)
)
go

create table VacuumTower 
(
 tag varchar(255) not null,
 description varchar(255),
 constraint PK_VacuumTower primary key (tag)
)
go

create table KOPot 
(
 tag varchar(255) not null,
 description varchar(255),
 constraint PK_KOPot primary key (tag)
)
go

insert into InLinePipingComponent(tag, componentType, diameter, uomDiameter, rating, system, unit, projectNumber, pid) values ('1-AB-PV-001', 'Valve', 1.5, 'INCH', '150 lb', '90', '1', '24193', '1-M6-AB-001')
insert into InLinePipingComponent(tag, componentType, diameter, uomDiameter, rating, system, unit, projectNumber, pid) values ('1-AB-PV-002', 'Instrument', 1.5, 'INCH', '150 lb', '90', '2', '24193', '1-M6-AB-001')
insert into InLinePipingComponent(tag, componentType, diameter, uomDiameter, rating, system, unit, projectNumber, pid) values ('1-AB-PV-003', 'Valve', 1.5, 'INCH', '150 lb', '90', '3', '24193', '1-M6-AB-001')
insert into InLinePipingComponent(tag, componentType, diameter, uomDiameter, rating, system, unit, projectNumber, pid) values ('1-AB-PV-004', 'Instrument', 1.5, 'INCH', '150 lb', '90', '4', '24193', '1-M6-AB-001')

insert into KOPot (tag, description) values ('1-AB-KO-001', 'Knock Out Vessel')
insert into KOPot (tag, description) values ('1-AB-KO-002', 'Knock Out Vessel')
insert into KOPot (tag, description) values ('1-AB-KO-003', 'Knock Out Vessel')
insert into KOPot (tag, description) values ('1-AB-KO-004', 'Knock Out Vessel')

insert into VacuumTower (tag, description) values ('1-AB-VT-001', 'Vacuum Vessel')
insert into VacuumTower (tag, description) values ('1-AB-VT-002', 'Vacuum Vessel')
insert into VacuumTower (tag, description) values ('1-AB-VT-003', 'Vacuum Vessel')
insert into VacuumTower (tag, description) values ('1-AB-VT-004', 'Vacuum Vessel')

insert into Line (tag, diameter, uomDiameter, system) values ('1-AB-L-001', 1, 'INCH', 'AB')
insert into Line (tag, diameter, uomDiameter, system) values ('1-AB-L-002', 2, 'INCH', 'AB')
insert into Line (tag, diameter, uomDiameter, system) values ('1-AB-L-003', 3, 'INCH', 'AB')
insert into Line (tag, diameter, uomDiameter, system) values ('1-AB-L-004', 4, 'INCH', 'AB')
