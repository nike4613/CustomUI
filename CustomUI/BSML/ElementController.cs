using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomUI.BSML
{
    public abstract class ElementController
    {

        private Element ownedElement;
        public Element OwnedElement
        {
            get => ownedElement;
            set
            {
                value.Controller = this;
                if (ownedElement != null)
                    ownedElement.Controller = null;

                ownedElement = value;
            }
        }

    }
}
