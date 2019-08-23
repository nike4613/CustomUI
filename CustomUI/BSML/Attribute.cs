using FastExpressionCompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

using static System.Linq.Expressions.Expression;

namespace CustomUI.BSML
{
    public enum AttributeType
    {
        Literal = 0x1,
        Binding = 0x2,
        InputBinding = Binding | 0x4,
        OutputBinding = Binding | 0x8,
        FunctionBinding = Binding | 0x10,
        SelfRef = OutputBinding | 0x1,

        ElementAttribute = 0x20
    }

    public class Attribute
    {
        public string NameSpace { get; private set; }
        public string Name { get; private set; }

        public AttributeType Type { get; private set; }

        public string LiteralValue => "";

        public delegate object GetBinding(object target);
        public delegate void SetBinding(object target, object value);

        public Type LinkedType { get; private set; }
        public Type BindingType { get; private set; }

        public GetBinding BindingGetter { get; private set; }
        public SetBinding BindingSetter { get; private set; }


        public MethodInfo FunctionBinding { get; private set; }

        public Attribute[] ElementAttributes { get; private set; }

        public IEnumerable<IElement> ElementContent { get; private set; }

        private static Regex functionBinding = new Regex(@"^([\.a-zA-Z0-9_]+)\((.*)\)$", RegexOptions.Compiled);

        private string value;

        private static Dictionary<Tuple<Type, string>, Tuple<Type, GetBinding, SetBinding>> bindingCache = new Dictionary<Tuple<Type, string>, Tuple<Type, GetBinding, SetBinding>>();

        internal Attribute(XmlAttribute attr, Type connectedType)
        {
            Name = attr.LocalName;
            NameSpace = attr.NamespaceURI;
            LinkedType = connectedType;
            value = attr.Value.Trim();

            if (value[0] == '{' && value[value.Length - 1] == '}')
                Type = AttributeType.Binding;
            if (Type == AttributeType.Binding)
            {
                var trimmed = value.Substring(1, value.Length - 2).Trim();

                if (trimmed[0] == '=') Type = AttributeType.InputBinding;
                if (trimmed[trimmed.Length - 1] == '=') Type = AttributeType.OutputBinding;

                Match reMatch;
                if (Type == AttributeType.InputBinding || Type == AttributeType.OutputBinding)
                {
                    value = trimmed.Trim('=').Trim();

                    if (Type == AttributeType.InputBinding)
                    {
                        var propExpr =
                            PropertyOrField(
                                Convert(
                                    Parameter(typeof(object), "self"),
                                    connectedType
                                ),
                                value
                            );

                        BindingType = propExpr.Type;

                        BindingGetter = Lambda<GetBinding>(
                                Convert(propExpr, typeof(object)),
                                Parameter(typeof(object), "self")
                            ).CompileFast();
                    }

                    if (Type == AttributeType.OutputBinding)
                    {
                        var propExpr =
                            PropertyOrField(
                                Convert(
                                    Parameter(typeof(object), "self"),
                                    connectedType
                                ),
                                value
                            );

                        BindingType = propExpr.Type;

                        BindingSetter = Lambda<SetBinding>(
                                Assign(
                                    Convert(propExpr, typeof(object)),
                                    Convert(
                                        Parameter(typeof(object), "value"),
                                        BindingType
                                    )
                                ),
                                Parameter(typeof(object), "self"),
                                Parameter(typeof(object), "value")
                            ).CompileFast();
                    }

                    if (Type == AttributeType.OutputBinding && NameSpace == BSML.CoreNamespace && Name == "ref")
                    {
                        Type = AttributeType.SelfRef;
                    }
                }
                else if ((reMatch = functionBinding.Match(trimmed)).Value != "")
                {
                    value = trimmed;

                }
            }

            if (NameSpace == BSML.CoreNamespace && Name == "ref" && Type != AttributeType.SelfRef)
                throw new InvalidProgramException("'ref' parameter MUST be an OutputBinding");
        }
    }
}
