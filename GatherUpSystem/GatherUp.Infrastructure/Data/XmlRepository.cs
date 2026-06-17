using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GatherUp.Core.Interfaces;

namespace GatherUp.Infrastructure.Data
{
    public class XmlRepository<T> : IRepository<T> where T : class, IEntity, new()
    {
        protected readonly string _filePath;

        public XmlRepository(string baseFolder)
        {
            if (!Directory.Exists(baseFolder))
                Directory.CreateDirectory(baseFolder);

            _filePath = Path.Combine(baseFolder, $"{typeof(T).Name}s.xml");
        }

        protected List<T> LoadAll()
        {
            if (!File.Exists(_filePath)) return new List<T>();
            return XMLSerializer.DeserializeFromXml<List<T>>(_filePath) ?? new List<T>();
        }

        protected void SaveAll(List<T> list) => XMLSerializer.SerializeToXml(_filePath, list);

        public void Add(T entity)
        {
            var list = LoadAll();
            list.Add(entity);
            SaveAll(list);
        }

        public T GetById(int id) =>
            LoadAll().FirstOrDefault(x => x.Id == id)
                ?? throw new KeyNotFoundException($"{typeof(T).Name} עם Id={id} לא נמצא");

        public IEnumerable<T> GetAll() => LoadAll();

        public void Update(T entity)
        {
            var list = LoadAll();
            var existing = list.FirstOrDefault(x => x.Id == entity.Id)
                ?? throw new KeyNotFoundException($"{typeof(T).Name} עם Id={entity.Id} לא נמצא");
            list.Remove(existing);
            list.Add(entity);
            SaveAll(list);
        }

        public void Delete(int id)
        {
            var list = LoadAll();
            var existing = list.FirstOrDefault(x => x.Id == id)
                ?? throw new KeyNotFoundException($"{typeof(T).Name} עם Id={id} לא נמצא");
            list.Remove(existing);
            SaveAll(list);
        }
    }
}
