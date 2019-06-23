﻿using System;

namespace Refactoring.Pipelines
{
    public interface IJoinedPipes : IGraphNodeWithOutput
    {
        Tuple<IGraphNode, IGraphNode> Predecessors { get; }
        IGraphNode Collector { get; }
    }
}