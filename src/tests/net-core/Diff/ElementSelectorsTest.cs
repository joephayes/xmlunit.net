/*
  This file is licensed to You under the Apache License, Version 2.0
  (the "License"); you may not use this file except in compliance with
  the License.  You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
*/

using System.Xml;
using NUnit.Framework;

namespace Org.XmlUnit.Diff {

    [TestFixture]
    public class ElementSelectorsTest {
        private const string FOO = "foo";
        private const string BAR = "bar";
        private const string SOME_URI = "urn:some:uri";

        private XmlDocument doc;

        [SetUp] public void CreateDoc() {
            doc = new XmlDocument();
        }

        private void PureElementNameComparisons(ElementSelector s) {
            XmlElement control = doc.CreateElement(FOO);
            XmlElement equal = doc.CreateElement(FOO);
            XmlElement different = doc.CreateElement(BAR);
            XmlElement controlNS = doc.CreateElement(BAR, FOO, SOME_URI);

            Assert.IsFalse(s(null, null));
            Assert.IsFalse(s(null, control));
            Assert.IsFalse(s(control, null));
            Assert.IsTrue(s(control, equal));
            Assert.IsFalse(s(control, different));
            Assert.IsFalse(s(control, controlNS));
            Assert.IsTrue(s(doc.CreateElement(FOO, SOME_URI), controlNS));
        }

        [Test] public void ByName() {
            PureElementNameComparisons(ElementSelectors.ByName);
        }

        [Test] public void ByNameAndText_NamePart() {
            PureElementNameComparisons(ElementSelectors.ByNameAndText);
        }

        private void ByNameAndText_SingleLevel(ElementSelector s) {
            XmlElement control = doc.CreateElement(FOO);
            control.AppendChild(doc.CreateTextNode(BAR));
            XmlElement equal = doc.CreateElement(FOO);
            equal.AppendChild(doc.CreateTextNode(BAR));
            XmlElement equalC = doc.CreateElement(FOO);
            equalC.AppendChild(doc.CreateCDataSection(BAR));
            XmlElement noText = doc.CreateElement(FOO);
            XmlElement differentText = doc.CreateElement(FOO);
            differentText.AppendChild(doc.CreateTextNode(BAR));
            differentText.AppendChild(doc.CreateTextNode(BAR));

            Assert.IsTrue(s(control, equal));
            Assert.IsTrue(s(control, equalC));
            Assert.IsFalse(s(control, noText));
            Assert.IsFalse(s(control, differentText));
        }

        [Test] public void ByNameAndText() {
            ByNameAndText_SingleLevel(ElementSelectors.ByNameAndText);
        }

        [Test] public void ByNameAndTextRec_NamePart() {
            PureElementNameComparisons(ElementSelectors.ByNameAndTextRec);
        }

        [Test] public void ByNameAndTextRec_Single() {
            ByNameAndText_SingleLevel(ElementSelectors.ByNameAndTextRec);
        }

        [Test] public void ByNameAndTextRec() {
            XmlElement control = doc.CreateElement(FOO);
            XmlElement child = doc.CreateElement(BAR);
            control.AppendChild(child);
            child.AppendChild(doc.CreateTextNode(BAR));
            XmlElement equal = doc.CreateElement(FOO);
            XmlElement child2 = doc.CreateElement(BAR);
            equal.AppendChild(child2);
            child2.AppendChild(doc.CreateTextNode(BAR));
            XmlElement equalC = doc.CreateElement(FOO);
            XmlElement child3 = doc.CreateElement(BAR);
            equalC.AppendChild(child3);
            child3.AppendChild(doc.CreateCDataSection(BAR));
            XmlElement noText = doc.CreateElement(FOO);
            XmlElement differentLevel = doc.CreateElement(FOO);
            differentLevel.AppendChild(doc.CreateTextNode(BAR));
            XmlElement differentElement = doc.CreateElement(FOO);
            XmlElement child4 = doc.CreateElement(FOO);
            differentElement.AppendChild(child4);
            child4.AppendChild(doc.CreateTextNode(BAR));
            XmlElement differentText = doc.CreateElement(FOO);
            XmlElement child5 = doc.CreateElement(BAR);
            differentText.AppendChild(child5);
            child5.AppendChild(doc.CreateTextNode(FOO));

            ElementSelector s = ElementSelectors.ByNameAndTextRec;
            Assert.IsTrue(s(control, equal));
            Assert.IsTrue(s(control, equalC));
            Assert.IsFalse(s(control, noText));
            Assert.IsFalse(s(control, differentLevel));
            Assert.IsFalse(s(control, differentElement));
            Assert.IsFalse(s(control, differentText));
        }

