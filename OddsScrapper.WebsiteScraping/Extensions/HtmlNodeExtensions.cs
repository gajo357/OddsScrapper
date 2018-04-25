using HtmlAgilityPack;
using OddsScrapper.WebsiteScraping.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace OddsScrapper.WebsiteScrapping.Extensions
{
    public static class HtmlNodeExtensions
    {
        public static double GetOddFromTdNode(this HtmlNode tdNode)
        {
            if (double.TryParse(tdNode.FirstChild.InnerText, out double odd))
                return odd;

            return double.NaN;
        }

        public static bool ContainsAttribute(this HtmlNode node, string attributeName)
        {
            return !string.IsNullOrEmpty(node.GetAttributeValue(attributeName, null));
        }

        public static bool AttributeContains(this HtmlNode node, string attributeName, string value)
        {
            return node.GetAttributeValue(attributeName, string.Empty).Contains(value);
        }

        public static IEnumerable<HtmlNode> WithName(this IEnumerable<HtmlNode> nodes, string name)
        {
            return nodes.Where(s => s.Name == name);
        }

        public static IEnumerable<HtmlNode> WithAttributeContains(this IEnumerable<HtmlNode> nodes, string attributeName, string value)
        {
            return nodes.Where(s => s.AttributeContains(attributeName, value));
        }

        public static IEnumerable<HtmlNode> WithAttribute(this IEnumerable<HtmlNode> nodes, string attributeName)
        {
            return nodes.Where(s => s.ContainsAttribute(attributeName));
        }

        public static string ReadGameLink(this IEnumerable<HtmlNode> tds)
        {
            var nameTd = tds.WithAttributeContains(HtmlAttributes.Class, "table-participant").First();
            var nameElement = nameTd.Elements(HtmlTagNames.A).First();

            var gameLink = nameElement.Attributes[HtmlAttributes.Href].Value;

            return gameLink;
        }

        public static bool HasGameFinished(this IEnumerable<HtmlNode> tds)
        {
            return tds
                .WithAttributeContains(HtmlAttributes.Class, "odds-nowrp")
                .WithAttributeContains(HtmlAttributes.Class, "result-ok")
                .Any();
        }
    }
}
