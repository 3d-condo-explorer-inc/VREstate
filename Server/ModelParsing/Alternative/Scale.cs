﻿using System.Xml;
using System;
using System.IO;

namespace Vre.Server.Model.Kmz
{
    public class Scale
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Scale(XmlNode root)
        {
            const string XNodeName = "x";
            const string YNodeName = "y";
            const string ZNodeName = "z";

            XmlNode node = root.FirstChild;
            while (node != null)
            {
                if (node.Name.Equals(XNodeName)) X = double.Parse(node.InnerText);
                else if (node.Name.Equals(YNodeName)) Y = double.Parse(node.InnerText);
                else if (node.Name.Equals(ZNodeName)) Z = double.Parse(node.InnerText);

                node = node.NextSibling;
            }
        }
    }
}