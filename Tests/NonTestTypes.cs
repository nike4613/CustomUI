

using CustomUI.BSML;

namespace Tests
{
    [ElementName("MyElement")]
    [ElementNamespace("bsml://tester")]
    public class CustomElement : IElement
    {
        public void Initialize(Attribute[] attributes)
        {

        }
    }

    public class MainPanelController
    {
        public object InBinding { get; set; }
        public object OutBinding { get; set; }
        public object InBindingElem { get; set; }
        public object OutBindingElem { get; set; }


        public IElement Ref { get; set; }

        public class ListCell
        {

        }
    }
}