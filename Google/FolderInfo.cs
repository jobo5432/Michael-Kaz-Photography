using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MKP.Google;

namespace MKP.Google {
    public class FolderInfo {
        public string Name { get; set; }
        public List<Image> Images { get; set; }
        public bool IsHomepage { get; set; }
    }
}
