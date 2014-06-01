using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using dbx_lib.assets;

namespace dbx_lib
{
    class DbxUtils
    {
        public const int GUID_LENGTH = 36;

        public static XmlElement buildXmlElement(string firstLine, StreamReader unreadData)
        {
            firstLine = firstLine.Trim();

            string xmlIdentifier = firstLine.Split(' ')[0];

            var sb = new StringBuilder();
            sb.AppendLine(firstLine);

            bool success = false;
            if (!firstLine.EndsWith("/>"))
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

        public static string findSubstring(string source, string identifier, int count)
        {
            var idx = source.IndexOf(identifier);
            return source.Substring(idx + identifier.Length, count);
        }

        public static string findSubstring(string source, string startIdentifier, string endIdentifier)
        {
            var startIdx = source.IndexOf(startIdentifier) + startIdentifier.Length;
            var endIdx = source.IndexOf(endIdentifier, startIdx + 1);
            return source.Substring(startIdx, endIdx - startIdx);
        }

        public static string parseRegexGroup(string line, string regexPattern, int regexGroup)
        {
            var regexGuid = new Regex(regexPattern, RegexOptions.Compiled);
            var guid = regexGuid.Match(line);

            if (!guid.Success)
                throw new Exception("Foobar");
            if (regexGroup >= guid.Groups.Count)
                throw new Exception("foobar!");

            return guid.Groups[regexGroup].Value;
        }

        public static string findReferencedGuid(string line)
        {
            line = findSubstring(line, "ref=\"", " ");
            return line.Substring(line.LastIndexOf('/') + 1, GUID_LENGTH);
        }

        public static string GetProperDirectoryCapitalization(DirectoryInfo dirInfo) {
            DirectoryInfo parentDirInfo = dirInfo.Parent;
            if (null == parentDirInfo)
                return dirInfo.Name;
            return Path.Combine(GetProperDirectoryCapitalization(parentDirInfo),
                                parentDirInfo.GetDirectories(dirInfo.Name)[0].Name);
        }

        public static string GetProperFilePathCapitalization(string filename) {
            FileInfo fileInfo = new FileInfo(filename);
            DirectoryInfo dirInfo = fileInfo.Directory;
            return Path.Combine(GetProperDirectoryCapitalization(dirInfo),
                                dirInfo.GetFiles(fileInfo.Name)[0].Name);
        }
    }
}
