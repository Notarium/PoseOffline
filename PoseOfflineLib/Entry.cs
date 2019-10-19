using System;
using System.Collections.Generic;
using System.Linq;

namespace PoseOfflineLib
{
    public class Entry
    {
        public int ID { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public int Counts { get; set; }
        public string SrcUriDir { get; set; }
        public string PreviewImgUri { get; set; }

    }
    public static class EntryHelper
    {
        public static string CombinePath(params string[] composition)
        {
            return composition.Aggregate("", (prev, next) => prev.Trim('/') + '/' + next.Trim('/')).Trim('/');
        }
        public static string[] PoseImgIDs(Entry entry)
        {
            return Enumerable.Range(1, entry.Counts).Select(i => $"{entry.ID.ToString(@"0000")}-{i.ToString(@"0000")}").ToArray();
        }
        public static string[] PoseImgFilenames(Entry entry)
        {
            return Enumerable.Range(1, entry.Counts).Select(i => $"pose_{i.ToString("0000")}.jpg").ToArray();
        }
        public static string PosePreviewID(Entry entry)
        {
            return entry.ID.ToString(@"0000") + "-" + "preview";
        }

        public static (string Id, string Name, Uri Uri)[] GetFileMeta(Entry entry)
        {
            var id = PoseImgIDs(entry);
            var fname = PoseImgFilenames(entry);
            var uri = PoseImgUris(entry);
            return Enumerable.Range(0, entry.Counts).Select(i => (id[i], fname[i], uri[i])).ToArray();
        }
        public static Uri PosePreviewUri(Entry entry)
        {
            return new Uri(CombinePath(entry.SrcUriDir, $"pose_0001_thumb.jpg"));
        }
        public static Uri[] PoseImgUris(Entry entry)
        {
            return Enumerable.Range(1, entry.Counts).Select(i => new Uri(CombinePath(entry.SrcUriDir, $"pose_{i.ToString(@"0000")}.jpg"))).ToArray();
        }
        public static bool SearchByTags(Entry e, string[] tags)
        {
            foreach (string t in tags)
            {
                if (!e.Tags.Contains(t))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