        [Test] public void ByNameAndAllAttributes_NamePart() {
            PureElementNameComparisons(ElementSelectors.ByNameAndAllAttributes);
        }

        [Test] public void ByNameAndAllAttributes() {
            XmlElement control = doc.CreateElement(FOO);
            control.SetAttribute(BAR, BAR);
            XmlElement equal = doc.CreateElement(FOO);
            equal.SetAttribute(BAR, BAR);
            XmlElement noAttributes = doc.CreateElement(FOO);
            XmlElement differentValue = doc.CreateElement(FOO);
            differentValue.SetAttribute(BAR, FOO);
            XmlElement differentName = doc.CreateElement(FOO);
            differentName.SetAttribute(FOO, FOO);
            XmlElement differentNS = doc.CreateElement(FOO);
            differentNS.SetAttribute(BAR, SOME_URI, BAR);

            Assert.IsTrue(ElementSelectors.ByNameAndAllAttributes(control,
                                                                  equal));
            Assert.IsFalse(ElementSelectors.ByNameAndAllAttributes(control,
                                                                   noAttributes));
            Assert.IsFalse(ElementSelectors.ByNameAndAllAttributes(noAttributes,
                                                                   control));
            Assert.IsFalse(ElementSelectors.ByNameAndAllAttributes(control,
                                                                   differentValue));
            Assert.IsFalse(ElementSelectors.ByNameAndAllAttributes(control,
                                                                   differentName));
            Assert.IsFalse(ElementSelectors.ByNameAndAllAttributes(control,
                                                                   differentNS));
        }

        [Test] public void ByNameAndAttributes_NamePart() {
            PureElementNameComparisons(ElementSelectors
                                       .ByNameAndAttributes(new string[] {}));
            PureElementNameComparisons(ElementSelectors
                                       .ByNameAndAttributes(new XmlQualifiedName[] {}));
            PureElementNameComparisons(ElementSelectors.ByNameAndAttributes(BAR));
            PureElementNameComparisons(ElementSelectors
                                       .ByNameAndAttributes(new XmlQualifiedName(BAR,
                                                                                 SOME_URI)));
        }

        [Test] public void ByNameAndAttributes_String() {
            XmlElement control = doc.CreateElement(FOO);
            control.SetAttribute(BAR, BAR);
            XmlElement equal = doc.CreateElement(FOO);
            equal.SetAttribute(BAR, BAR);
            XmlElement noAttributes = doc.CreateElement(FOO);
            XmlElement differentValue = doc.CreateElement(FOO);
            differentValue.SetAttribute(BAR, FOO);
            XmlElement differentName = doc.CreateElement(FOO);
            differentName.SetAttribute(FOO, FOO);
            XmlElement differentNS = doc.CreateElement(FOO);
            differentNS.SetAttribute(BAR, SOME_URI, BAR);

            Assert.IsTrue(ElementSelectors.ByNameAndAttributes(BAR)(control,
                                                                    equal));
            Assert.IsFalse(ElementSelectors.ByNameAndAttributes(BAR)(control,
                                                                     noAttributes));
            Assert.IsTrue(ElementSelectors.ByNameAndAttributes(FOO)(control,
                                                                    noAttributes));
            Assert.IsTrue(ElementSelectors.ByNameAndAttributes(new string[] {})
                          (control, noAttributes));
            Assert.IsFalse(ElementSelectors.ByNameAndAttributes(BAR)(noAttributes,
                                                                     control));
            Assert.IsFalse(ElementSelectors.ByNameAndAttributes(BAR)(control,
                                                                     differentValue));
            Assert.IsFalse(ElementSelectors.ByNameAndAttributes(BAR)(control,
                                                                     differentName));
            Assert.IsFalse(ElementSelectors.ByNameAndAttributes(BAR)(control,
                                                                     differentNS));
        }

