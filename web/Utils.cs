using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ByteSizeLib;

namespace web {
    public static class Utils {
        public const string ContainerNameDelimiter = "-date-";
        public const string DefaultMimeType = "image/jpg";

        public static string GetFileSizeString(double bytes) {
            var bs = ByteSize.FromBytes(bytes);


            //bytes
            if (bs.KiloBytes < 1)
            {
                return String.Format("{0} bytes", bytes.ToString("N1") );
            }

            //kilobytes
            if (bs.MegaBytes < 1) {
                return String.Format("{0}KB", bs.KiloBytes.ToString("N1"));
            }

            //megabytes
            if (bs.GigaBytes < 1) {
                return String.Format("{0}MB", bs.MegaBytes.ToString("N1"));
            }

            //gigabytes
            if (bs.TeraBytes < 1) {
                return String.Format("{0}MB", bs.GigaBytes.ToString("N1"));
            }

            return "unknown size";
        }

        public static string HumanizeAzureContainerName(string ContainerName) {
            var returnString = ContainerName.Replace(ContainerNameDelimiter, "|").Split('|')[0];
            return returnString;
        }
    }
}