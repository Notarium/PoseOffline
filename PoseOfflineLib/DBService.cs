using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiteDB;
using System.IO;
using System.Threading.Tasks;

namespace PoseOfflineLib
{
    class IndexIndex
    {
        public int Id { get; set; }
        public int IdOfEntry { get; set; }
    }
    public class DBService
    {
        private readonly LiteDatabase _db;
        private LiteCollection<Entry> _entrys;
        private LiteStorage _storage;
        private Random _rand;
        private int _entryCount;
        private const string _STR_ENTRY_COLLECTION_NAME = "entrys";
        private const string _STR_INDEX_OF_INDEX = "idoid";
        public DBService(string connstr)
        {
            _db = new LiteDatabase(connstr);
            _entrys = _db.GetCollection<Entry>(_STR_ENTRY_COLLECTION_NAME);
            _storage = _db.FileStorage;
            _entryCount = _entrys.Count();
            _rand = new Random();
        }
        public void CheckIDoID()
        {
            if (!_db.CollectionExists(_STR_INDEX_OF_INDEX))
            {
                UpdateIndexIndex();
            }
        }
        public int InsertRange(IEnumerable<Entry> sets)
        {
            return _entrys.Insert(sets);
        }
        public int Insert(Entry entry)
        {
            return _entrys.Insert(entry);
        }
        public IEnumerable<Entry> GetAll() => _entrys.FindAll();
        public IEnumerable<Entry> Query(string[] tags)
        {
            return _entrys.Find(en => EntryHelper.SearchByTags(en, tags));
        }
        public Entry Get(int id)
        {
            return _entrys.Find(e => e.ID == id).FirstOrDefault();
        }
        public bool Drop(int id)
        {
            return _entrys.Delete(id);
        }

        public string FileUpload(string id, string fname, Stream filestream)
        {
            var res = _storage.Upload(id, fname, filestream);
            return res.Id;
        }
        public Stream FileDownloadStream(string id)
        {
            return _storage.OpenRead(id);
        }

        public bool IsFileExisit(string id)
        {
            return _storage.Exists(id);
        }
        public async Task<byte[]> FileDownloadByte(string id)
        {
            byte[] ret;
            using (MemoryStream ms = new MemoryStream())
            {
                using (LiteFileStream stm = _storage.OpenRead(id))
                {
                    await stm.CopyToAsync(ms);
                }
                ret = new byte[ms.Length];
                await ms.ReadAsync(ret, 0, (int)ms.Length);
            }
            return ret;
        }
        public bool Update(Entry entry)
        {
            return _entrys.Update(entry);
        }
        public void CleanAll(int codedede)
        {
            if (codedede == 217654389)
            {
                _db.DropCollection(_STR_ENTRY_COLLECTION_NAME);
                _entrys = _db.GetCollection<Entry>(_STR_ENTRY_COLLECTION_NAME);
            }
        }
        public void UpdateIndexIndex()
        {
            LiteCollection<IndexIndex> idoid = _db.GetCollection<IndexIndex>(_STR_INDEX_OF_INDEX);
            var coll = _entrys.FindAll();
            foreach (var co in coll)
            {
                idoid.Insert(new IndexIndex()
                {
                    IdOfEntry = co.ID
                });
            }
        }

        public Stream Random()
        {
            Entry entry = null;
            while (true)
            {
                var rand = _rand.Next(0, _entryCount);
                var idoentry = _db.GetCollection<IndexIndex>(_STR_INDEX_OF_INDEX).FindById(rand).IdOfEntry;
                entry = _entrys.FindById(idoentry);
                if (!entry.Tags.Contains("model"))
                {
                    break;
                }
            }
            int inrand = _rand.Next(0, 36);
            return FileDownloadStream(EntryHelper.PoseImgIDs(entry)[inrand]);
        }
    }
}
