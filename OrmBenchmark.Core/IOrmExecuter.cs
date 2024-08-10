using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrmBenchmark.Core
{
    public interface IOrmExecuter
    {
        string TestName { get; }

        string ORMName { get; }

        void Init(string connectionStrong);

        Task<IPost> GetItemAsObjectAsync(int Id)
        {
            throw new NotImplementedException();
        }

        IPost GetItemAsObject(int Id)
        {
            throw new NotImplementedException();
        }

        dynamic GetItemAsDynamic(int Id)
        {
            throw new NotImplementedException();
        }

        IList<IPost> GetAllItemsAsObject()
        {
            throw new NotImplementedException();
        }

        Task<IList<IPost>> GetAllItemsAsObjectAsync()
        {
            throw new NotImplementedException();
        }

        IList<dynamic> GetAllItemsAsDynamic()
        {
            throw new NotImplementedException();
        }

        Task MultipleInsertAsync(int numberOfRecords)
        {
            throw new NotImplementedException();
        }

        Task BulkUpdateAsync(int numberOfRecords)
        {
            throw new NotImplementedException();
        }

        Task BulkDeleteAsync(int numberOfRecords)
        {
            throw new NotImplementedException();
        }

        void Finish();
    }
}