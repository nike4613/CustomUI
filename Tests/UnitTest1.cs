using System;
using System.IO;
using System.Reflection;
using CustomUI.BSML;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        public const string BSML = @"
<!-- This would be in the embedded file 'BSIPA_ModList.UI.MainPanel.bsml' or something -->
<bs:panel xmlns:bs=""bsml://beat-saber-markup-core"" back-button=""true"" bs:controller=""MainPanelController"">
	<bs:horizontal-stack align=""center"" >

        <!-- id is basically only for the reader -->
		<bs:list bs:id=""modList"" scrollable=""true"" selected=""{selectedCell=}"" bs:ref=""{modTable=}"">
			<!-- '.' in controller specifies subtype to main controller

                 all fields in the definition reference that instead    -->
			<bs:list.cell template=""list-cell"" bs:controller="".ListCell""> 
				<!-- Should also be able to define the template inline by leaving out the `template` property -->
				<icon><bs:controller-value>icon</bs:controller-value></icon>
				<!-- Content references use the same format, but without the brackets -->
				<text><bs:controller-value>title</bs:controller-value></text>
				<subtext>
					<bs:controller-value>version</bs:controller-value> 
					<bs:font size=""75%"" ><bs:controller-value > author </bs:controller-value ></bs:font >

                </subtext >

            </bs:list.cell >

            <bs:list.entries >

                <bs:list.cell > <!-- Can define entries inline -->
					<text>Cell text override</text>
					<subtext>Cell subtext override</subtext>
				</bs:list.cell>
			</bs:list.entries>
		</bs:list>
		<!-- Custom elements are named as their filename, with preceding dots interpreted as namespacing -->
		<ModInfoElement bs:id=""customElement"" modCell=""{=selectedCell}"" visibility=""{=modInfoVisibility}"" />
	</bs:horizontal-stack>
</bs:panel>
";

        [TestMethod]
        public void BSMLAttributes()
        {
            var bsml = BSMLParser.LoadFrom(Assembly.GetExecutingAssembly(), "Tests", new StringReader(BSML));

            var doc = bsml.Doc;

            Type owner = null;
            var attrs = bsml.GetAttributes(doc.DocumentElement, ref owner);
        }
    }

    public class MainPanelController
    {
        public class ListCell
        {

        }
    }
}
