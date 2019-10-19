using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

using HtmlAgilityPack;
using PoseOfflineLib;
using Newtonsoft.Json;
using System.Windows;

namespace PoseOffline
{
    class NetService
    {
        private readonly HttpClient _client;

        public DBService _dBService;

        private string _HostAddr = "http://www.posemaniacs.com";
        public Action<string> OnProgressChanged;

        private bool _stopFlag;
        public void CallStop()
        {
            _stopFlag = true;
        }

        private void Reset()
        {
            _stopFlag = false;
        }

        public NetService()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.AutomaticDecompression = DecompressionMethods.All;
            _client = new HttpClient(handler);
            _dBService = new DBService("Filename=posedb");
        }

        public async Task<Result> Analyse()
        {
            string frontpage = await GetString(new Uri(_HostAddr));
            HtmlDocument hd = new HtmlDocument();
            hd.LoadHtml(frontpage);
            List<int> pagimation = hd.DocumentNode.Descendants().Where(n => n.GetClasses().Contains("page")).Select(n => int.Parse(n.InnerText)).ToList();
            pagimation.Sort();
            int pages = pagimation.Last();
            int errCounter = 0;
            List<string> errmsg = new List<string>();
            OnProgressChanged?.Invoke($"[{pages}]pages found");
            List<Entry> Entrys = new List<Entry>();
            for(int i = 1; i <= pages; i++)
            {
                string page = await GetString(new Uri(EntryHelper.CombinePath(_HostAddr, "page", i.ToString())));
                Entrys.AddRange(ExtractEntry(page));
                OnProgressChanged?.Invoke($"[{i}/{pages}]pages done");
            }
            int idx = 1;
            
            foreach(var e in Entrys)
            {
                await ExtendEntry(e);
                Thread.Sleep(50);
                try
                {
                    _dBService.Insert(e);
                }catch(Exception exc)
                {
                    errCounter += 1;
                    errmsg.Add(exc.Message);
                }
                OnProgressChanged?.Invoke($"[{idx}/{Entrys.Count}]pages done");
                idx++;
            }
            OnProgressChanged?.Invoke("Ready");
            Result ret = new Result
            {
                PageCount = pages,
                ErrorEntry = errCounter,
                ErrorMessages = errmsg,
            };
            return ret;
        }

        internal void CleanAll(int v)
        {
            _dBService.CleanAll(v);
        }

        public IEnumerable<Entry> QueryAll()
        {
            return _dBService.GetAll();
        }

        public IEnumerable<Entry> ExtractEntry(string page)
        {
            HtmlDocument hd = new HtmlDocument();
            hd.LoadHtml(page);
            var lists = hd.DocumentNode.Descendants().Where(n => n.GetClasses().Contains("list")).ToList();
            List<Entry> entrys = new List<Entry>();
            foreach (var l in lists)
            {
                foreach (var a in l.Descendants("a"))
                {
                    var entry = new Entry()
                    {
                        ID = int.Parse(a.Attributes["href"].Value.Trim('/').Split('/').Last()),
                        PreviewImgUri = EntryHelper.CombinePath(_HostAddr, a.FirstChild.Attributes["src"].Value),
                    };
                    entrys.Add(entry);
                }
            }
            return entrys;
        }
        public async Task ExtendEntry(Entry entry)
        {
            entry.Counts = 36;
            string page = await GetString(new Uri(EntryHelper.CombinePath(_HostAddr, "archives", entry.ID.ToString())));
            HtmlDocument hd = new HtmlDocument();
            hd.LoadHtml(page);
            var tags = hd.DocumentNode.Descendants("a").Where(n=>n.Attributes.Contains("rel")).Where(n => n.Attributes["rel"].Value == "category tag").Select(n => n.InnerText).ToList();
            entry.Tags = tags;
        }

        public async Task<Stream> GetFileStream(Uri uri)
        {

            return await (await GetResponse(uri)).Content.ReadAsStreamAsync();
        }

        public async Task<string> GetString(Uri uri)
        {
            
            return await (await GetResponse(uri)).Content.ReadAsStringAsync();
        }
        public async Task<HttpResponseMessage> GetResponse(Uri uri)
        {
            HttpResponseMessage res = null;
            int counter = 0;
            while(true)
            {
                res = await _client.GetAsync(uri);
                if (!res.IsSuccessStatusCode)
                {
                    counter++;
                    Thread.Sleep(50);
                }
                else
                {
                    break;
                }

                if(counter >= 5)
                {
                    throw new Exception("Unable to access:" + uri.ToString() + "." + "ErrCode:" + res.StatusCode);
                }
            }
            return res;
        }
        public bool Update(Entry entry)
        {
            return _dBService.Update(entry);
        }
        public string Backup()
        {
            IEnumerable<Entry> qrslt = _dBService.GetAll();
            return JsonConvert.SerializeObject(qrslt);
        }
        public void RestoreFromString(string json)
        {
            IEnumerable<Entry> arr = JsonConvert.DeserializeObject<IEnumerable<Entry>>(json);
            foreach(var a in arr)
            {
                _dBService.Insert(a);
            }
        }
        public async Task UploadFiles()
        {
            IEnumerable<Entry> collection = _dBService.GetAll();
            int count = collection.Count();
            int ide = 1;
            foreach(var entry in collection)
            {
                if (_stopFlag)
                {
                    MessageBox.Show("Aborting...");
                    Reset();
                    return;
                }
                if (entry.Tags.Contains("model"))
                {
                    OnProgressChanged?.Invoke($"[{ide}/{count}]Skipping");
                    ide++;
                    continue;
                }
                string pvid = EntryHelper.PosePreviewID(entry);
                Uri pvuri = EntryHelper.PosePreviewUri(entry);
                
                using (Stream stream = await GetFileStream(pvuri))
                {
                    _dBService.FileUpload(pvid, "pose_0001_thumb.jpg", stream);
                }
                OnProgressChanged?.Invoke($"[{ide}/{count}]-PrevImgSaved");
                (string Id, string Name, Uri Uri)[] meta = EntryHelper.GetFileMeta(entry);
                int dix = 1;
                foreach(var trip in meta)
                {
                    if (_stopFlag)
                    {
                        MessageBox.Show("Aborting...");
                        Reset();
                        return;
                    }
                    if (_dBService.IsFileExisit(trip.Id))
                    {
                        OnProgressChanged?.Invoke($"[{ide}/{count}]:[{dix}/{entry.Counts}]:Downloading");
                        dix++;
                        continue;
                    }
                    
                    using (Stream stream = await GetFileStream(trip.Uri))
                    {
                        var r = _dBService.FileUpload(trip.Id, trip.Name, stream);
                        if(r != trip.Id)
                        {
                            throw new Exception("upload failed?[" + trip.Id + "]");
                        }
                    }
                    OnProgressChanged?.Invoke($"[{ide}/{count}]:[{dix}/{entry.Counts}]:Downloading");
                    dix++;
                }
                OnProgressChanged?.Invoke($"[{ide}/{count}]Processing");
                ide++;
            }
            OnProgressChanged?.Invoke("Ready");
        }
    }
}
