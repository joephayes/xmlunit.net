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

using System;
using System.Collections.Generic;

namespace Org.XmlUnit.Diff {

    /// <summary>
    /// Useful base-implementation of some parts of the
    /// IDifferenceEngine interface.
    /// </summary>
    public abstract class AbstractDifferenceEngine : IDifferenceEngine {
        public event ComparisonListener ComparisonListener;
        public event ComparisonListener MatchListener;
        public event ComparisonListener DifferenceListener;

        private INodeMatcher nodeMatcher = new DefaultNodeMatcher();
        public virtual INodeMatcher NodeMatcher {
            set {
                if (value == null) {
                    throw new ArgumentNullException("node matcher");
                }
                nodeMatcher = value;
            }
            get {
                return nodeMatcher;
            }
        }

        private DifferenceEvaluator diffEvaluator = DifferenceEvaluators.Default;
        public virtual DifferenceEvaluator DifferenceEvaluator {
            set {
                if (value == null) {
                    throw new ArgumentNullException("difference evaluator");
                }
                diffEvaluator = value;
            }
            get {
                return diffEvaluator;
            }
        }

        private ComparisonController comparisonController = ComparisonControllers.Default;
        public virtual ComparisonController ComparisonController {
            set {
                if (value == null) {
                    throw new ArgumentNullException("comparison controller");
                }
                comparisonController = value;
            }
            get {
                return comparisonController;
            }
        }

        public abstract void Compare(ISource control, ISource test);

        private IDictionary<string, string> namespaceContext;

        public IDictionary<string, string> NamespaceContext {
            set {
                namespaceContext = value == null ? value
                    : new Dictionary<string, string>(value);
            }
            protected get {
                return namespaceContext == null ? namespaceContext
                    : new Dictionary<string, string>(namespaceContext);
            }
        }

        /// <summary>
        /// Compares the detail values for object equality, lets the
        /// difference evaluator evaluate the result, notifies all
        /// listeners and returns the outcome.
        /// </summary>
        protected internal ComparisonState Compare(Comparison comp) {
            ComparisonResult initial = Equals(comp.ControlDetails.Value, comp.TestDetails.Value)
                ? ComparisonResult.EQUAL
                : ComparisonResult.DIFFERENT;
            ComparisonResult altered = DifferenceEvaluator(comp, initial);
            FireComparisonPerformed(comp, altered);
            return altered != ComparisonResult.EQUAL
                && ComparisonController(new Difference(comp, altered))
                ? (ComparisonState) new FinishedComparisonState(this, altered)
                : new OngoingComparisonState(this, altered);
        }

        private void FireComparisonPerformed(Comparison comp,
                                             ComparisonResult outcome) {
            ComparisonListener cl = ComparisonListener;
            ComparisonListener ml = MatchListener;
            ComparisonListener dl = DifferenceListener;
            if (cl != null) {
                cl(comp, outcome);
            }
            if (outcome == ComparisonResult.EQUAL && ml != null) {
                ml(comp, outcome);
            } else if (outcome != ComparisonResult.EQUAL && dl != null) {
                dl(comp, outcome);
            }
        }

        protected static string GetXPath(XPathContext ctx) {
            return ctx == null ? null : ctx.XPath;
        }

        /// <summary>
        ///   Encapsulates the current result and a flag that
        ///   indicates whether comparison should be stopped.
        /// </summary>
        public abstract class ComparisonState : IEquatable<ComparisonState> {
            private readonly AbstractDifferenceEngine engine;
            private readonly bool finished;
            private readonly ComparisonResult result;

            protected ComparisonState(AbstractDifferenceEngine engine, bool finished,
                                      ComparisonResult result) {
                this.engine = engine;
                this.finished = finished;
                this.result = result;
            }

            public ComparisonState AndThen(Func<ComparisonState> newStateProducer) {
                return finished ? this : newStateProducer();
            }

            public ComparisonState AndIfTrueThen(bool predicate,
                                                 Func<ComparisonState> newStateProducer) {
                return predicate ? AndThen(newStateProducer) : this;
            }

            public ComparisonState AndThen(Comparison comp) {
                return AndThen(() => engine.Compare(comp));
            }

            public ComparisonState AndIfTrueThen(bool predicate,
                                                 Comparison comp) {
                return AndIfTrueThen(predicate, () => engine.Compare(comp));
            }

            public override string ToString() {
                return string.Format("{0}: current result is {1}", GetType().Name,
                                     result);
            }

            public override bool Equals(object other) {
                return Equals(other as ComparisonState);
            }

            public bool Equals(ComparisonState other) {
                return other != null
                    && GetType() == other.GetType()
                    && finished == other.finished
                    && result == other.result;
            }

            public override int GetHashCode() {
                return (finished ? 7 : 1) * result.GetHashCode();
            }
        }

        internal sealed class FinishedComparisonState : ComparisonState {
            internal FinishedComparisonState(AbstractDifferenceEngine engine, ComparisonResult result)
                : base(engine, true, result) {
            }
        }

        internal sealed class OngoingComparisonState : ComparisonState {
            internal OngoingComparisonState(AbstractDifferenceEngine engine, ComparisonResult result)
                : base(engine, false, result) {
            }
            internal OngoingComparisonState(AbstractDifferenceEngine engine)
                : this(engine, ComparisonResult.EQUAL) {
            }
        }
    }
}
