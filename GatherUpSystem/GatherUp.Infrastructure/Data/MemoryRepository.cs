using System;
using System.Collections.Generic;
using System.Linq;
using GatherUp.Core.Interfaces;

namespace GatherUp.Infrastructure.Data
{
    public class MemoryRepository<T> : IRepository<T> where T : class, IEntity
    {
        private readonly List<T> _data = new();

        public void Add(T entity) => _data.Add(entity);

        public T GetById(int id) =>
            _data.FirstOrDefault(x => x.Id == id)
                ?? throw new KeyNotFoundException($"{typeof(T).Name} עם Id={id} לא נמצא");

        public IEnumerable<T> GetAll() => _data;

        public void Update(T entity)
        {
            var existing = _data.FirstOrDefault(x => x.Id == entity.Id)
                ?? throw new KeyNotFoundException($"{typeof(T).Name} עם Id={entity.Id} לא נמצא");
            _data.Remove(existing);
            _data.Add(entity);
        }

        public void Delete(int id)
        {
            var existing = _data.FirstOrDefault(x => x.Id == id)
                ?? throw new KeyNotFoundException($"{typeof(T).Name} עם Id={id} לא נמצא");
            _data.Remove(existing);
        }
    }
}
