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

        public string Name => "Insight Database Concurrency Stress Test";

        public void Finish()
        {
            conn.Close();
        }

        public IList<dynamic> GetAllItemsAsDynamic()
        {
            const int totalTasks = 100;
            const int progressBarWidth = 50;
            var tasks = new List<Task<IList<dynamic>>>(totalTasks);

            Console.Write("Progress: [");
            Console.CursorLeft = progressBarWidth + 1;

            var progress = new Progress<int>(percent =>
            {
                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                Console.Write("#");
            });

            for (int i = 0; i < totalTasks; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var result = conn.QuerySql<dynamic>("SELECT * FROM Posts").ToList();
                    ((IProgress<int>)progress).Report(i + 1);
                    return (IList<dynamic>)result; // Explicit cast to IList<dynamic>
                }));
            }

            Task.WhenAll(tasks).Wait();

            Console.Write("] Done\n");

            return tasks.SelectMany(t => t.Result).ToList();
        }

        public async Task<IList<IPost>> GetAllItemsAsObjectAsync()
        {
            const int totalTasks = 100; // Number of tasks
            const int progressBarWidth = 50; // Width of the progress bar
            var tasks = new List<Task<IList<IPost>>>(totalTasks);
            var progress = new Progress<int>(percent =>
            {
                // Update progress bar safely
                Console.SetCursorPosition(progressBarWidth + 12, Console.CursorTop);
                Console.Write(new string('#', percent));
            });

            // Print the progress bar header
            Console.Write("Progress: [");
            Console.Write(new string(' ', progressBarWidth)); // Initialize the progress bar with spaces
            Console.Write("]"); // Close the progress bar

            // Move cursor to start of the progress bar area
            Console.SetCursorPosition("Progress: [".Length, Console.CursorTop);

            for (int i = 0; i < totalTasks; i++)
            {
                // Run tasks with separate connections
                tasks.Add(Task.Run(() =>
                {
                    // Use a new connection for each task
                    using (var localConn = new SqlConnection(conn.ConnectionString))
                    {
                        localConn.Open();
                        // Query the database and return the result as IList<IPost>
                        var result = localConn.QuerySql<Post>("SELECT * FROM Posts").ToList();
                        return (IList<IPost>)result;
                    }
                }));
            }

            // Update progress as tasks complete
            for (int i = 0; i < totalTasks; i++)
            {
                var completedTask = await Task.WhenAny(tasks); // Wait for any task to complete
                tasks.Remove(completedTask); // Remove completed task from the list
                ((IProgress<int>)progress).Report((i + 1) * progressBarWidth / totalTasks); // Report progress
            }

            // Wait for all remaining tasks to complete
            await Task.WhenAll(tasks);

            // Move cursor to new line after progress bar
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"Progress: [{new string('#', progressBarWidth)}] Done\n"); // Complete the progress bar

            // Aggregate results from all tasks
            return tasks.SelectMany(t => t.Result).ToList();
        }

        public dynamic GetItemAsDynamic(int Id)
        {
            using (var localConn = new SqlConnection(conn.ConnectionString))
            {
                localConn.Open();
                var result = localConn.QuerySql<dynamic>("SELECT * FROM Posts WHERE Id=@Id", new { Id }).FirstOrDefault();
                return result;
            }
        }

        public async Task<IPost> GetItemAsObjectAsync(int Id)
        {
            const int totalTasks = 10; // Number of tasks
            const int progressBarWidth = 50; // Width of the progress bar

            var tasks = new List<Task<IPost>>(totalTasks);

            // Print the progress bar header with task count
            Console.Write($"Running {totalTasks} tasks... ");
            Console.Write("Progress: [");
            Console.Write(new string(' ', progressBarWidth)); // Fill the progress bar with spaces
            Console.Write("]"); // Close the progress bar

            // Move the cursor to the start of the progress bar area
            Console.SetCursorPosition(Console.CursorLeft - progressBarWidth, Console.CursorTop);

            // Start the tasks
            for (int i = 0; i < totalTasks; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    using (var localConn = new SqlConnection(conn.ConnectionString))
                    {
                        await localConn.OpenAsync(); // Open the connection asynchronously

                        // Query the database and return the result as IPost
                        var post = await localConn.QueryAsync<Post>("SELECT * FROM Posts WHERE Id = @Id", new { Id });
                        return (IPost)post.FirstOrDefault(); // Cast Post to IPost
                    }
                }));
            }

            // Monitor progress
            var progress = new Progress<int>(percent =>
            {
                // Update the progress bar
                Console.SetCursorPosition(Console.CursorLeft - progressBarWidth, Console.CursorTop);
                int filledWidth = percent * progressBarWidth / 100;
                Console.Write($"Progress: [");
                Console.Write(new string('#', filledWidth));
                Console.Write(new string(' ', progressBarWidth - filledWidth));
                Console.Write("]");
            });

            for (int i = 0; i < totalTasks; i++)
            {
                // Wait for a task to complete
                var completedTask = (Task<IPost>)await Task.WhenAny(tasks);
                tasks.Remove(completedTask);

                // Report progress
                int percentComplete = (i + 1) * 100 / totalTasks;
                ((IProgress<int>)progress).Report(percentComplete);
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            // Move cursor to a new line and print "Done"
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"Running {totalTasks} tasks... ");
            Console.Write("Progress: [");
            Console.Write(new string('#', progressBarWidth)); // Complete the progress bar
            Console.Write("] Done\n"); // Finish the progress

            // Example of returning the first result (or aggregate results as needed)
            return (await tasks.FirstOrDefault()) ?? null;
        }

        public void Init(string connectionString)
        {
            conn = new SqlConnection(connectionString);
            SqlInsightDbProvider.RegisterProvider();
        }
    }
}