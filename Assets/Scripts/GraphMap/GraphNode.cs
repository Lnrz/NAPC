using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphNode
{
    private GraphNode[] neighbors = new GraphNode[4]; //0 right | 1 left | 2 up | 3 down
    private Vector2Int position;

    public GraphNode(int x, int y)
    {
        position.x = x;
        position.y = y;
    }

    public void SetNeighbors(GraphNode right, GraphNode left, GraphNode up, GraphNode down)
    {
        neighbors[0] = right;
        neighbors[1] = left;
        neighbors[2] = up;
        neighbors[3] = down;
    }

    public void SetNeighbor(int i, GraphNode node)
    {
        neighbors[i] = node;
    }

    public Vector2Int GetPosition()
    {
        return position;
    }


    public GraphNode GetNeighbor(int i)
    {
        return neighbors[i];
    }
}
