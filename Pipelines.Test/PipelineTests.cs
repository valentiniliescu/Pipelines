using System;
using System.Collections.Generic;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using ApprovalTests.Writers;
using ApprovalUtilities.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pipelines.DotGraph;

namespace Pipelines.Test
{
    [UseReporter(typeof(VisualStudioReporter))]
    [TestClass]
    public class PipelineTests
    {
        [TestMethod]
        public void BasicPipelineFunctionalTest()
        {
            Func<string, long> normal = age =>
            {
                // begin-snippet: basic_code_line 
                var result = long.Parse(age);
                // end-snippet 
                return result;
            };
            Func<string, long> piped = age =>
            {
                // begin-snippet: basic_pipeline
                var inputPipe = new InputPipe<string>("age");
                var parsePipe = inputPipe.Process(long.Parse);
                var collector = parsePipe.Collect();

                inputPipe.Send("42");
                var result = collector.SingleResult;
                // end-snippet
                return result;
            };

            Assert.AreEqual(normal("42"), piped("42"));
        }

        [TestMethod]
        public void BasicPipelineTest()
        {
            var input = new InputPipe<string>("age");
            var parse = input.Process(long.Parse);
            var collector = parse.Collect();

            Verify(input);
            input.Send("42");
            Assert.AreEqual(42, collector.SingleResult);
        }

        [TestMethod]
        public void ProcessIsNotCovariant()
        {
            var input = new InputPipe<int>("i");
            var collector = input.Process(RangeArray)
                .Process(_ => (IEnumerable<int>) _) // Would be nice not to need this
                .Process(SumEnumerable)
                .Collect();
            input.Send(4);
            Assert.AreEqual(10, collector.SingleResult);
        }

        private int SumEnumerable(IEnumerable<int> _) { return _.Sum(); }

        private int[] RangeArray(int count) { return Enumerable.Range(1, count).ToArray(); }

        [TestMethod]
        public void ConnectedPipelinesTest()
        {
            var input = new InputPipe<string>("age");
            var parsePipe = input.Process(long.Parse);
            var collector = parsePipe.Collect();
            parsePipe.Process(LongToString)
                .WithCollector()
                .Process(long.Parse)
                .WithCollector()
                .Process(LongToString)
                .Collect();

            Verify(input);
        }

        [TestMethod]
        public void SplitAndJoin()
        {
            var input = new InputPipe<string>("age");
            var parse = input.Process(long.Parse);
            var longToString = parse.Process(LongToString);
            var incrementLong = parse.Process(IncrementLong);

            var joinedPipes = longToString.JoinTo(incrementLong).Collect();

            Verify(input);
        }

        [TestMethod]
        public void SplitInput()
        {
            var input = new InputPipe<long>("value");
            input.Process(LongToString);
            input.Process(IncrementLong);

            Verify(input);
        }

        [TestMethod]
        public void JoinInputs()
        {
            // begin-snippet: joined_pipeline 
            var input1 = new InputPipe<long>("value1");
            var input2 = new InputPipe<long>("value2");
            var join = input1.JoinTo(input2);
            // end-snippet
            var collector = join.Collect();

            input1.Send(42);
            Assert.IsTrue(collector.IsEmpty);

            input2.Send(99);
            Assert.AreEqual("(42, 99)", collector.SingleResult.ToString());

            Verify(join);
        }

        [TestMethod]
        public void CannotFindNodeExceptionHelpMessage()
        {
            var input1 = new InputPipe<long>("value1");
            var subject = new NodeMetadata.CannotFindNodeException(input1);
            Approvals.Verify(subject);
        }

        [TestMethod]
        public void ApplyTo()
        {
            var prefix = new InputPipe<string>("prefix");
            var values = new InputPipe<int[]>("values");

            var applyToPipeline = prefix.ApplyTo(values);
            var collector = applyToPipeline.Collect();
            Verify(applyToPipeline);

            // begin-snippet: ApplyTo_inputs
            var apply = "#";
            var to = new[] {1, 2};
            // end-snippet

            // begin-snippet: ApplyTo_outputs
            var result = "[(#, 1), (#, 2)]";
            // end-snippet

            var manualApplyTo =
                // begin-snippet: ApplyTo_manual
                prefix.JoinTo(values).Process(t => t.Item2.Select(i => Tuple.Create(t.Item1, i)));
            // end-snippet
            var manualApplyToResult = manualApplyTo.Collect();

            prefix.Send(apply);
            values.Send(to);
            Assert.AreEqual(result, collector.SingleResult.ToReadableString());

            Assert.AreEqual(
                manualApplyToResult.SingleResult.ToReadableString(),
                collector.SingleResult.ToReadableString());
        }

        [TestMethod]
        public void Concat()
        {
            var part1 = new InputPipe<List<int>>("part1");
            var part2 = new InputPipe<int[]>("part2");
            var concatWithPipeline = part1.ConcatWith(part2);
            var collector = concatWithPipeline.Collect();
            Verify(concatWithPipeline);

            // begin-snippet: ConcatWith_inputs
            var concat = new List<int> { 1, 2 };
            var with = new[] { 3, 4 };
            // end-snippet

            // begin-snippet: ConcatWith_outputs
            var result = "[1, 2, 3, 4]";
            // end-snippet

            var manualConcatWith =
                // begin-snippet: ConcatWith_manual
                part1.JoinTo(part2).Process(t => t.Item1.Concat(t.Item2).ToList());
            // end-snippet
            var manualConcatWithResult = manualConcatWith.Collect();

            part1.Send(concat);
            part2.Send(with);
            Assert.AreEqual(result, collector.SingleResult.ToReadableString());

            Assert.AreEqual(
                manualConcatWithResult.SingleResult.ToReadableString(),
                collector.SingleResult.ToReadableString());
        }

        private string LongToString(long value) { return value.ToString(); }

        private long IncrementLong(long value) { return value + 1; }

        private static void Verify(IGraphNode input)
        {
            Approvals.Verify(WriterFactory.CreateTextWriter(DotGraph.DotGraph.FromPipeline(input), "dot"));
        }
    }

    internal static class _
    {
        public static FunctionPipe<TInput, TOutput> WithCollector<TInput, TOutput>(
            this FunctionPipe<TInput, TOutput> @this)
        {
            @this.Collect();
            return @this;
        }
    }
}
