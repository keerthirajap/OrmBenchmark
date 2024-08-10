namespace OrmBenchmark.InsightDatabase
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using Insight.Database;
    using OrmBenchmark.Core;

    public class InsightDatabaseConcurrencyStressTest : IOrmExecuter
    {
        private SqlConnection conn;

        public string TestName => "Concurrency Stress Test";
        public string ORMName => "Insight Database";

        public void Finish()
        {
            conn.Close();
        }

        public IList<dynamic> GetAllItemsAsDynamic()
        {
            const int totalTasks = 100;
            var tasks = new List<Task<IList<dynamic>>>(totalTasks);

            // Log the start of the method
            Console.WriteLine($"{ORMName} | {TestName} - Starting GetAllItemsAsDynamic method...");

            for (int i = 0; i < totalTasks; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        // Ensure connection is properly managed
                        using (var localConn = new SqlConnection(conn.ConnectionString))
                        {
                            localConn.Open(); // Open the connection
                            // Query the database and return the result
                            var result = localConn.QuerySql<dynamic>("SELECT * FROM Posts").ToList();
                            return (IList<dynamic>)result; // Cast result to IList<dynamic>
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log any exceptions
                        Console.WriteLine($"{ORMName} | {TestName} - Task {Task.CurrentId} encountered an error: {ex.Message}");
                        return new List<dynamic>(); // Return an empty list on error
                    }
                }));
            }

            // Log before waiting for tasks to complete
            Console.WriteLine("Waiting for all tasks to complete...");

            // Wait for all tasks to complete
            var results = Task.WhenAll(tasks).Result;

            // Log after all tasks are complete
            Console.WriteLine("All tasks completed.");

            // Aggregate results from all tasks
            var aggregatedResults = results.SelectMany(result => result).ToList();

            // Log the result
            Console.WriteLine(aggregatedResults.Any() ? "Results retrieved successfully." : "No results found.");

            // Log the end of the method
            Console.WriteLine($"{ORMName} | {TestName} - Ending GetAllItemsAsDynamic method.");

            return aggregatedResults;
        }

        public async Task<IList<IPost>> GetAllItemsAsObjectAsync()
        {
            const int totalTasks = 100; // Number of tasks
            var tasks = new List<Task<IList<IPost>>>(totalTasks);

            // Log the start of the method
            Console.WriteLine($"{ORMName} | {TestName} - Starting GetAllItemsAsObjectAsync method...");

            for (int i = 0; i < totalTasks; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using (var localConn = new SqlConnection(conn.ConnectionString))
                        {
                            await localConn.OpenAsync(); // Open the connection asynchronously

                            // Log task start
                            Console.WriteLine($"{ORMName} | {TestName} - Task {Task.CurrentId} started...");

                            // Query the database and return the result
                            var posts = await localConn.QuerySqlAsync<Post>("SELECT * FROM Posts");

                            // Log the result retrieval
                            Console.WriteLine($"{ORMName} | {TestName} - Task {Task.CurrentId} retrieved data.");

                            // Convert the result to IList<IPost>
                            return (IList<IPost>)posts.Cast<IPost>().ToList(); // Cast each Post to IPost and return as IList<IPost>
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log any exceptions
                        Console.WriteLine($"{ORMName} | {TestName} - Task {Task.CurrentId} encountered an error: {ex.Message}");
                        return new List<IPost>(); // Return an empty list on error
                    }
                }));
            }

            // Log before waiting for tasks to complete
            Console.WriteLine($"{ORMName} | {TestName} - Waiting for all tasks to complete...");

            // Wait for all tasks to complete
            var results = await Task.WhenAll(tasks);

            // Log after all tasks are complete
            Console.WriteLine($"{ORMName} | {TestName} - All tasks completed.");

            // Aggregate results from all tasks
            var aggregatedResults = results.SelectMany(result => result).ToList();

            // Log the result
            Console.WriteLine(aggregatedResults.Any() ? "Results retrieved successfully." : "No results found.");

            // Log the end of the method
            Console.WriteLine($"{ORMName} | {TestName} - Ending GetAllItemsAsObjectAsync method.");

            return aggregatedResults;
        }

        public dynamic GetItemAsDynamic(int Id)
        {
            using (var localConn = new SqlConnection(conn.ConnectionString))
            {
                localConn.Open();
                var result = localConn.QuerySqlAsync<dynamic>("SELECT TOP 1 * FROM Posts WHERE Id=@Id", new { Id }).Result;
                return result;
            }
        }

        public async Task<IPost> GetItemAsObjectAsync(int Id)
        {
            const int totalTasks = 10; // Number of tasks

            // Log the start of the method
            Console.WriteLine($"{ORMName} | {TestName} - Starting GetItemAsObjectAsync method...");

            var tasks = new List<Task<IPost>>(totalTasks);

            // Start the tasks
            for (int i = 0; i < totalTasks; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using (var localConn = new SqlConnection(conn.ConnectionString))
                        {
                            await localConn.OpenAsync(); // Open the connection asynchronously

                            // Log task start
                            Console.WriteLine($"{ORMName} | {TestName} - Task {Task.CurrentId} started...");

                            // Use QuerySqlAsync for raw SQL query
                            var post = await localConn.QuerySqlAsync<Post>(
                                "SELECT * FROM Posts WHERE Id = @Id", new { Id });

                            // Log the result retrieval
                            Console.WriteLine($"{ORMName} | {TestName} - Task {Task.CurrentId} retrieved data.");

                            return (IPost)post.FirstOrDefault(); // Cast Post to IPost
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log any exceptions
                        Console.WriteLine($"{ORMName} | {TestName} - Task {Task.CurrentId} encountered an error: {ex.Message}");
                        return null;
                    }
                }));
            }

            // Log before waiting for tasks to complete
            Console.WriteLine($"{ORMName} | {TestName} - Waiting for all tasks to complete...");

            // Wait for all tasks to complete
            var results = await Task.WhenAll(tasks);

            // Log after all tasks are complete
            Console.WriteLine($"{ORMName} | {TestName} - All tasks completed.");

            // Log the result
            var firstResult = results.FirstOrDefault();
            Console.WriteLine(firstResult != null ? "Result retrieved successfully." : "No result found.");

            // Log the end of the method
            Console.WriteLine($"{ORMName} | {TestName} - Ending GetItemAsObjectAsync method.");

            // Return the first result (or aggregate results as needed)
            return firstResult;
        }

        public void Init(string connectionString)
        {
            conn = new SqlConnection(connectionString);
            SqlInsightDbProvider.RegisterProvider();
        }
    }
}