using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO.Compression;

namespace Common.Utility
{
    public static class ToolSnippet
    {
        public static Image CreateTextImage(String text, Font font, Color textColor, Color backColor)
        {
            //measure the string to see how big the image needs to be
            SizeF textSize = ToolSnippet.GetTextImageSize(text, font);


            //Create with 32bit
            Image img = new Bitmap((int)textSize.Width, (int)textSize.Height, PixelFormat.Format32bppPArgb);
            Graphics drawing = Graphics.FromImage(img);
            drawing.TextRenderingHint = TextRenderingHint.AntiAlias;
            drawing.Clear(backColor);

            Brush textBrush = new SolidBrush(textColor);
            drawing.DrawString(text, font, textBrush, 0, 0);
            drawing.Save();
            textBrush.Dispose();
            drawing.Dispose();

            return img;
        }

        public static SizeF GetTextImageSize(String text, Font font)
        {
            //!< by string property Pre-measure image size
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            img.Dispose();
            drawing.Dispose();
            return textSize;
        }

        public static void CopyRegionIntoImage(Bitmap srcBitmap, Rectangle srcRegion, ref Bitmap destBitmap, Rectangle destRegion)
        {
            using (Graphics grD = Graphics.FromImage(destBitmap))
            {
                grD.DrawImage(srcBitmap, destRegion, srcRegion, GraphicsUnit.Pixel);
            }
        }
        public static IPAddress GetExternalIpDyndns()
        {
            try
            {
                //!< myip.com 폐쇄, whatismyip.com api key를 위한 비용 필요
                //IPHostEntry ipEntry = Dns.GetHostByName(Dns.GetHostName());
                //IPAddress[] addr = ipEntry.AddressList;
                //!< vpn등에서 처리되지 않음

                string externalIP;
                externalIP = (new WebClient()).DownloadString("http://checkip.dyndns.com/");
                externalIP = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"))
                             .Matches(externalIP)[0].ToString();
                return IPAddress.Parse(externalIP);
            }
            catch { return null; }
        }

        public static void CsvUnescapeSplit(string path)
        {
            string[] seps = { "\",", ",\"" };
            char[] quotes = { '\"', ' ' };
            foreach (var line in File.ReadAllLines(path, Encoding.UTF8).Skip(1))
            {
                var fields = line
                    .Split(seps, StringSplitOptions.None)
                    .Select(s => s.Trim(quotes).Replace("\\\"", "\""))
                    .ToArray();
                foreach (var field in fields)
                    Console.Write("{0} | ", field);
                Console.WriteLine();
            }
        }

        public static bool ExtractZipArchive(string zipPath, string folderPath)
        {
            if (File.Exists(zipPath))
            {
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                try
                {
                    using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            entry.ExtractToFile(Path.Combine(folderPath, entry.FullName), true);
                        }
                    }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

    }
}