        [Test] public void byNameAndAttributes_QName() {
            XmlElement control = doc.CreateElement(FOO);
            control.SetAttribute(BAR, BAR);
            XmlElement equal = doc.CreateElement(FOO);
            equal.SetAttribute(BAR, BAR);
            XmlElement noAttributes = doc.CreateElement(FOO);
            XmlElement differentValue = doc.CreateElement(FOO);
            differentValue.SetAttribute(BAR, FOO);
            XmlElement differentName = doc.CreateElement(FOO);
            differentName.SetAttribute(FOO, FOO);
            XmlElement differentNS = doc.CreateElement(FOO);
            differentNS.SetAttribute(BAR, SOME_URI, BAR);

            Assert.IsTrue(ElementSelectors.ByNameAndAttributes(new XmlQualifiedName(BAR))
                          (control, equal));
            Assert.IsFalse(ElementSelectors.ByNameAndAttributes(new XmlQualifiedName(BAR))
                           (control, noAttributes));
            Assert.IsTrue(ElementSelectors.ByNameAndAttributes(new XmlQualifiedName(FOO))
                          (control, noAttributes));
            Assert.IsTrue(ElementSelectors.ByNameAndAttributes(new XmlQualifiedName[] {})
                          (control, noAttributes));
            Assert.IsFalse(ElementSelectors.ByNameAndAttributes(new XmlQualifiedName(BAR))
                           (noAttributes, control));
            Assert.IsFalse(ElementSelectors.ByNameAndAttributes(new XmlQualifiedName(BAR))
                           (control, differentValue));
            Assert.IsFalse(ElementSelectors.ByNameAndAttributes(new XmlQualifiedName(BAR))
                           (control, differentName));
            Assert.IsFalse(ElementSelectors.ByNameAndAttributes(new XmlQualifiedName(BAR))
                           (control, differentNS));
        }

        [Test] public void ByNameAndAttributesControlNS_NamePart() {
            PureElementNameComparisons(ElementSelectors
                                       .ByNameAndAttributesControlNS());
            PureElementNameComparisons(ElementSelectors
                                       .ByNameAndAttributesControlNS(BAR));
        }

        [Test] public void ByNameAndAttributesControlNS() {
            XmlElement control = doc.CreateElement(FOO);
            control.SetAttribute(BAR, SOME_URI, BAR);
            XmlElement equal = doc.CreateElement(FOO);
            equal.SetAttribute(BAR, SOME_URI, BAR);
            XmlElement noAttributes = doc.CreateElement(FOO);
            XmlElement differentValue = doc.CreateElement(FOO);
            differentValue.SetAttribute(BAR, SOME_URI, FOO);
            XmlElement differentName = doc.CreateElement(FOO);
            differentName.SetAttribute(FOO, SOME_URI, FOO);
            XmlElement differentNS = doc.CreateElement(FOO);
            differentNS.SetAttribute(BAR, SOME_URI + "2", BAR);
            XmlElement noNS = doc.CreateElement(FOO);
            noNS.SetAttribute(BAR, BAR);

            Assert.IsTrue(ElementSelectors.ByNameAndAttributesControlNS(BAR)
                          (control, equal));
            Assert.IsFalse(ElementSelectors.ByNameAndAttributesControlNS(BAR)
                           (control, noAttributes));
            Assert.IsTrue(ElementSelectors.ByNameAndAttributesControlNS(FOO)
                          (control, noAttributes));
            Assert.IsTrue(ElementSelectors.ByNameAndAttributesControlNS()
                          (control, noAttributes));
            Assert.IsTrue(ElementSelectors.ByNameAndAttributesControlNS(BAR)
                          (noAttributes, control));
            Assert.IsFalse(ElementSelectors.ByNameAndAttributesControlNS(BAR)
                           (noAttributes, noNS));
            Assert.IsFalse(ElementSelectors.ByNameAndAttributesControlNS(BAR)
                           (control, differentValue));
            Assert.IsFalse(ElementSelectors.ByNameAndAttributesControlNS(BAR)
                           (control, differentName));
            Assert.IsFalse(ElementSelectors.ByNameAndAttributesControlNS(BAR)
                           (control, differentNS));
            Assert.IsFalse(ElementSelectors.ByNameAndAttributesControlNS(BAR)
                           (control, noNS));
        }

