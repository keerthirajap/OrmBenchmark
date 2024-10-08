﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrmBenchmark.Core
{
    public class Benchmarker
    {
        private List<IOrmExecuter> executers { get; set; }
        public List<BenchmarkResult> results { get; set; }
        public List<BenchmarkResult> resultsForAllItems { get; set; }
        public List<BenchmarkResult> resultsForDynamicItem { get; set; }
        public List<BenchmarkResult> resultsForAllDynamicItems { get; set; }

        public List<BenchmarkResult> resultsForMultipleInsert { get; set; }

        public List<BenchmarkResult> resultsWarmUp { get; set; }
        private int IterationCount { get; set; }
        private string ConnectionString { get; set; }

        public Benchmarker(string connectionString, int iterationCount)
        {
            ConnectionString = connectionString;
            IterationCount = iterationCount;
            executers = new List<IOrmExecuter>();
            results = new List<BenchmarkResult>();
            resultsForDynamicItem = new List<BenchmarkResult>();
            resultsForAllItems = new List<BenchmarkResult>();
            resultsForAllDynamicItems = new List<BenchmarkResult>();
            resultsForMultipleInsert = new List<BenchmarkResult>();
            resultsWarmUp = new List<BenchmarkResult>();
        }

        public void RegisterOrmExecuter(IOrmExecuter executer)
        {
            executers.Add(executer);
        }

        public async Task Run(bool warmUp = false)
        {
            results.Clear();
            resultsForDynamicItem.Clear();
            resultsForAllItems.Clear();
            resultsForAllDynamicItems.Clear();
            resultsWarmUp.Clear();

            var rand = new Random();
            foreach (IOrmExecuter executer in executers.OrderBy(ignore => rand.Next()))
            {
                PrepareDatabase();
                Console.WriteLine($"\nRunning benchmark for {executer.TestName}...");
                executer.Init(ConnectionString);

                // Exception handling for each executer
                try
                {
                    // Bulk Insert
                    Stopwatch watchForMultipleInsertAsync = new Stopwatch();
                    watchForMultipleInsertAsync.Start();
                    await executer.MultipleInsertAsync(5000);
                    watchForMultipleInsertAsync.Stop();
                    resultsForMultipleInsert.Add(new BenchmarkResult { TestName = executer.TestName, ORMName = executer.ORMName, ExecTime = watchForMultipleInsertAsync.ElapsedMilliseconds });

                    // Warm-up
                    if (warmUp)
                    {
                        Stopwatch watchForWaemUp = new Stopwatch();
                        watchForWaemUp.Start();
                        await executer.GetItemAsObjectAsync(IterationCount + 1);
                        await executer.GetItemAsObjectAsync(IterationCount + 1);
                        watchForWaemUp.Stop();
                        resultsWarmUp.Add(new BenchmarkResult { TestName = executer.TestName, ORMName = executer.ORMName, ExecTime = watchForWaemUp.ElapsedMilliseconds });
                    }

                    // Object
                    Stopwatch watch = new Stopwatch();
                    long firstItemExecTime = 0;
                    for (int i = 1; i <= IterationCount; i++)
                    {
                        watch.Start();
                        var obj = await executer.GetItemAsObjectAsync(i);
                        watch.Stop();
                        //if (obj?.Id != i)
                        //    throw new ApplicationException("Invalid object returned.");
                        if (i == 1)
                            firstItemExecTime = watch.ElapsedMilliseconds;
                    }
                    results.Add(new BenchmarkResult { TestName = executer.TestName, ORMName = executer.ORMName, ExecTime = watch.ElapsedMilliseconds, FirstItemExecTime = firstItemExecTime });

                    // Dynamic
                    Stopwatch watchForDynamic = new Stopwatch();
                    firstItemExecTime = 0;
                    for (int i = 1; i <= IterationCount; i++)
                    {
                        watchForDynamic.Start();
                        var dynamicObj = executer.GetItemAsDynamic(i);
                        watchForDynamic.Stop();
                        //if (dynamicObj?.Id != i)
                        //    throw new ApplicationException("Invalid object returned.");
                        if (i == 1)
                            firstItemExecTime = watchForDynamic.ElapsedMilliseconds;
                    }
                    resultsForDynamicItem.Add(new BenchmarkResult { TestName = executer.TestName, ORMName = executer.ORMName, ExecTime = watchForDynamic.ElapsedMilliseconds, FirstItemExecTime = firstItemExecTime });

                    // All Objects
                    Stopwatch watchForAllItems = new Stopwatch();
                    watchForAllItems.Start();

                    var result = await executer.GetAllItemsAsObjectAsync();

                    watchForAllItems.Stop();
                    resultsForAllItems.Add(new BenchmarkResult { TestName = executer.TestName, ORMName = executer.ORMName, ExecTime = watchForAllItems.ElapsedMilliseconds });

                    // All Dynamics
                    Stopwatch watchForAllDynamicItems = new Stopwatch();
                    watchForAllDynamicItems.Start();
                    executer.GetAllItemsAsDynamic();
                    watchForAllDynamicItems.Stop();
                    resultsForAllDynamicItems.Add(new BenchmarkResult { TestName = executer.TestName, ORMName = executer.ORMName, ExecTime = watchForAllDynamicItems.ElapsedMilliseconds });

                    executer.Finish();
                }
                catch (Exception ex)
                {
                    //throw ex;
                    Console.WriteLine($"Error during {executer.TestName} benchmark: {ex.Message}");
                }
                finally
                {
                    // Force a garbage collection for all generations
                    GC.Collect();

                    // Wait for the finalizer queue to be processed
                    GC.WaitForPendingFinalizers();
                    Thread.Sleep(1000);
                }
            }
        }

        private void PrepareDatabase()
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    if (OBJECT_ID('Posts') is null)
                    begin
	                    create table Posts
	                    (
		                    Id int identity primary key,
		                    [Text] varchar(max) not null,
		                    CreationDate datetime not null,
		                    LastChangeDate datetime not null,
		                    Counter1 int,
		                    Counter2 int,
		                    Counter3 int,
		                    Counter4 int,
		                    Counter5 int,
		                    Counter6 int,
		                    Counter7 int,
		                    Counter8 int,
		                    Counter9 int
	                    )

	                    set nocount on

	                    declare @i int
	                    declare @c int
	                    declare @id int
	                    set @i = 0

	                    while @i <= 5001
	                    begin
		                    insert Posts ([Text], CreationDate, LastChangeDate) values (replicate('x', 2000), GETDATE(), GETDATE())
		                    set @id = @@IDENTITY

		                    set @i = @i + 1
	                    end
                    end";

                cmd.Connection = conn;
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }
    }
}