

using System.Collections;
using System.Collections.Generic;
using CustomUI.BSML;

namespace Tests
{
    [ElementName("MyElement")]
    [ElementNamespace("bsml://tester")]
    public class CustomElement : Element
    {
        public Attribute[] Attributes { get; set; }

        public override void Initialize(Attribute[] attributes)
        {
            Attributes = attributes;
        }
    }

    public class MainPanelController
    {
        public object InBinding { get; set; }
        public object OutBinding { get; set; }
        public object InBindingElem { get; set; }
        public object OutBindingElem { get; set; }


        public Element Ref { get; set; }

        public class ListCell
        {

        }
    }
}