        [Test]
        public void Not() {
            XmlElement control = doc.CreateElement(FOO);
            XmlElement equal = doc.CreateElement(FOO);
            XmlElement different = doc.CreateElement(BAR);
            Assert.IsFalse(ElementSelectors.Not(ElementSelectors.ByName)
                           (control, equal));
            Assert.IsTrue(ElementSelectors.Not(ElementSelectors.ByName)
                          (control, different));
        }

        [Test]
        public void Or() {
            XmlElement control = doc.CreateElement(FOO);
            XmlElement test = doc.CreateElement(BAR);
            Assert.IsFalse(ElementSelectors.Or(ElementSelectors.ByName)
                           (control, test));
            Assert.IsTrue(ElementSelectors.Or(ElementSelectors.ByName,
                                              ElementSelectors.Default)
                          (control, test));
        }

        [Test]
        public void And() {
            XmlElement control = doc.CreateElement(FOO);
            control.SetAttribute(BAR, SOME_URI, BAR);
            XmlElement test = doc.CreateElement(FOO);
            Assert.IsTrue(ElementSelectors.And(ElementSelectors.ByName)
                          (control, test));
            Assert.IsTrue(ElementSelectors.And(ElementSelectors.ByName,
                                               ElementSelectors.Default)
                          (control, test));
            Assert.IsFalse(ElementSelectors.And(ElementSelectors.ByName,
                                                ElementSelectors.Default,
                                                ElementSelectors.ByNameAndAllAttributes)
                           (control, test));
        }


        [Test]
        public void Xor() {
            XmlElement control = doc.CreateElement(FOO);
            XmlElement test = doc.CreateElement(BAR);
            XmlElement test2 = doc.CreateElement(FOO);
            Assert.IsFalse(ElementSelectors.Xor(ElementSelectors.ByName,
                                                ElementSelectors.ByNameAndAllAttributes)
                           (control, test));
            Assert.IsTrue(ElementSelectors.Xor(ElementSelectors.ByName,
                                               ElementSelectors.Default)
                          (control, test));
            Assert.IsFalse(ElementSelectors.Xor(ElementSelectors.ByName,
                                                ElementSelectors.Default)
                           (control, test2));
        }


        [Test]
        public void ConditionalReturnsFalseIfConditionIsNotMet() {
            XmlElement control = doc.CreateElement(FOO);
            XmlElement test = doc.CreateElement(FOO);
            Assert.IsFalse(ElementSelectors.ConditionalSelector(o => false,
                                                                ElementSelectors.ByName)
                           (control, test));
        }

        [Test]
        public void ConditionalAsksWrappedSelectorIfConditionIsMet() {
            XmlElement control = doc.CreateElement(FOO);
            XmlElement test = doc.CreateElement(BAR);
            XmlElement test2 = doc.CreateElement(FOO);
            Assert.IsFalse(ElementSelectors.ConditionalSelector(o => true,
                                                                ElementSelectors.ByName)
                           (control, test));
            Assert.IsTrue(ElementSelectors.ConditionalSelector(o => true,
                                                               ElementSelectors.ByName)
                          (control, test2));
        }

