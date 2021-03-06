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

using NUnit.Framework;

namespace Org.XmlUnit.Diff {

    [TestFixture]
    public class DifferenceEvaluatorsTest {

        internal class Evaluator {
            internal bool Called = false;
            private readonly ComparisonResult Ret;
            internal ComparisonResult Orig;
            internal Evaluator(ComparisonResult ret) {
                Ret = ret;
            }
            public ComparisonResult Evaluate(Comparison comparison,
                                             ComparisonResult orig) {
                Called = true;
                Orig = orig;
                return Ret;
            }
        }

        [Test]
        public void EmptyFirstJustWorks() {
            DifferenceEvaluator d = DifferenceEvaluators.First();
            Assert.AreEqual(ComparisonResult.DIFFERENT,
                            d(null, ComparisonResult.DIFFERENT));
        }

        [Test]
        public void FirstChangeWinsInFirst() {
            Evaluator e1 = new Evaluator(ComparisonResult.DIFFERENT);
            Evaluator e2 = new Evaluator(ComparisonResult.EQUAL);
            DifferenceEvaluator d = DifferenceEvaluators.First(e1.Evaluate,
                                                               e2.Evaluate);
            Assert.AreEqual(ComparisonResult.DIFFERENT,
                            d(null, ComparisonResult.SIMILAR));
            Assert.IsTrue(e1.Called);
            Assert.IsFalse(e2.Called);
            e1.Called = false;
            Assert.AreEqual(ComparisonResult.EQUAL,
                            d(null, ComparisonResult.DIFFERENT));
            Assert.IsTrue(e1.Called);
            Assert.IsTrue(e2.Called);
        }

        [Test]
        public void AllEvaluatorsAreCalledInSequence() {
            Evaluator e1 = new Evaluator(ComparisonResult.SIMILAR);
            Evaluator e2 = new Evaluator(ComparisonResult.EQUAL);
            DifferenceEvaluator d = DifferenceEvaluators.Chain(e1.Evaluate,
                                                               e2.Evaluate);
            Assert.AreEqual(ComparisonResult.EQUAL,
                            d(null, ComparisonResult.DIFFERENT));

            Assert.IsTrue(e1.Called);
            Assert.That(e1.Orig, Is.EqualTo(ComparisonResult.DIFFERENT)); // passed initial ComparisonResult
            Assert.IsTrue(e2.Called);
            Assert.That(e2.Orig, Is.EqualTo(ComparisonResult.SIMILAR)); // passed ComparisonResult from e1
}
    }
}
