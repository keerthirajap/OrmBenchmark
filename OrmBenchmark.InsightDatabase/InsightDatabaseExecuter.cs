using Insight.Database;
using OrmBenchmark.Core;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace OrmBenchmark.InsightDatabase
{
    public class InsightDatabaseExecuter : IOrmExecuter
    {
        private SqlConnection conn;

        public string TestName => "Default";
        public string ORMName => "Insight Database";

        public void Finish()
        {
            conn.Close();
        }

        public IList<dynamic> GetAllItemsAsDynamic()
        {
            // Log the start of the method
            Console.WriteLine($"{ORMName} | {TestName} - Starting GetAllItemsAsDynamicAsync method...");

            try
            {
                // Query the database and return the result
                var result = conn.QuerySql<dynamic>("SELECT * FROM Posts");
                var resultList = result.ToList();

                // Log the result
                Console.WriteLine($"{ORMName} | {TestName} - Retrieved {resultList.Count} items.");

                return resultList;
            }
            catch (Exception ex)
            {
                // Log any exceptions
                Console.WriteLine($"{ORMName} | {TestName} - Error: {ex.Message}");
                return new List<dynamic>(); // Return an empty list on error
            }
        }

        public async Task<IList<IPost>> GetAllItemsAsObjectAsync()
        {
            // Log the start of the method
            Console.WriteLine($"{ORMName} | {TestName} - Starting GetAllItemsAsObjectAsync method...");

            try
            {
                // Query the database and return the result
                var posts = await conn.QuerySqlAsync<Post>("SELECT * FROM Posts");
                var resultList = posts.Cast<IPost>().ToList();

                // Log the result
                Console.WriteLine($"{ORMName} | {TestName} - Retrieved {resultList.Count} items.");

                return resultList;
            }
            catch (Exception ex)
            {
                // Log any exceptions
                Console.WriteLine($"{ORMName} | {TestName} - Error: {ex.Message}");
                return new List<IPost>(); // Return an empty list on error
            }
        }

        public dynamic GetItemAsDynamic(int Id)
        {
            // Log the start of the method
            Console.WriteLine($"{ORMName} | {TestName} - Starting GetItemAsDynamicAsync method...");

            try
            {
                var result = conn.QuerySql<dynamic>("SELECT TOP 1 * FROM Posts WHERE Id=@Id", new { Id });

                // Log the result
                Console.WriteLine($"{ORMName} | {TestName} - Retrieved item with Id {Id}.");

                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // Log any exceptions
                Console.WriteLine($"{ORMName} | {TestName} - Error: {ex.Message}");
                return null; // Return null on error
            }
        }

        public async Task<IPost> GetItemAsObjectAsync(int Id)
        {
            // Log the start of the method
            Console.WriteLine($"{ORMName} | {TestName} - Starting GetItemAsObjectAsync method...");

            try
            {
                var posts = await conn.QuerySqlAsync<Post>("SELECT * FROM Posts WHERE Id = @Id", new { Id });
                var post = posts.FirstOrDefault();

                // Log the result
                Console.WriteLine($"{ORMName} | {TestName} - Retrieved item with Id {Id}.");

                return (IPost)post;
            }
            catch (Exception ex)
            {
                // Log any exceptions
                Console.WriteLine($"{ORMName} | {TestName} - Error: {ex.Message}");
                return null; // Return null on error
            }
        }

        public void Init(string connectionString)
        {
            conn = new SqlConnection(connectionString);
            SqlInsightDbProvider.RegisterProvider();
        }
    }
}