        [Test]
        public void PlainStringNamed() {
            XmlElement control = doc.CreateElement(FOO);
            XmlElement controlNS = doc.CreateElement(FOO, SOME_URI);
            XmlElement test = doc.CreateElement(FOO);
            XmlElement testNS = doc.CreateElement(FOO, SOME_URI);
            Assert.IsFalse(ElementSelectors.SelectorForElementNamed(BAR,
                                                                    ElementSelectors.ByName)
                           (control, test));
            Assert.IsTrue(ElementSelectors.SelectorForElementNamed(FOO,
                                                                   ElementSelectors.ByName)
                          (control, test));
            Assert.IsTrue(ElementSelectors.SelectorForElementNamed(FOO,
                                                                   ElementSelectors.ByName)
                          (controlNS, testNS));
        }

        [Test]
        public void QnameNamed() {
            XmlElement control = doc.CreateElement(FOO);
            XmlElement controlNS = doc.CreateElement(FOO, SOME_URI);
            XmlElement test = doc.CreateElement(FOO);
            XmlElement testNS = doc.CreateElement(FOO, SOME_URI);
            Assert.IsFalse(ElementSelectors.SelectorForElementNamed(new XmlQualifiedName(BAR),
                                                                    ElementSelectors.ByName)
                           (control, test));
            Assert.IsTrue(ElementSelectors.SelectorForElementNamed(new XmlQualifiedName(FOO),
                                                                   ElementSelectors.ByName)
                          (control, test));
            Assert.IsTrue(ElementSelectors.SelectorForElementNamed(new XmlQualifiedName(FOO, SOME_URI),
                                                                   ElementSelectors.ByName)
                          (controlNS, testNS));
        }

        [Test]
        public void XPath() {
            string BAZ = "BAZ";
            string XYZZY1 = "xyzzy1";
            string XYZZY2 = "xyzzy2";

            XmlElement control = doc.CreateElement(FOO);
            XmlElement bar = doc.CreateElement(BAR);
            control.AppendChild(bar);
            XmlElement baz = doc.CreateElement(BAZ);
            bar.AppendChild(baz);
            baz.AppendChild(doc.CreateTextNode(XYZZY1));
            baz = doc.CreateElement(BAZ);
            bar.AppendChild(baz);
            baz.AppendChild(doc.CreateTextNode(XYZZY2));

            XmlElement test = doc.CreateElement(FOO);
            bar = doc.CreateElement(BAR);
            test.AppendChild(bar);
            baz = doc.CreateElement(BAZ);
            bar.AppendChild(baz);
            baz.AppendChild(doc.CreateTextNode(XYZZY2));
            baz = doc.CreateElement(BAZ);
            bar.AppendChild(baz);
            baz.AppendChild(doc.CreateTextNode(XYZZY1));

            XmlElement test2 = doc.CreateElement(FOO);
            bar = doc.CreateElement(BAR);
            test2.AppendChild(bar);
            baz = doc.CreateElement(BAZ);
            bar.AppendChild(baz);
            baz.AppendChild(doc.CreateTextNode(XYZZY2));
            baz = doc.CreateElement(BAZ);
            bar.AppendChild(baz);
            baz.AppendChild(doc.CreateTextNode(XYZZY2));

            Assert.IsTrue(ElementSelectors.ByXPath(".//BAZ", ElementSelectors.ByNameAndText)
                          (control, test));
            Assert.IsFalse(ElementSelectors.ByXPath(".//BAZ",
                                                    ElementSelectors.ByNameAndText)
                           (control, test2));
        }

        [Test]
        public void ConditionalBuilder() {
            XmlElement control = doc.CreateElement(FOO);
            XmlElement test = doc.CreateElement(BAR);

            ElementSelectors.IConditionalSelectorBuilder builder =
                ElementSelectors.ConditionalBuilder()
                .WhenElementIsNamed(FOO).ThenUse(ElementSelectors.ByName);

            Assert.IsFalse(builder.Build()(control, test));

            builder.DefaultTo(ElementSelectors.Default);
            Assert.IsTrue(builder.Build()(control, test));
        }
   }
}