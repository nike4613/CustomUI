using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomUI.BSML
{
    public enum AttributeType
    {
        Literal, InputBinding, OutputBinding
    }

    public class Attribute
    {
        public string Name { get; private set; }

    }
}
