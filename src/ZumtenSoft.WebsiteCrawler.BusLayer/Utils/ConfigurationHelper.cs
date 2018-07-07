using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Utils
{
    public static class ConfigurationHelper
    {
        const string CRAWLING_CONFIG_FILE = "CrawlingConfigs.xml";
        const string REPORT_CONFIG_FILE = "ReportConfigs.xml";

        public static FileInfo GetFile(string name)
        {
            string directoryName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return new FileInfo(Path.Combine(directoryName ?? ".", name));
        }

        public static ICollection<CrawlingConfig> LoadCrawlingConfigs()
        {
            FileInfo file = GetFile(CRAWLING_CONFIG_FILE);

            List<CrawlingConfig> configs = new List<CrawlingConfig>();
            if (file != null && file.Exists)
            {
                XDocument document = XDocument.Load(file.FullName);
                foreach (XElement configElem in document.Elements("CrawlingConfigs").Elements("CrawlingConfig"))
                    configs.Add(CrawlingConfigurationSerializer.DeserializeConfiguration(configElem));
            }

            return configs;
        }

        public static void SaveCrawlingConfigs(ICollection<CrawlingConfig> configs)
        {
            FileInfo file = GetFile(CRAWLING_CONFIG_FILE);

            XElement root = new XElement("CrawlingConfigs",
                from config in configs
                select CrawlingConfigurationSerializer.SerializeConfiguration(config));

            root.Save(file.FullName);
        }

        public static ICollection<ReportConfig> LoadReportConfigurations()
        {
            FileInfo file = GetFile(REPORT_CONFIG_FILE);

            List<ReportConfig> configs = new List<ReportConfig>();
            if (file.Exists)
            {
                XDocument document = XDocument.Load(file.FullName);
                foreach (XElement configElem in document.Elements("ReportConfigs").Elements("ReportConfig"))
                    configs.Add(ReportConfigSerializer.DeserializeReport(configElem));
            }

            return configs;
        }

        public static void SaveReportConfigs(ICollection<ReportConfig> configs)
        {
            FileInfo file = GetFile(REPORT_CONFIG_FILE);

            XElement root = new XElement("ReportConfigs",
                from config in configs
                select ReportConfigSerializer.SerializeReport(config));

            root.Save(file.FullName);
        }
    }
}
