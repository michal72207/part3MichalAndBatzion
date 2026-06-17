using System.Collections.Generic;

namespace GatherUp.Core.Interfaces
{
    public interface IRepository<T> where T : class, IEntity
    {
        void Add(T entity);
        T GetById(int id);
        IEnumerable<T> GetAll();
        void Update(T entity);
        void Delete(int id);
    }
}
