using WebShoppingOnline.Models;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace WebShoppingOnline.Repository
{
    public interface IObjects
    {
        void Add<T>(T obj) where T: class;
        void Update<T>(T obj) where T : class;
        void Delete<T>(String id);
        T Get<T>(String id) where T : class;
        IEnumerable<T> GetAll<T>() where T : class;

        string SetId<T>(string prefix, Expression<Func<T, string>> keySelector) where T: class;
    }
}
