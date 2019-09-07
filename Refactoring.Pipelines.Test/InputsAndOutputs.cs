using System.Collections.Generic;

namespace Refactoring.Pipelines.Test
{
    public class InputsAndOutputs
    {
        private readonly IGraphNode _node;

        public InputsAndOutputs(IGraphNode node)
        {
            _node = node;
            Inputs.AddRange(IGraphNodeHelper.GetRoots(_node));
            Outputs.AddRange(IGraphNodeHelper.GetOutputs(_node));
        }

        public List<IGraphNode> Inputs = new List<IGraphNode>();
        public List<IGraphNode> Outputs = new List<IGraphNode>();
    }
}
