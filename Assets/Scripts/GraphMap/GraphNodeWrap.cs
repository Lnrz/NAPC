using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphNodeWrap
{
    private GraphNode node;
    private GraphNodeWrap parent;
    private int fValue;

    public GraphNodeWrap(GraphNode node, GraphNodeWrap parent, int fValue)
    {
        this.node = node;
        this.parent = parent;
        this.fValue = fValue;
    }

    public int GetF()
    {
        return fValue;
    }

    public GraphNode GetGraphNode()
    {
        return node;
    }

    public GraphNodeWrap GetParent()
    {
        return parent;
    }
}
