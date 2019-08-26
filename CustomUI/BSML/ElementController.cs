using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomUI.BSML
{
    public abstract class ElementController
    {

        public Element OwnedElement { get; internal set; }

    }
}
