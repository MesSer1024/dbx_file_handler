using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace dbx_lib
{
    public class LibMain
    {
        public enum DbxType
        {
            unknown,
            SoundConfigurationAsset,
            SoundPatchConfigurationAsset,
            UIWidget,
        }

        public FileInfo[] GetDbxFiles(string path)
        {
            var dir = new DirectoryInfo(path);
            if (!dir.Exists)
                throw new ArgumentException("Folder does not exist");

            return dir.GetFiles("*.dbx");
        }

        public DiceAsset GetDiceAsset(string filePath)
        {
            var file = new FileInfo(filePath);
            return GetDiceAsset(file);
        }

        public DiceAsset GetDiceAsset(FileInfo file)
        {
            if (!file.Exists)
                throw new ArgumentException("File does not exist");

            var asset = findDbxAssetInfo(new StreamReader(file.FullName));
            asset.FilePath = file.FullName;
            return asset;
        }

        //public class EarliestOccuranceSearchOptions
        //{
        //    public FileInfo file;
        //    public string lineFilterToApplyRegexTo;
        //    public int maxLinesToRead;
        //}

        //public class SearchResult
        //{
        //    public int lineNumber;
        //    public string line;
        //    public StringBuilder sb;
        //}

        //public SearchResult findEarliestOccurance(StreamReader sr, EarliestOccuranceSearchOptions options)
        //{
        //    ML.Assert(options.file.Exists && !String.IsNullOrEmpty(options.lineFilterToApplyRegexTo));

        //    int lineNr = 0;
        //    StringBuilder sb = new StringBuilder();
        //    while (lineNr++ < options.maxLinesToRead)
        //    {
        //        var line = sr.ReadLine().Trim();
        //        sb.AppendLine(line);
        //        if (line.Contains(options.lineFilterToApplyRegexTo))
        //        {
        //            return new SearchResult() { lineNumber = lineNr, line = line, sb = sb };
        //        }
        //    }
        //    return null;
        //}

        private DiceAsset findDbxAssetInfo(StreamReader sr)
        {
            DiceAsset asset = new DiceAsset();
            int lineNr = 0;

            bool hasPrimaryInstance = false;
            bool parsedPrimaryInstance = false;

            while (!sr.EndOfStream)
            {
                lineNr++;
                var line = sr.ReadLine();
                if (line.Contains("primaryInstance"))
                {
                    var regexGuid = new Regex("[gG]uid=\"([A-Za-z0-9-]*)\"");
                    var regexPrimaryInstance = new Regex("[pP]rimaryInstance=\"([A-Za-z0-9-]*)\"");

                    var guid = regexGuid.Match(line);
                    var primary = regexPrimaryInstance.Match(line);

                    if (!guid.Success || !primary.Success)
                        throw new Exception("Foobar");
                    if (guid.Groups.Count < 2 || primary.Groups.Count < 2)
                        throw new Exception("foobar!");

                    asset.Guid = guid.Groups[1].Value;
                    asset.PrimaryInstance = primary.Groups[1].Value;

                    hasPrimaryInstance = true;
                }
                else if (hasPrimaryInstance && line.Contains(asset.PrimaryInstance))
                {
                    var xml = makeNearestXmlObject(line, sr);
                    if (xml.HasAttribute("type") && xml.HasAttribute("id"))
                    {
                        asset.Type = xml.GetAttribute("type");
                        asset.Name = xml.GetAttribute("id");
                    }
                    else
                        throw new Exception("Foobar");
                    parsedPrimaryInstance = true;
                    break;
                }
            }

            if (parsedPrimaryInstance)
            {
                return asset;
            }
            else
                throw new Exception("Foobar!");
            //return DbxType.unknown;
        }

        private XmlElement makeNearestXmlObject(string line, StreamReader unreadData)
        {
            line = line.Trim();
            ML.Assert(line[0] == '<');

            string xmlIdentifier = line.Split(' ')[0];
            ML.Assert(xmlIdentifier[0] == '<');

            var sb = new StringBuilder();
            sb.AppendLine(line);

            bool success = false;
            if (!line.EndsWith("/>"))
            {
                xmlIdentifier = xmlIdentifier.Remove(0, 1);
                var xmlEndTag = "</" + xmlIdentifier;

                while (!unreadData.EndOfStream)
                {
                    var readLine = unreadData.ReadLine();
                    sb.AppendLine(readLine);
                    if (readLine.Contains(xmlEndTag))
                    {
                        success = true;
                        break;
                    }
                }
            }

            if (!success)
                throw new Exception("foobar!");

            var xml = new XmlDocument();
            xml.LoadXml(sb.ToString());
            return xml.DocumentElement;
        }
    }
}
