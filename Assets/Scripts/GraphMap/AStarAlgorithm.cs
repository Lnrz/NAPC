using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AStarAlgorithm
{
    public static IList<Vector2> AStarAlgo(GraphNode src, GraphNode dest)
    {
        GraphNodeWrap selected;
        GraphNodeWrap neighborWrapped;
        GraphNode neighbor;
        int fNeighborValue;
        ICollection<GraphNodeWrap> open = new LinkedList<GraphNodeWrap>();
        ICollection<GraphNodeWrap> closed = new LinkedList<GraphNodeWrap>();

        open.Add(new GraphNodeWrap(src, null, 0));
        while (open.Count != 0)
        {
            selected = GetNodeWithLowestF(open);
            open.Remove(selected);
            for (int i = 0; i < 4; i++)
            {



                if (selected == null || selected.GetGraphNode() == null)
                {
                    UnityEditor.EditorApplication.isPaused = true;
                }



                neighbor = selected.GetGraphNode().GetNeighbor(i);
                if (neighbor != null)
                {
                    fNeighborValue = selected.GetF() + MeasureDistance(selected.GetGraphNode(), neighbor) + MeasureDistance(neighbor, dest);
                    neighborWrapped = new GraphNodeWrap(neighbor, selected, fNeighborValue);
                    if (neighbor == dest)
                    {
                        return MakeVector2List(neighborWrapped);
                    }
                    if (IsItsFValueBetter(neighbor, fNeighborValue, open, closed))
                    {
                        open.Add(neighborWrapped);
                    }
                }
            }
            closed.Add(selected);
        }
        return null;
    }

    private static GraphNodeWrap GetNodeWithLowestF(ICollection<GraphNodeWrap> open)
    {
        GraphNodeWrap res = null;

        foreach (GraphNodeWrap wrap in open)
        {
            if (res == null)
            {
                res = wrap;
            }
            else
            {
                if (res.GetF() > wrap.GetF())
                {
                    res = wrap;
                }
            }
        }
        return res;
    }

    private static int MeasureDistance(GraphNode nodeOne, GraphNode nodeTwo)
    {
        int res;

        res = Mathf.Abs(nodeOne.GetPosition().x - nodeTwo.GetPosition().x);
        res += Mathf.Abs(nodeOne.GetPosition().y - nodeTwo.GetPosition().y);
        return res;
    }

    private static bool IsItsFValueBetter(GraphNode node, int fValue, ICollection<GraphNodeWrap> open, ICollection<GraphNodeWrap> closed)
    {
        return IsItsFValueBetterInColl(node, fValue, open) && IsItsFValueBetterInColl(node, fValue, closed);
    }

    private static bool IsItsFValueBetterInColl(GraphNode node, int fValue, ICollection<GraphNodeWrap> coll)
    {
        foreach (GraphNodeWrap wrap in coll)
        {
            if (wrap.GetGraphNode() == node)
            {
                if (wrap.GetF() < fValue)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        return true;
    }

    private static IList<Vector2> MakeVector2List(GraphNodeWrap wrap)
    {
        GraphNode[] route;
        int routeLength;
        IList<Vector2> res;
        Vector2 temp;


        res = new List<Vector2>();
        routeLength = GetRouteLength(wrap);
        route = new GraphNode[routeLength];
        do
        {
            routeLength--;
            route[routeLength] = wrap.GetGraphNode();
            wrap = wrap.GetParent();
        }
        while (wrap != null);
        for (int i = 0; i < route.Length; i++)
        {
            temp = new Vector2();
            temp.x = route[i].GetPosition().x + 0.5f;
            temp.y = route[i].GetPosition().y + 0.5f;
            res.Add(temp);
        }
        return res;
    }

    private static int GetRouteLength(GraphNodeWrap wrap)
    {
        int res = 1;

        while (wrap.GetParent() != null)
        {
            wrap = wrap.GetParent();
            res++;
        }
        return res;
    }
}