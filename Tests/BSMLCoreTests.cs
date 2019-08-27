using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using CustomUI.BSML;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class BSMLCoreTests
    {
        private TestLogger logger;
        private CustomUI.Plugin plugin = new CustomUI.Plugin();

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            logger = new TestLogger(TestContext);
            plugin.Init(logger);

            BSML.RegisterTopLevelElement<PanelRootElement>();
            BSML.RegisterCustomElement<CustomElement>();
        }

        [TestCleanup]
        public void Deinitialize()
        {
            BSML.ResetGlobalState();
        }

        private void VerifyCustomElementStringAttributes(CustomUI.BSML.Attribute[] attrs, Type ogOwner)
        {
            var bindingTestObj = new MainPanelController();

            Assert.AreEqual(5, attrs.Length);

            var attr = attrs[0];

            Assert.AreEqual(AttributeType.Literal, attr.Type);
            Assert.AreEqual("literalAttr", attr.Name);
            Assert.AreEqual("", attr.NameSpace);
            Assert.AreEqual("ha, string!", attr.LiteralValue);
            Assert.AreEqual(ogOwner, attr.LinkedType);

            attr = attrs[1];

            Assert.AreEqual(AttributeType.InputBinding, attr.Type);
            Assert.AreEqual("inBindingAttr", attr.Name);
            Assert.AreEqual("", attr.NameSpace);
            Assert.AreEqual(typeof(object), attr.BindingType);
            Assert.AreEqual(ogOwner, attr.LinkedType);
            Assert.AreEqual("InBinding", attr.LiteralValue);
            bindingTestObj.InBinding = new object();
            Assert.AreEqual(bindingTestObj.InBinding, attr.BindingGetter(bindingTestObj));

            attr = attrs[2];

            Assert.AreEqual(AttributeType.OutputBinding, attr.Type);
            Assert.AreEqual("outBindingAttr", attr.Name);
            Assert.AreEqual("", attr.NameSpace);
            Assert.AreEqual(typeof(object), attr.BindingType);
            Assert.AreEqual(ogOwner, attr.LinkedType);
            Assert.AreEqual("OutBinding", attr.LiteralValue);
            var obj = new object();
            attr.BindingSetter(bindingTestObj, obj);
            Assert.AreEqual(obj, bindingTestObj.OutBinding);

            attr = attrs[3];

            Assert.AreEqual(AttributeType.SelfRef, attr.Type);
            Assert.AreEqual("ref", attr.Name);
            Assert.AreEqual(BSML.CoreNamespace, attr.NameSpace);
            Assert.AreEqual(typeof(Element), attr.BindingType);
            Assert.AreEqual(ogOwner, attr.LinkedType);
            Assert.AreEqual("Ref", attr.LiteralValue);
            obj = new CustomElement();
            attr.BindingSetter(bindingTestObj, obj);
            Assert.AreEqual(obj, bindingTestObj.Ref);

            attr = attrs[4];

            Assert.AreEqual(AttributeType.ElementAttribute, attr.Type);
            Assert.AreEqual("elementAttr", attr.Name);
            Assert.AreEqual("bsml://tester", attr.NameSpace);
            Assert.AreEqual(ogOwner, attr.LinkedType);
            Assert.AreEqual(null, attr.LiteralValue);

            attrs = attr.ElementAttributes;

            Assert.AreEqual(3, attrs.Length);

            attr = attrs[0];

            Assert.AreEqual(AttributeType.Literal, attr.Type);
            Assert.AreEqual("literalAttrElem", attr.Name);
            Assert.AreEqual("", attr.NameSpace);
            Assert.AreEqual("ha, string! Elem", attr.LiteralValue);
            Assert.AreEqual(ogOwner, attr.LinkedType);

            attr = attrs[1];

            Assert.AreEqual(AttributeType.InputBinding, attr.Type);
            Assert.AreEqual("inBindingAttrElem", attr.Name);
            Assert.AreEqual("", attr.NameSpace);
            Assert.AreEqual(typeof(object), attr.BindingType);
            Assert.AreEqual(ogOwner, attr.LinkedType);
            Assert.AreEqual("InBindingElem", attr.LiteralValue);
            bindingTestObj.InBindingElem = new object();
            Assert.AreEqual(bindingTestObj.InBindingElem, attr.BindingGetter(bindingTestObj));

            attr = attrs[2];

            Assert.AreEqual(AttributeType.OutputBinding, attr.Type);
            Assert.AreEqual("outBindingAttrElem", attr.Name);
            Assert.AreEqual("", attr.NameSpace);
            Assert.AreEqual(typeof(object), attr.BindingType);
            Assert.AreEqual(ogOwner, attr.LinkedType);
            Assert.AreEqual("OutBindingElem", attr.LiteralValue);
            obj = new object();
            attr.BindingSetter(bindingTestObj, obj);
            Assert.AreEqual(obj, bindingTestObj.OutBindingElem);
        }

        [TestMethod]
        public void AttributesParsing()
        {
            var bsml = BSMLParser.LoadFrom(Assembly.GetExecutingAssembly(), "Tests", new StringReader(Literals.CustomElementBSML));

            var doc = bsml.Doc;

            var ogOwner = typeof(MainPanelController);
            var state = new BSMLParser.ParseState { Ref = new MainPanelController(), Type = ogOwner };
            var attrs = bsml.GetAttributes(doc.DocumentElement.FirstChild as XmlElement, ref state, out var hasController).ToArray();

            Assert.IsFalse(hasController);

            VerifyCustomElementStringAttributes(attrs, ogOwner);
        }

        private void VerifyCustomElementStringRootChildren(Element[] elems, Type owner)
        {
            Assert.AreEqual(1, elems.Length);

            var elem = elems[0];

            Assert.IsInstanceOfType(elem, typeof(CustomElement));

            var ce = elem as CustomElement;

            VerifyCustomElementStringAttributes(ce.Attributes, owner);
        }

        [TestMethod]
        public void ElementParsing()
        {
            var bsml = BSMLParser.LoadFrom(Assembly.GetExecutingAssembly(), "Tests", new StringReader(Literals.CustomElementBSML));

            var doc = bsml.Doc;

            var owner = typeof(MainPanelController);
            var state = new BSMLParser.ParseState { Ref = new MainPanelController(), Type = owner };

            var elems = bsml.ReadTree(new[] { doc.DocumentElement.FirstChild }, state).ToArray();

            VerifyCustomElementStringRootChildren(elems, owner);
        }

        [TestMethod]
        public void RootParsing()
        {
            var bsml = BSMLParser.LoadFrom(Assembly.GetExecutingAssembly(), "Tests", new StringReader(Literals.CustomElementBSML));

            var elem = bsml.Parse();
        }
    }
}
