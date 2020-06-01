using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CloudDisk
{
    public class XmlConfig
    {
        XmlDocument doc;
        public XmlConfig()
        {
            doc = new XmlDocument();
            if (!File.Exists(Environment.CurrentDirectory + "\\CloudDiskConfig.xml"))
            {
                XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", null);
                doc.AppendChild(dec);

                //创建一个根节点（一级）
                XmlElement root = doc.CreateElement("CloudDiskConfig");
                doc.AppendChild(root);

                //创建节点（二级）
                XmlNode node = doc.CreateElement("Cookies");
                /*XmlElement child = doc.CreateElement("Cookie");//子节点
                child.SetAttribute("BDUSS", "BDUSS");
                node.AppendChild(child);*/
                root.AppendChild(node);
                doc.Save(Environment.CurrentDirectory + "\\CloudDiskConfig.xml");
            }
            else
            {
                doc.Load(Environment.CurrentDirectory + "\\CloudDiskConfig.xml");
            }
        }

        public string GetChildNode(string attributeName, string rootNode = "CloudDiskConfig/Cookies")
        {
            XmlNode root = doc.SelectSingleNode(rootNode);
            foreach (XmlNode item in root.ChildNodes)
            {
                foreach (XmlNode itemAttributes in item.Attributes)
                {
                    if (itemAttributes.Name == attributeName)
                    {
                        return itemAttributes.Value;
                    }
                }
            }

            return null;
        }

        public void AddChildNode(string attributeName, string attributeVlue, string childNode = "Cookie", string rootNode = "CloudDiskConfig/Cookies")
        {
            bool repeat = false;
            XmlNode root = doc.SelectSingleNode(rootNode);
            foreach (XmlNode item in root.ChildNodes)
            {
                foreach (XmlNode itemAttributes in item.Attributes)
                {
                    if (itemAttributes.Name == attributeName)
                    {
                        itemAttributes.Value = attributeVlue;
                        repeat = true;
                        break;
                    }
                }
            }
            if (!repeat)
            {
                XmlElement child = doc.CreateElement(childNode);
                child.SetAttribute(attributeName, attributeVlue);
                root.AppendChild(child);
            }

            Save();
        }

        public void Save()
        {
            doc.Save(Environment.CurrentDirectory + "\\CloudDiskConfig.xml");
        }
    }
}
