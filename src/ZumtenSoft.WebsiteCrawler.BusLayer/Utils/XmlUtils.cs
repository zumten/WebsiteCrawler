using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Utils
{
    public static class XmlUtils
    {
        public static void TransformXml(XDocument inputXml, FileInfo inputXslFile, FileInfo outputFile)
        {
            inputXml.Add(new XProcessingInstruction("xml-stylesheet", "type='text/xsl' href='hello.xsl'"));
            //<?xml-stylesheet type="text/xsl" href="Report.xsl"?>
            XslCompiledTransform xsl = new XslCompiledTransform();
            xsl.Load(inputXslFile.FullName);
            using (XmlWriter writer = XmlWriter.Create(outputFile.FullName))
            {
                xsl.Transform(inputXml.CreateReader(), writer);
            }
        }
    }
}
