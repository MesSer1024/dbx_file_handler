using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using dbx_lib.assets;

namespace dbx_lib
{
    class DbxUtils
    {
        public static void updateAssetsRelatedToFile(FileInfo file, ref List<DiceAsset> identifiers, string lineFilter = "uid=")
        {
            ML.Assert(file.Exists);

            var references = new Dictionary<string, DiceAsset>();
            var fileAsset = AssetDatabase.containsAsset(file) ? AssetDatabase.getAsset(file) : DiceAsset.Create(file);

            using (var sr = new StreamReader(file.FullName))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line.Contains(lineFilter))
                    {
                        int n = identifiers.Count;
                        for (int i = 0; i < n; ++i)
                        {
                            var asset = identifiers[i];
                            if (asset != fileAsset && line.Contains(asset.PrimaryInstance))
                            {
                                fileAsset.addChildIfUnique(asset.Guid);
                                //asset.addParentIfUnique(fileAsset);
                            }
                        }
                    }
                }
            }
        }

        public static XmlElement buildXmlElement(string firstLine, StreamReader unreadData)
        {
            firstLine = firstLine.Trim();
            ML.Assert(firstLine[0] == '<');

            string xmlIdentifier = firstLine.Split(' ')[0];
            ML.Assert(xmlIdentifier[0] == '<');

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
    }
}
