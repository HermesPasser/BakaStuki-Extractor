using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace BakaTsukiExtractor.util
{
    public static class XmlUtility
    {

        public static IEnumerable<XmlNode> Concat(this XmlNodeList list1, XmlNodeList list2)
        {
            return list1.Cast<XmlNode>().Concat<XmlNode>(list2.Cast<XmlNode>());
        }

        public static void RemoveNodesByXpath(this XmlNode refNode, string xpath)
        {
            XmlNodeList nodes = refNode.SelectNodes(xpath);
            foreach (XmlNode node in nodes)
                node.ParentNode.RemoveChild(node);
        }
    }
}
