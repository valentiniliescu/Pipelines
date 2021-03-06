# Testing Pipelines and Generating Output

<!-- toc -->
## Contents

  * [GraphViz](#graphviz)
  * [PNG/SVG](#pngsvg)<!-- endToc -->

When verifying a pipeline graph, you can generate either a GraphViz diagram or an image in PNG or SVG format. This is done using [ApprovalTests](https://github.com/approvals/approvaltests.net).

## GraphViz

If you want to verify a `Pipeline`, you can call `PipelineApprovals.Verify()`:

<!-- snippet: graphviz_approval -->
<a id='snippet-graphviz_approval'></a>
```cs
var input = new InputPipe<long>("value");
input.ProcessFunction(LongToString);
input.ProcessFunction(IncrementLong);

PipelineApprovals.Verify(input);
```
<sup><a href='/Refactoring.Pipelines.Test/PipelineTests.cs#L139-L145' title='File snippet `graphviz_approval` was extracted from'>snippet source</a> | <a href='#snippet-graphviz_approval' title='Navigate to start of snippet `graphviz_approval`'>anchor</a></sup>
<!-- endSnippet -->

Which produces the following approved file:

<!-- snippet: _approvals/PipelineTests.SplitInput.approved.txt -->
<a id='snippet-_approvals/PipelineTests.SplitInput.approved.txt'></a>
```txt
digraph G { node [style=filled, shape=rect]

# Nodes
"Int64 value" -> "PipelineTests.LongToString()" -> "String"
"Int64 value" -> "PipelineTests.IncrementLong()" -> "Int64"


# Formatting
"Int64 value" [color=green]
"String" [color="#9fbff4"]
"PipelineTests.LongToString()" [shape=invhouse]
"Int64" [color="#9fbff4"]
"PipelineTests.IncrementLong()" [shape=invhouse]



}
```
<sup><a href='/Refactoring.Pipelines.Test/_approvals/PipelineTests.SplitInput.approved.txt#L1-L17' title='File snippet `_approvals/PipelineTests.SplitInput.approved.txt` was extracted from'>snippet source</a> | <a href='#snippet-_approvals/PipelineTests.SplitInput.approved.txt' title='Navigate to start of snippet `_approvals/PipelineTests.SplitInput.approved.txt`'>anchor</a></sup>
<!-- endSnippet -->


Usually we open these in Visual Studio Code with a plugin. The advantage of these files is that they diff easily and render nicely.

## PNG/SVG

If you want to create the images, you can do that automatically as well, using the following:

<!-- snippet: graphviz_png_approval -->
<a id='snippet-graphviz_png_approval'></a>
```cs
var input = CreateQuickPipelineWithInput();
PipelineApprovals.VerifyAsPng(input);
```
<sup><a href='/Refactoring.Pipelines.Test/PipelineTests.cs#L46-L49' title='File snippet `graphviz_png_approval` was extracted from'>snippet source</a> | <a href='#snippet-graphviz_png_approval' title='Navigate to start of snippet `graphviz_png_approval`'>anchor</a></sup>
<!-- endSnippet -->

which produces the following approved file:

![GraphViz of Pipeline in PNG format](/Refactoring.Pipelines.Test/_approvals/PipelineTests.TestPng.approved.png)
