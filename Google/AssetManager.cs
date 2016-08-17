using System;
using System.Collections.Generic;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using File = Google.Apis.Drive.v3.Data.File;

namespace MKP.Google {
    public class AssetManager {
        private string ConfigFile;
        private DriveService Service;
        private const string GoogleAppName = "mkp-mvc";
        private string[] Scopes = {DriveService.Scope.Drive};
        private string RootFolderID = "0B_SIV2COvToUMHZQLUx1RXlhcVU";           //not sure if this is correct or not, but we'll find out.

        public AssetManager(string ConfigFile) {
            this.ConfigFile = ConfigFile;
            Service = Authenticate();
        }

        public List<string> GetFolders() {
            var FolderList = new List<string>();
            var req = Service.Files.List();
            req.Q = "mimeType = 'application/vnd.google-apps.folder' and trashed = false and '" + RootFolderID + "' in parents";
            var list = req.Execute();

            foreach(var f in req.Execute().Files){
                FolderList.Add(f.Name);
            }
            
            return FolderList;
        }

        public List<File> GetImages(string Name) {
            var ImageList = new List<File>();

            var Folder = GetFolderByName(Name, RootFolderID);

            var req = Service.Files.List();
            req.Q = "mimeType = 'image/jpeg' and trashed = false and '" + Folder.Id + "' in parents";
            var list = req.Execute();

            foreach (var f in req.Execute().Files) {
                ImageList.Add(f);
            }

            return ImageList;
        }

        private File GetFolderByName(string Name, string ParentID) {
            var FolderList = new List<string>();
            var req = Service.Files.List();
            req.Q = "mimeType = 'application/vnd.google-apps.folder' and trashed = false and '" + ParentID + "' in parents and name contains '" + Name + "'";

            var folders = req.Execute().Files;

            if (folders.Count != 1) {
                return null;
            }
            else {
                return folders[0];
            }
        }


        private DriveService Authenticate() {
            try {
                using (var fs = new FileStream(ConfigFile, FileMode.Open, FileAccess.Read)) {
                    var credential = GoogleCredential.FromStream(fs);
                    credential = credential.CreateScoped(Scopes);

                    var svc = new DriveService(new BaseClientService.Initializer() {
                        HttpClientInitializer = credential,
                        ApplicationName = GoogleAppName
                    });
                    return svc;
                }
            }
            catch(Exception x){ 
                throw new Exception("Google Drive Service authentication failed... See inner exception for more details.");
            }
        }
    }
}
