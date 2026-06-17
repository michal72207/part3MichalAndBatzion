using System;
using System.IO;
using System.Linq;
using GatherUp.Core.DO;
using GatherUp.Core.Interfaces;

namespace GatherUp.Infrastructure.Data
{
    public class ReceiptRepository : IRepository<Receipt>
    {
        private readonly XmlRepository<Receipt> _xmlRepo;
        private readonly string _receiptsStorageFolder;

        public ReceiptRepository(string baseFolder, string receiptsStorageFolder)
        {
            if (!Directory.Exists(receiptsStorageFolder))
                Directory.CreateDirectory(receiptsStorageFolder);

            _xmlRepo = new XmlRepository<Receipt>(baseFolder);
            _receiptsStorageFolder = receiptsStorageFolder;
        }

        // שומר קובץ הקבלה פיזית ומוסיף רשומה ל-XML
        public void Add(Receipt receipt)
        {
            if (File.Exists(receipt.FilePath))
            {
                string fileName  = Path.GetFileName(receipt.FilePath);
                string targetPath = Path.Combine(_receiptsStorageFolder, fileName);
                File.Copy(receipt.FilePath, targetPath, overwrite: true);
                receipt.FilePath = targetPath;
            }

            _xmlRepo.Add(receipt);
        }

        public Receipt GetById(int id) =>
            _xmlRepo.GetById(id) ?? throw new KeyNotFoundException($"קבלה {id} לא נמצאה");

        public System.Collections.Generic.IEnumerable<Receipt> GetAll() => _xmlRepo.GetAll();

        public void Update(Receipt receipt) => _xmlRepo.Update(receipt);

        public void Delete(int id) => _xmlRepo.Delete(id);
    }
}
