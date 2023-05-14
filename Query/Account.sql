use SimpleRPG

drop table if exists tb_account
drop proc if exists usp_account_signin
drop proc if exists usp_account_signup

create table tb_account
(
	idx			bigint identity(1, 1) not null primary key,
	account_id	nvarchar(32) not null,
	account_pwd	nvarchar(32) not null,
	gm_level	tinyint,
	create_date datetime not null,
	delete_date datetime
)

go
create or alter procedure usp_account_signup
@account_id		nvarchar(32),
@account_pwd	nvarchar(32)
as
begin
set nocount on
if exists (select idx from tb_account where account_id = @account_id)
begin
	return -1
end

insert into tb_account (account_id, account_pwd, gm_level, create_date) 
output inserted.idx values (@account_id, @account_pwd, 0, getdate())
if @@rowcount != 1
begin
	return -2
end

return 1
end

go
create or alter procedure usp_account_signin
@account_id		nvarchar(32),
@account_pwd	nvarchar(32)
as
begin
set nocount on
if not exists (select idx from tb_account where account_id = @account_id and account_pwd = @account_pwd and delete_date is null)
begin
	return -1
end

select idx, gm_level from tb_account where account_id = @account_id and account_pwd = @account_pwd
return 1
end