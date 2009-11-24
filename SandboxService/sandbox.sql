create database if not exists sandbox;
delete from mysql.user where user='sandbox';
create user 'sandbox' identified by 'sandbox';
grant all on sandbox.* to sandbox;