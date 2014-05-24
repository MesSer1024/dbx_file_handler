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
        public FileInfo[] GetDbxFiles(string searchFolder)
        {
            var dir = new DirectoryInfo(searchFolder);
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

            var asset = buildAsset(new StreamReader(file.FullName));
            asset.FilePath = file.FullName;
            return asset;
        }

        private DiceAsset buildAsset(StreamReader sr)
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
                    var regexGuid = new Regex("[gG]uid=\"([A-Za-z0-9-]*)\"", RegexOptions.Compiled);
                    var regexPrimaryInstance = new Regex("[pP]rimaryInstance=\"([A-Za-z0-9-]*)\"", RegexOptions.Compiled);

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
