using Faker;
using FizzWare.NBuilder;
using Insight.Database;
using OrmBenchmark.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace OrmBenchmark.InsightDatabase
{
    public class BulkOperationsPerformanceTest : IOrmExecuter
    {
        private SqlConnection conn;

        public string TestName => "Bulk Operations Performance Test";
        public string ORMName => "Insight Database";

        public void Finish()
        {
            conn.Close();
        }

        public async Task MultipleInsertAsync(int numberOfRecords)
        {
            var posts = GeneratePosts(numberOfRecords);

            // Measure the performance of bulk insert
            var startTime = DateTime.Now;

            foreach (var post in posts)
            {
                Console.WriteLine($"{ORMName} | {TestName} - MultipleInsertAsync method... {post.Id}");

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        conn.ExecuteSql(
            "INSERT INTO Posts (Text, CreationDate, LastChangeDate, Counter1, Counter2, Counter3, Counter4, Counter5, Counter6, Counter7, Counter8, Counter9) VALUES (@Text, @CreationDate, @LastChangeDate, @Counter1, @Counter2, @Counter3, @Counter4, @Counter5, @Counter6, @Counter7, @Counter8, @Counter9)",
            new
            {
                post.Text,
                post.CreationDate,
                post.LastChangeDate,
                post.Counter1,
                post.Counter2,
                post.Counter3,
                post.Counter4,
                post.Counter5,
                post.Counter6,
                post.Counter7,
                post.Counter8,
                post.Counter9
            },
            transaction: tx
        );

                        tx.Commit();
                    }
                    catch (Exception)
                    {
                        tx.Commit();
                    }
                }
            }

            var endTime = DateTime.Now;
            Console.WriteLine($"{ORMName} | {TestName} - Bulk Insert of {numberOfRecords} records took: {endTime - startTime}");
        }

        public async Task BulkUpdateAsync(int numberOfRecords)
        {
            var posts = GeneratePosts(numberOfRecords);

            // Measure the performance of bulk update
            var startTime = DateTime.Now;

            using (var transaction = await conn.OpenWithTransactionAsync())
            {
                foreach (var post in posts)
                {
                    await conn.ExecuteAsync(
                        "UPDATE Posts SET Text = @Text WHERE Id = @Id",
                        new { post.Text, post.Id });
                }
                transaction.Commit();
            }

            var endTime = DateTime.Now;
            Console.WriteLine($"{ORMName} | {TestName} - Bulk Update of {numberOfRecords} records took: {endTime - startTime}");
        }

        public async Task BulkDeleteAsync(int numberOfRecords)
        {
            var ids = GeneratePostIds(numberOfRecords);

            // Measure the performance of bulk delete
            var startTime = DateTime.Now;

            using (var transaction = await conn.OpenWithTransactionAsync())
            {
                foreach (var id in ids)
                {
                    await conn.ExecuteAsync(
                        "DELETE FROM Posts WHERE Id = @Id",
                        new { Id = id });
                }
                transaction.Commit();
            }

            var endTime = DateTime.Now;
            Console.WriteLine($"{ORMName} | {TestName} - Bulk Delete of {numberOfRecords} records took: {endTime - startTime}");
        }

        private List<Post> GeneratePosts(int count)
        {
            return Builder<Post>.CreateListOfSize(count)
                .All()
                .With(p => p.Text = Lorem.Sentence())
                .With(p => p.CreationDate = DateTime.UtcNow)
                .With(p => p.LastChangeDate = DateTime.UtcNow)
                .With(p => p.Counter1 = RandomNumber.Next(1, 100))
                .With(p => p.Counter2 = RandomNumber.Next(1, 100))
                .With(p => p.Counter3 = RandomNumber.Next(1, 100))
                .With(p => p.Counter4 = RandomNumber.Next(1, 100))
                .With(p => p.Counter5 = RandomNumber.Next(1, 100))
                .With(p => p.Counter6 = RandomNumber.Next(1, 100))
                .With(p => p.Counter7 = RandomNumber.Next(1, 100))
                .With(p => p.Counter8 = RandomNumber.Next(1, 100))
                .With(p => p.Counter9 = RandomNumber.Next(1, 100))
                .Build()
                .ToList();
        }

        private IEnumerable<int> GeneratePostIds(int numberOfRecords)
        {
            return Enumerable.Range(1, numberOfRecords);
        }

        public void Init(string connectionString)
        {
            conn = new SqlConnection(connectionString);
            conn.Open();  // Ensure the connection is opened
            SqlInsightDbProvider.RegisterProvider();
        }
    }
}