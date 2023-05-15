use SimpleRPG

drop table if exists tb_account
drop procedure if exists usp_account_insert
drop procedure if exists usp_account_select

create table tb_account
(
	unique_index	bigint identity(1,1) not null,
	account_id		nvarchar(32) not null,
	account_pw		nvarchar(32) not null,
	create_date		datetime not null,
	delete_date		datetime,
	cash			int not null,
	gm_level		tinyint not null

	constraint pk_account primary key (account_id, account_pw)
)

go
create or alter procedure usp_account_insert
@account_id		nvarchar(32),
@account_pw		nvarchar(32)
as
begin
set nocount on
begin tran

-- 중복 ID 확인
if exists (select unique_index from tb_account where account_id = @account_id)
begin
	rollback tran
	return -1
end

insert into tb_account (account_id, account_pw, cash, create_date, gm_level)
output inserted.unique_index
values (@account_id, @account_pw, 0, getdate(), 0)

if @@rowcount != 1
begin
	rollback tran
	return 0
end

commit tran
return 1
end

go
create or alter procedure usp_account_select
@account_id nvarchar(32),
@account_pw nvarchar(32)
as
begin
set nocount on
select unique_index, cash, gm_level from tb_account where account_id = @account_id and account_pw = @account_pw and delete_date is null
end

go
create or alter procedure usp_account_update_cash
@account_index	bigint,
@cash			int
as
begin
set nocount on
begin tran

update tb_account set cash = @cash where unique_index = @account_index
if @@rowcount != 1
begin
	rollback tran
	return 0
end

commit tran
return 1
end