using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrmBenchmark.Core
{
    public interface IOrmExecuter
    {
        string Name { get; }

        void Init(string connectionStrong);

        Task<IPost> GetItemAsObjectAsync(int Id)
        {
            throw new NotImplementedException();
        }

        IPost GetItemAsObject(int Id)
        {
            throw new NotImplementedException();
        }

        dynamic GetItemAsDynamic(int Id);

        IList<IPost> GetAllItemsAsObject()
        {
            throw new NotImplementedException();
        }

        Task<IList<IPost>> GetAllItemsAsObjectAsync()
        {
            throw new NotImplementedException();
        }

        IList<dynamic> GetAllItemsAsDynamic();

        void Finish();
    }
}