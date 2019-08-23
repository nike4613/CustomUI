using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CustomUI.BSML
{
    internal class BSMLFile
    {
        private XmlDocument tree;

        private BSMLFile(XmlDocument tree)
        {
            this.tree = tree;
        }

        public static BSMLFile LoadFrom(TextReader reader)
        {
            var xml = new XmlDocument();
            xml.Load(reader);
            return new BSMLFile(xml);
        }



    }
}
