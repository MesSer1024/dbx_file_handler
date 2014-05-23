using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace dbx_lib
{
    public class Class1
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

        public DbxType GetDbxType(string filePath)
        {
            var file = new FileInfo(filePath);
            return GetDbxType(file);
        }

        public DbxType GetDbxType(FileInfo file)
        {
            if (!file.Exists)
                throw new ArgumentException("File does not exist");

            findDbxTypeInFile(new StreamReader(file.FullName));
            return DbxType.unknown;
        }

        public class EarliestOccuranceSearchOptions
        {
            public FileInfo file;
            public string lineFilterToApplyRegexTo;
            public int maxLinesToRead;
        }

        public class SearchResult
        {
            public int lineNumber;
            public string line;
            public StringBuilder sb;
        }

        public SearchResult findEarliestOccurance(StreamReader sr, EarliestOccuranceSearchOptions options)
        {
            ML.Assert(options.file.Exists && !String.IsNullOrEmpty(options.lineFilterToApplyRegexTo));

            int lineNr = 0;
            StringBuilder sb = new StringBuilder();
            while (lineNr++ < options.maxLinesToRead)
            {
                var line = sr.ReadLine().Trim();
                sb.AppendLine(line);
                if (line.Contains(options.lineFilterToApplyRegexTo))
                {
                    return new SearchResult() { lineNumber = lineNr, line = line, sb = sb };
                }
            }
            return null;
        }
        public System.Xml.XmlDocument findXmlObjectContaining(string s)
        {
            return null;
        }

        public DbxType findDbxTypeInFile(StreamReader sr)
        {
            int lineNr = 0;


            //find out guid of "PrimaryInstance"
            //find the value of "PrimaryInstance"

            var assetGuid = "";
            var primaryInstanceGuid = "";
            bool hasPrimaryInstance = false;

            while (!sr.EndOfStream)
            {
                lineNr++;
                var line = sr.ReadLine();
                if (line.Contains("primaryInstance"))
                {
                    var regexGuid = new Regex("[gG]uid=\"{0}\"");
                    var regexPrimaryInstance = new Regex("[pP]rimaryInstance=\"{0}\"");

                    var guid = regexGuid.Match(line);
                    var primary = regexPrimaryInstance.Match(line);

                    if (!guid.Success || !primary.Success)
                        throw new Exception("Foobar");

                    assetGuid = guid.Value;
                    primaryInstanceGuid = primary.Value;
                    hasPrimaryInstance = true;
                }
                else if (hasPrimaryInstance && line.Contains(primaryInstanceGuid))
                {
                    //need to work with xml data
                }
            }

            throw new Exception("Foobar!");
            //return DbxType.unknown;
        }
    }
}
