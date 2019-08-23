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


        public IElement Parse()
        { // seperate because there are only a handful of valid top-level elements that should be processed


            return null;
        }
    }
}
