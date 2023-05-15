use SimpleRPG

drop table if exists tb_character
drop table if exists tb_character_status
drop table if exists tb_character_history

drop procedure if exists usp_character_insert
drop procedure if exists usp_character_delete
drop procedure if exists usp_character_select
drop procedure if exists usp_character_select_all
drop procedure if exists usp_character_update_gold
drop procedure if exists usp_character_update_exp
drop procedure if exists usp_character_update_level_up
drop procedure if exists usp_character_update_info
drop procedure if exists usp_character_update_status
drop procedure if exists usp_character_update_position
drop procedure if exists usp_character_update_upgrade
drop procedure if exists usp_character_login
drop procedure if exists usp_character_logout

create table tb_character
(
	unique_index	bigint not null identity(1,1),
	account_index	bigint not null,
	character_name	nvarchar(32) not null,
	gold			bigint not null,
	status_point	int not null,
	character_level int not null,
	character_exp	bigint not null,
	character_hp	int not null,
	character_sp	int not null,
	map_index		int not null,
	x				int not null,
	y				int not null,
	z				int not null,
	create_date		datetime not null,
	delete_date		datetime

	constraint pk_character primary key (unique_index)
)

create table tb_character_status
(
	character_index bigint not null,
	max_hp			int not null,
	max_sp			int not null,
	basic_attack	int not null,
	skill_attack	int not null,
	basic_defense	int not null,
	skill_defense	int not null,

	constraint pk_character_status primary key (character_index)
)

create table tb_character_history
(
	character_index bigint not null,
	login_date		datetime,
	logout_date		datetime,
	play_time		bigint

	constraint pk_character_history primary key (character_index)
)

go
create or alter procedure usp_character_insert
@character_name nvarchar(32),
@account_index	bigint,
@map_index		int,
@x				int,
@y				int,
@z				int,
@hp				int,
@sp				int,
@basic_attack	int,
@skill_attack	int,
@basic_defense	int,
@skill_defense	int
as
begin
set nocount on
begin tran

-- 중복 캐릭터 명 확인
if exists (select unique_index from tb_character where character_name = @character_name)
begin
	rollback tran
	return -1
end

insert into tb_character (character_name, account_index, gold, status_point, character_level, character_exp, character_hp, character_sp, map_index, x, y, z, create_date)
output inserted.unique_index
values (@character_name, @account_index, 0, 0, 1, 0, @hp, @sp, @map_index, @x, @y, @z, getdate())

if @@rowcount != 1
begin
	rollback tran
	return 0
end

declare @character_index bigint = scope_identity()

insert into tb_character_status (character_index, max_hp, max_sp, basic_attack, skill_attack, basic_defense, skill_defense)
values (@character_index, @hp, @sp, @basic_attack, @skill_attack, @basic_defense, @skill_defense)

if @@rowcount != 1
begin
	rollback tran
	return 0
end

commit tran
return 1
end

go
create or alter procedure usp_character_delete
@character_index bigint
as
begin
set nocount on

update tb_character set character_name = 'Delete_' + character_name, delete_date = getdate()
where unique_index = @character_index

if @@rowcount != 1
begin
	return 0
end

return 1
end

go
create or alter procedure usp_character_select
@character_index bigint
as
begin
set nocount on

select 
a.unique_index, a.character_name, a.gold, a.status_point, a.character_level, a.character_exp, a.character_hp, a.character_sp, a.map_index, a.x, a.y, a.z,
b.max_hp, b.max_sp, b.basic_attack, b.skill_attack, b.basic_defense, b.skill_defense
from tb_character as a inner join tb_character_status as b
on a.unique_index = b.character_index
where a.unique_index = @character_index and a.delete_date is null

end

go
create or alter procedure usp_character_select_all
@account_index bigint
as
begin
set nocount on

select unique_index, character_name, character_level from tb_character
where account_index = @account_index

end

go
create or alter procedure usp_character_update_gold
@character_index	bigint,
@gold				bigint
as
begin
set nocount on

update tb_character set gold = @gold 
where unique_index = @character_index

if @@rowcount != 1
begin
	return 0
end

return 1
end

go
create or alter procedure usp_character_update_exp
@character_index	bigint,
@exp				int
as
begin
set nocount on

update tb_character set character_exp = @exp 
where unique_index = @character_index

if @@rowcount != 1
begin
	return 0
end

return 1
end

go
create or alter procedure usp_character_update_level_up
@character_index	bigint,
@level				int,
@status_point		int
as
begin
set nocount on

update tb_character set character_level = @level, status_point = @status_point
where unique_index = @character_index

if @@rowcount != 1
begin
	return 0
end

return 1
end

go
create or alter procedure usp_character_update_status
@character_index	bigint,
@hp					int,
@sp					int
as
begin
set nocount on

update tb_character set character_hp = @hp, character_sp = @sp
where unique_index = @character_index

if @@rowcount != 1
begin
	return 0
end

return 1
end

go
create or alter procedure usp_character_update_position
@character_index	bigint,
@map_index			int,
@x					int,
@y					int,
@z					int
as
begin
set nocount on

update tb_character set map_index = @map_index, x = @x, y = @y, z = @z
where unique_index = @character_index

if @@rowcount != 1
begin
	return 0
end

return 1
end

go
create or alter procedure usp_character_update_upgrade
@character_index	bigint,
@max_hp				int,
@max_sp				int,
@basic_attack		int,
@skill_attack		int,
@basic_defense		int,
@skill_defense		int
as
begin
set nocount on

update tb_character_status set max_hp = @max_hp, max_sp = @max_sp, basic_attack = @basic_attack, skill_attack = @skill_attack, basic_defense = @basic_defense, skill_defense = @skill_defense
where character_index = @character_index

if @@rowcount != 1
begin
	return 0
end

return 1
end

go
create or alter procedure usp_character_login
@character_index bigint
as
begin
set nocount on

update tb_character_history set login_date = getdate()
where character_index = @character_index

if @@rowcount != 1
begin
	return 0
end

return 1
end

go
create or alter procedure usp_character_logout
@character_index bigint
as
begin
set nocount on

update tb_character_history set logout_date = getdate(), play_time += datediff_big(second, login_date, logout_date)
where character_index = @character_index

if @@rowcount != 1
begin
	return 0
end

return 1
end
