using System;
using System.Linq;
using System.Xml.Linq;
using ZumtenSoft.WebsiteCrawler.BusLayer.Utils;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration
{
    public static class ReportConfigSerializer
    {
        public static ReportConfig Clone(ReportConfig report)
        {
            return DeserializeReport(SerializeReport(report));
        }

        public static XElement SerializeReport(ReportConfig report)
        {
            return new XElement("ReportConfig",
                new XAttribute("Guid", report.Guid),
                new XAttribute("Name", report.Name),
                new XAttribute("Description", report.Description),
                new XAttribute("Type", report.Type),
                new XElement("Columns", report.Columns.Select(SerializeReportColumn)));
        }

        private static XElement SerializeReportColumn(ReportConfigColumn column)
        {
            return new XElement("Column",
                new XAttribute("Name", column.Name),
                new XAttribute("Path", column.Path),
                new XAttribute("Width", column.Width));
        }

        public static ReportConfig DeserializeReport(XElement element)
        {
            ReportConfig report = new ReportConfig
            {
                Guid = Guid.NewGuid(),
                Name = (string)element.Attribute("Name"),
                Description = (string)element.Attribute("Description"),
                Type = EnumHelper<ReportType>.Parse((string)element.Attribute("Type")),
            };

            report.Columns.AddRange(element.Elements("Columns").Elements().Select(DeserializeReportColumn));

            return report;
        }

        private static ReportConfigColumn DeserializeReportColumn(XElement element)
        {
            return new ReportConfigColumn
            {
                Name = (string)element.Attribute("Name"),
                Path = (string)element.Attribute("Path"),
                Width = (int)element.Attribute("Width")
            };
        }
    }
}
