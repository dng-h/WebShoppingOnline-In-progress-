using System.Linq.Expressions;
using WebShoppingOnline.Models;

namespace WebShoppingOnline.Repository
{
    public class ObjectRepo : IObjects
    {
        private readonly DbShoppingContext _context;
        public ObjectRepo(DbShoppingContext context)
        {
            _context = context;
        }
        public void Add<T>(T obj) where T: class
        {
            _context.Set<T>().Add(obj);
            _context.SaveChanges();
        }

        public void Delete<T>(string id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetAll<T>() where T : class
        {
            return _context.Set<T>();
        }

        public T Get<T>(string id) where T: class
        {
            return _context.Set<T>().Find(id);
        }

        public void Update<T>(T obj) where T: class
        {
            _context.Set<T>().Update(obj);
            _context.SaveChanges();
        }

        public string SetId<T>(string prefix, Expression<Func<T, string>> keySelector) where T: class
        {
            var lastId = _context.Set<T>()
                        .OrderByDescending(keySelector)
                        .Select(keySelector)
                        .FirstOrDefault();

            if (!string.IsNullOrEmpty(lastId))
            {
                string numberPart = lastId.Substring(2);

                int newNumber = int.Parse(numberPart) + 1;

                return prefix + $"{newNumber:D10}";
            }
            return prefix + "0000000001";
        }
    }
}
