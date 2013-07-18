create database if not exists iring;
delete from mysql.user where user='iring';
create user 'iring' identified by 'iring';
grant all on iring.* to iring;