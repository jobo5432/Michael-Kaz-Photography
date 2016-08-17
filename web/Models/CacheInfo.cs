using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using MKP.Google;

namespace web.Models
{
    public class CacheInfo : IDisposable {
        public DateTime LastUpdated { get; set;}
        public List<FolderInfo> Folders { get; set; }

        private string _ConfigPath = "";

        public CacheInfo(string ConfigPath) {
            _ConfigPath = ConfigPath;
        }

        public void Dispose() {
            this.Folders = null;
        }

        public void UpdateCacheConfig()
        {
            try {
                var s = DateTime.Now.ToShortDateString();
                var t = DateTime.Now.ToShortTimeString();
                var json = "{\"lastUpdated\": \"" + s + " " + t + "\"}";
                File.WriteAllText(_ConfigPath, json);
            }
            catch (Exception x) {
                //log it
                    //not implemented
                //throw it.
                throw;
            }
        }

    }
}