using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public static class GameDBToCharacter
    {
        public static int CharacterInsert(GameUserSession user, string characterName, out long characterID)
        {
            characterID = 0;
            using (var conn = GameDB.Instance.GetDB())
            {
                if (false == conn.NewProcedure("usp_character_insert"))
                    return ProcedureResult.FAIL;

                conn.AddReturnValue();
                conn.AddParamToWString("@character_name", characterName);
                conn.AddParam("@account_index", user.AccountID);
                conn.AddParam("@map_index", GameData.StartMapID);
                conn.AddParam("@x", 0);
                conn.AddParam("@y", 0);
                conn.AddParam("@z", 0);
                conn.AddParam("@hp", GameData.StartMaxHP);
                conn.AddParam("@sp", GameData.StartMaxSP);
                conn.AddParam("@basic_attack", GameData.StartBasicAttack);
                conn.AddParam("@skill_attack", GameData.StartSkillAttack);
                conn.AddParam("@basic_defense", GameData.StartBasicDefense);
                conn.AddParam("@skill_defense", GameData.StartSkillDefense);

                if (false == conn.GetProcedure())
                    return ProcedureResult.FAIL;

                var result = conn.GetReturnValue();
                if (ProcedureResult.SUCCESS != result)
                    return result;

                if (false == conn.Fetch())
                    return ProcedureResult.FAIL;

                characterID = conn.GetParam<long>("unique_index");
                return ProcedureResult.SUCCESS;
            }
        }

        public static bool CharacterDelete(long characterID)
        {
            using (var conn = GameDB.Instance.GetDB())
            {
                if (false == conn.NewProcedure("usp_character_delete"))
                    return false;

                conn.AddReturnValue();
                conn.AddParam("@character_index", characterID);

                if (false == conn.TryProcedure())
                    return false;

                return ProcedureResult.SUCCESS == conn.GetReturnValue();
            }
        }

        public static List<SCharacterListInfo> GetCharacterListInfo(GameUserSession user)
        {
            using (var conn = GameDB.Instance.GetDB())
            {
                if (false == conn.NewProcedure("usp_character_select_all"))
                    return null;

                conn.AddParam("@account_index", user.AccountID);

                if (false == conn.GetProcedure())
                    return null;

                var characterList = new List<SCharacterListInfo>();

                while (true == conn.Fetch())
                {
                    var info = new SCharacterListInfo();
                    info.index = conn.GetParam<long>("unique_index");
                    info.name = conn.GetParamString("character_name");
                    info.level = conn.GetParam<int>("character_level");

                    characterList.Add(info);
                }
                return characterList;
            }
        }
    }
}
