using CustomUI.BSML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public static class Literals
    {
        public const string BSML_ = @"
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
        public const string CustomElementBSML = @"
<bs:panel back-button='true' bs:controller='Tests.MainPanelController'
        xmlns:bs='" + BSML.CoreNamespace + @"'
        xmlns:test='bsml://tester'>
    <test:MyElement literalAttr='ha, string!' inBindingAttr='{=InBinding}' outBindingAttr='{OutBinding=}' bs:ref='{Ref=}'>
        <test:MyElement.elementAttr literalAttrElem='ha, string! Elem' inBindingAttrElem='{=InBindingElem}' outBindingAttrElem='{OutBindingElem=}'>
            <test:MyElement.elementAttr.notAnAttr></test:MyElement.elementAttr.notAnAttr>
        </test:MyElement.elementAttr>
    </test:MyElement>
</bs:panel>
";
    }
}
