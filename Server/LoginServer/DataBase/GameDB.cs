using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer
{
    public class GameDB : Singleton<GameDB>
    {
        private SQLServer SQLServer;

        private MSSQLConnection GetDB()
        {           
            return SQLServer.Get();
        }

        public void Initialize()
        {
            SQLServer = new SQLServer();
            SQLServer.Initialize("Jsons\\GameDB.json");
        }

        public ServerErrorCode AccountCreate(string id, string password, out long accountID)
        {
            accountID = 0;
            using (var conn = GetDB())
            {
                if (false == conn.NewProcedure("usp_account_create"))
                    return ServerErrorCode.DB_ERROR;

                conn.AddReturnValue();
                conn.AddParamToWString("@account_id", id);
                conn.AddParamToWString("@account_pw", password);

                if (false == conn.GetProcedure())
                    return ServerErrorCode.DB_ERROR;

                var result = conn.GetReturnValue();
                if (ProcedureResult.SUCCESS != result)
                    return ServerErrorCode.ALREADY_USED_ACCOUNT_ID;

                if (false == conn.Fetch())
                    return ServerErrorCode.DB_ERROR;

                accountID = conn.GetParam<long>("unique_index");
                return ServerErrorCode.SUCCESS;
            }
        }

        public ServerErrorCode AccountLogin(string id, string password, out long accountID)
        {
            accountID = 0;
            using (var conn = GetDB())
            {
                if (false == conn.NewProcedure("usp_account_login"))
                    return ServerErrorCode.DB_ERROR;

                conn.AddParamToWString("@account_id", id);
                conn.AddParamToWString("@account_pw", password);

                if (false == conn.GetProcedure())
                    return ServerErrorCode.DB_ERROR;

                if (false == conn.Fetch())
                    return ServerErrorCode.NOT_FOUND_ACCOUNT_INFO;

                accountID = conn.GetParam<long>("unique_index");
                return ServerErrorCode.SUCCESS;
            }
        }
    }
}
