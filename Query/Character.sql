use SimpleRPG

drop table if exists	tb_character
drop proc if exists		usp_character_name_check
drop proc if exists		usp_character_insert
drop proc if exists		usp_character_delete
drop proc if exists		usp_character_select
drop proc if exists		usp_character_update_gold
drop proc if exists		usp_character_update_position
drop proc if exists		usp_character_update_status

create table tb_character
(
	character_idx		bigint identity(1, 1),
	account_idx			bigint not null,
	character_name		nvarchar(32) not null,

	character_level		int not null,
	character_exp		bigint not null,
	character_hp		int not null,
	character_sp		int not null,

	gold				bigint not null,

	position_x			bigint not null,
	position_y			bigint not null,

	create_date			datetime not null,
	delete_date			datetime,

	constraint pk_character primary key (character_idx)
)

go
create or alter proc usp_character_name_check
@character_name nvarchar(32)
as
begin
set nocount on

if exists (select character_idx from tb_character where character_name = @character_name)
begin
	return -1
end

return 1
end

go
create or alter proc usp_character_insert
@account_idx	bigint,
@character_name nvarchar(32),
@gold			bigint,
@character_hp	int,
@character_sp	int
as
begin
set nocount on

if exists (select character_idx from tb_character where character_name = @character_name)
begin
	return -1
end

insert into tb_character (account_idx, character_name, character_level, character_exp, character_hp, character_sp, gold, position_x, position_y, create_date)
output inserted.character_idx
values (@account_idx, @character_name, 1, 0, @character_hp, @character_sp, @gold, 0, 0, getdate())

if @@rowcount != 1
begin
	return -2
end

return 1
end

go
create or alter proc usp_character_delete
@character_idx bigint
as
begin
set nocount on

update tb_character set character_name = 'deleted_' + character_name, delete_date = getdate() where character_idx = @character_idx
if @@rowcount != 1
begin
	return -1
end

return 1
end

go
create or alter proc usp_character_select
@account_idx bigint
as
begin
set nocount on

select character_idx, character_name, character_level, character_exp, character_hp, character_sp, gold, position_x, position_y
from tb_character where account_idx = @account_idx and delete_date is null
order by create_date asc
end

go
create or alter proc usp_character_update_gold
@character_idx bigint,
@gold bigint
as
begin
set nocount on

update tb_character set gold = @gold where character_idx = @character_idx and delete_date is null

if @@rowcount != 1
begin
	return -1
end

return 1
end

go
create or alter proc usp_character_update_position
@character_idx bigint,
@position_x bigint,
@position_y bigint
as
begin
set nocount on

update tb_character set position_x = @position_x, position_y = @position_y
where character_idx = @character_idx and delete_date is null

if @@rowcount != 1
begin
	return -1
end

return 1
end

go
create or alter proc usp_character_update_status
@character_idx bigint,
@character_level int,
@character_exp bigint,
@character_hp int,
@character_sp int
as
begin
set nocount on

update tb_character set character_level = @character_level, character_exp = @character_exp, character_hp = @character_hp, character_sp = @character_sp
where character_idx = @character_idx and delete_date is null

if @@rowcount != 1
begin
	return -1
end

return 1
end