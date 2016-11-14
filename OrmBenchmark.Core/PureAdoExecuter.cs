﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmBenchmark.Core
{
    public class PureAdoExecuter : IOrmExecuter
    {
        SqlConnection conn;

        public string Name
        {
            get
            {
                return "Pure ADO";
            }
        }

        public void Init(string connectionStrong)
        {
            conn = new SqlConnection(connectionStrong);
            conn.Open();
        }

        public object GetItemAsObject(int Id)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"select Id, [Text], [CreationDate], LastChangeDate, 
                Counter1,Counter2,Counter3,Counter4,Counter5,Counter6,Counter7,Counter8,Counter9 from Posts where Id = @Id";
            var idParam = cmd.Parameters.Add("@Id", System.Data.SqlDbType.Int);
            idParam.Value = Id;

            dynamic obj;
            using (var reader = cmd.ExecuteReader())
            {
                reader.Read();
                obj = new {
                    Id = reader.GetInt32(0),
                    Text = reader.GetValue(1),
                    CreationDate = reader.GetDateTime(2),
                    LastChangeDate = reader.GetDateTime(3),
                    Counter1 = reader.GetNullableValue<int>(4),
                    Counter2 = reader.GetNullableValue<int>(5),
                    Counter3 = reader.GetNullableValue<int>(6),
                    Counter4 = reader.GetNullableValue<int>(7),
                    Counter5 = reader.GetNullableValue<int>(8),
                    Counter6 = reader.GetNullableValue<int>(9),
                    Counter7 = reader.GetNullableValue<int>(10),
                    Counter8 = reader.GetNullableValue<int>(11),
                    Counter9 = reader.GetNullableValue<int>(12),
                };
            }

            return obj;
        }

        public dynamic GetItemAsDynamic(int Id)
        {
            throw new NotImplementedException();
        }

        public IList<object> GetAllItemsAsObject()
        {
            throw new NotImplementedException();
        }

        public IList<dynamic> GetAllItemsAsDynamic()
        {
            throw new NotImplementedException();
        }

        public void Finish()
        {
            conn.Close();
        }
       
    }
}
