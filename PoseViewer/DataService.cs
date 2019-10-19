using PoseOfflineLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PoseViewer
{
    internal class DataService
    {
        public DBService _db;

        public DataService()
        {
            _db = new DBService("Filename=posedb");
            _db.CheckIDoID();
        }

        public IEnumerable<DisplayableEntry> Query(string tag)
        {
            var rslt = _db.Query(new[] { tag });
            List<DisplayableEntry> ret = new List<DisplayableEntry>();
            foreach (var rs in rslt)
            {
                if (rs.Tags.Contains("model"))
                {
                    continue;
                }
                ret.Add(new DisplayableEntry(rs));
            }
            return ret;
        }

        public async Task<IEnumerable<DisplayableEntry>> QueryAsync(string tag)
        {
            return await Task.Run(() => Query(tag));
        }

        public void ExtendQuery(IEnumerable<DisplayableEntry> de)
        {
            foreach (var d in de)
            {
                d.LoadPreview(_db.FileDownloadStream(EntryHelper.PosePreviewID(d)));
            }
        }

        public async Task ExtendedQueryAsync(IEnumerable<DisplayableEntry> de)
        {
            await Task.Run(() =>
            {
                ExtendQuery(de);
            });
        }
        
    }
}