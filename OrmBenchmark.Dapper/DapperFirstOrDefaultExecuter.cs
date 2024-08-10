using Dapper;
using OrmBenchmark.Core;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace OrmBenchmark.Dapper
{
    public class DapperFirstOrDefaultExecuter : IOrmExecuter
    {
        private SqlConnection conn;

        public string TestName
        {
            get
            {
                return "Dapper Query (First Or Default)";
            }
        }

        public string ORMName => "Insight Database";

        public void Init(string connectionStrong)
        {
            conn = new SqlConnection(connectionStrong);
            conn.Open();
        }

        public IPost GetItemAsObject(int Id)
        {
            object param = new { Id = Id };
            return conn.QueryFirstOrDefault<Post>("select * from Posts where Id=@Id", param);
        }

        public dynamic GetItemAsDynamic(int Id)
        {
            object param = new { Id = Id };
            return conn.QueryFirstOrDefault("select * from Posts where Id=@Id", param);
        }

        public IList<IPost> GetAllItemsAsObject()
        {
            return null;
        }

        public IList<dynamic> GetAllItemsAsDynamic()
        {
            return null;
        }

        public void Finish()
        {
            conn.Close();
        }
    }
}