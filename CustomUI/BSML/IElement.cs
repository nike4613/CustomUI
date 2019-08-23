using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CustomUI.BSML
{
    // interface for all custom elements, generated or not
    public interface IElement
    {
        void Initialize(Attribute[] attributes);

    }
}
