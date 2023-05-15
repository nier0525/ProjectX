using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public static class GameDBToAccount
    {
        public static int AccountInsert(GameUserSession user, string id, string password)
        {
            using (var conn = GameDB.Instance.GetDB())
            {
                if (false == conn.NewProcedure("usp_account_insert"))
                    return ProcedureResult.FAIL;

                conn.AddReturnValue();
                conn.AddParamToWString("@account_id", id);
                conn.AddParamToWString("@account_pw", password);

                if (false == conn.GetProcedure())
                    return ProcedureResult.FAIL;

                var result = conn.GetReturnValue();
                if (ProcedureResult.SUCCESS != result)
                    return result;

                if (false == conn.Fetch())
                    return ProcedureResult.FAIL;

                user.AccountID  = conn.GetParam<long>("unique_index");
                user.GMLevel    = GMLevel.NORMAL;
                user.Cash       = 0;

                return ProcedureResult.SUCCESS;
            }
        }

        public static int AccountSelect(GameUserSession user, string id, string password)
        {
            using (var conn = GameDB.Instance.GetDB())
            {
                if (false == conn.NewProcedure("usp_account_select"))
                    return ProcedureResult.FAIL;

                conn.AddParamToWString("@account_id", id);
                conn.AddParamToWString("@account_pw", password);

                if (false == conn.GetProcedure())
                    return ProcedureResult.FAIL;

                if (false == conn.Fetch())
                    return -1;

                user.AccountID  = conn.GetParam<long>("unique_index");
                user.GMLevel    = conn.GetParam<GMLevel>("gm_level");
                user.Cash       = conn.GetParam<int>("cash");

                return ProcedureResult.SUCCESS;
            }
        }

        public static bool AccountUpdateCash(GameUserSession user)
        {
            using (var conn = GameDB.Instance.GetDB())
            {
                if (false == conn.NewProcedure("usp_account_update_cash"))
                    return false;

                conn.AddReturnValue();
                conn.AddParam("@account_index", user.AccountID);
                conn.AddParam("@cash", user.Cash);

                if (false == conn.TryProcedure())
                    return false;

                return ProcedureResult.SUCCESS == conn.GetReturnValue();
            }
        }
    }
}
