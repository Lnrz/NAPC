using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Graph
{
    private static ICollection<GraphNode> map;
    private static ICollection<GraphNode> hardBrickMap = new LinkedList<GraphNode>();
    private static ICollection<GraphNode> coldIceMap = new LinkedList<GraphNode>();
    private static bool isHardBrickMapModified = false;

    public static void AddNode(Vector2 blockPos)
    {
        Vector2Int blockPosInt;
        GraphNode node;
        GraphNode[] nearNodes;

        blockPosInt = NormalizeToInt(blockPos);
        node = new GraphNode(blockPosInt.x, blockPosInt.y);
        nearNodes = new GraphNode[4];

        GetNearNodes(blockPosInt, nearNodes);
        for (int i = 0; i < 4; i++)
        {
            if (nearNodes[i] == null)
            {
                nearNodes[i] = MakeNeighborNode(blockPos, i, node);
                if (nearNodes[i] != null)
                {
                    hardBrickMap.Add(nearNodes[i]);
                }
            }
            else
            {
                nearNodes[i].SetNeighbor((i % 2 == 0)? i + 1 : i - 1, node);
            }
        }
        node.SetNeighbors(nearNodes[0], nearNodes[1], nearNodes[2], nearNodes[3]);
        hardBrickMap.Add(node);
        isHardBrickMapModified = true;
    }

    private static GraphNode MakeNeighborNode(Vector2 blockPos, int dir, GraphNode node)
    {
        GraphNode res;
        Vector2 direction;
        ContactFilter2D filter;
        RaycastHit2D[] rcha;
        int length;
        Vector2 wallPos;
        Vector2 newNodePos;

        res = null;
        direction = GetDirection(dir);
        filter = new ContactFilter2D();
        filter.useTriggers = false;
        rcha = new RaycastHit2D[7];
        length = Physics2D.Raycast(blockPos, direction, filter, rcha);
        wallPos = Vector2.zero;
        for (int i = 0; i < length; i++)
        {
            if (rcha[i].collider != null && rcha[i].collider.gameObject.CompareTag("Wall"))
            {
                wallPos = rcha[i].collider.gameObject.transform.position;
                break;
            }
        }
        newNodePos = wallPos - direction;
        if (newNodePos.x != blockPos.x || newNodePos.y != blockPos.y)
        {
            res = new GraphNode(Mathf.FloorToInt(newNodePos.x), Mathf.FloorToInt(newNodePos.y));
            MakeNeighborNodeSetNeighbors(res, node, dir);
        }
        return res;
    }

    private static void MakeNeighborNodeSetNeighbors(GraphNode res, GraphNode node, int dir)
    {
        GraphNode[] nearNodes;

        nearNodes = new GraphNode[4];
        GetNearNodes(res.GetPosition(), nearNodes);
        nearNodes[(dir % 2 == 0)? dir + 1 : dir - 1] = node;
        res.SetNeighbors(nearNodes[0], nearNodes[1], nearNodes[2], nearNodes[3]);
        for (int i = 0; i < 4; i++)
        {
            if (nearNodes[i] != null)
            {
                nearNodes[i].SetNeighbor((i % 2 == 0)? i + 1 : i - 1, res);
            }
        }
    }

    private static Vector2 GetDirection(int dir)
    {
        switch (dir)
        {
            case 0:
                return Vector2.right;
            case 1:
                return Vector2.left;
            case 2:
                return Vector2.up;
            default:
                return Vector2.down;
        }
    }

    private static void GetNearNodes(Vector2Int blockPos, GraphNode[] nearNodes)
    {
        int[] dist = { int.MaxValue, int.MaxValue , int.MaxValue , int.MaxValue };
        int temp;
        Vector2Int nodePos;
        Vector2 blockPosFloat;

        blockPosFloat = new Vector2(blockPos.x + 0.5f, blockPos.y + 0.5f);
        foreach(GraphNode node in hardBrickMap)
        {
            nodePos = node.GetPosition();
            if (!CheckForWallCollision(blockPosFloat, nodePos))
            {
                if (nodePos.y == blockPos.y)
                {
                    temp = nodePos.x - blockPos.x;
                    temp = Mathf.Abs(temp);
                    if (nodePos.x > blockPos.x)
                    {
                        if (temp < dist[0])
                        {
                            dist[0] = temp;
                            nearNodes[0] = node;
                        }
                    }
                    else
                    {
                        if (temp < dist[1])
                        {
                            dist[1] = temp;
                            nearNodes[1] = node;
                        }
                    }
                }
                else if (nodePos.x == blockPos.x)
                {
                    temp = nodePos.y - blockPos.y;
                    temp = Mathf.Abs(temp);
                    if (nodePos.y > blockPos.y)
                    {
                        if (temp < dist[2])
                        {
                            dist[2] = temp;
                            nearNodes[2] = node;
                        }
                    }
                    else
                    {
                        if (temp < dist[3])
                        {
                            dist[3] = temp;
                            nearNodes[3] = node;
                        }
                    }
                }
            }
        }
    }

    public static GraphNode GetApproximateNode(Vector2 pos)
    {
        GraphNode res = null;
        int srcDist;
        int srcDistTemp;

        srcDist = int.MaxValue;
        foreach (GraphNode node in map)
        {
            if (!CheckForWallCollision(pos, node.GetPosition()))
            {
                srcDistTemp = MeasureDistance(pos, node.GetPosition());
                if (srcDistTemp < srcDist || (srcDistTemp == srcDist && FiftyFifty()))
                {
                    res = node;
                    srcDist = srcDistTemp;
                }
            }
        }
        return res;
    }

    private static bool FiftyFifty()
    {
        return Random.Range(0, 2) % 2 == 0;
    }

    private static bool CheckForWallCollision(Vector2 src, Vector2 dest)
    {
        Vector2 dir;
        ContactFilter2D filter;
        RaycastHit2D[] rchArray;
        int rchNum;

        rchArray = new RaycastHit2D[7];
        filter = new ContactFilter2D();
        filter.useTriggers = false;
        dir = dest - src;
        dir.x = dir.x + 0.5f;
        dir.y = dir.y + 0.5f;
        rchNum = Physics2D.Raycast(src, dir, filter, rchArray, dir.magnitude);
        for (int i = 0; i < rchNum; i++)
        {
            if (rchArray[i].collider != null && rchArray[i].collider.gameObject.CompareTag("Wall"))
            {
                return true;
            }
        }
        return false;
    }

    private static int MeasureDistance(Vector2 nodePos, Vector2Int entityPos)
    {
        Vector2Int nodePosInt;

        nodePosInt = NormalizeToInt(nodePos);
        return Mathf.Abs(entityPos.x - nodePosInt.x) + Mathf.Abs(entityPos.y - nodePosInt.y);
    }

    private static Vector2Int NormalizeToInt(Vector2 pos)
    {
        Vector2Int newPos;

        newPos = new Vector2Int();
        newPos.x = Mathf.FloorToInt(pos.x);
        newPos.y = Mathf.FloorToInt(pos.y);
        return newPos;
    }

    public static void BuildHardBrickMap()
    {
        map = hardBrickMap;
        if (isHardBrickMapModified)
        {
            hardBrickMap.Clear();
            isHardBrickMapModified = false;
        }
        if (hardBrickMap.Count == 0)
        {
            GraphNode node1 = new GraphNode(-13, -6);
            GraphNode node2 = new GraphNode(-9, -6);
            GraphNode node3 = new GraphNode(-3, -6);
            GraphNode node4 = new GraphNode(1, -6);
            GraphNode node5 = new GraphNode(3, -6);
            GraphNode node6 = new GraphNode(6, -6);
            GraphNode node7 = new GraphNode(9, -6);
            GraphNode node8 = new GraphNode(12, -6);
            GraphNode node9 = new GraphNode(-13, -4);
            GraphNode node10 = new GraphNode(-9, -4);
            GraphNode node11 = new GraphNode(-7, -4);
            GraphNode node12 = new GraphNode(-5, -4);
            GraphNode node13 = new GraphNode(-3, -4);
            GraphNode node14 = new GraphNode(1, -4);
            GraphNode node15 = new GraphNode(3, -4);
            GraphNode node16 = new GraphNode(5, -4);
            GraphNode node17 = new GraphNode(6, -4);
            GraphNode node18 = new GraphNode(8, -4);
            GraphNode node19 = new GraphNode(9, -4);
            GraphNode node20 = new GraphNode(10, -4);
            GraphNode node21 = new GraphNode(-13, -2);
            GraphNode node22 = new GraphNode(-9, -2);
            GraphNode node23 = new GraphNode(-7, -2);
            GraphNode node24 = new GraphNode(-5, -2);
            GraphNode node25 = new GraphNode(-3, -2);
            GraphNode node26 = new GraphNode(3, -2);
            GraphNode node27 = new GraphNode(5, -2);
            GraphNode node28 = new GraphNode(8, -2);
            GraphNode node29 = new GraphNode(9, -2);
            GraphNode node30 = new GraphNode(10, -2);
            GraphNode node31 = new GraphNode(5, -1);
            GraphNode node32 = new GraphNode(6, -1);
            GraphNode node33 = new GraphNode(-13, 0);
            GraphNode node34 = new GraphNode(-9, 0);
            GraphNode node35 = new GraphNode(-7, 0);
            GraphNode node36 = new GraphNode(-5, 0);
            GraphNode node37 = new GraphNode(-3, 0);
            GraphNode node38 = new GraphNode(9, 0);
            GraphNode node39 = new GraphNode(12, 0);
            GraphNode node40 = new GraphNode(-10, 2);
            GraphNode node41 = new GraphNode(-8, 2);
            GraphNode node42 = new GraphNode(-6, 2);
            GraphNode node43 = new GraphNode(-4, 2);
            GraphNode node44 = new GraphNode(-2, 2);
            GraphNode node45 = new GraphNode(2, 2);
            GraphNode node46 = new GraphNode(3, 2);
            GraphNode node47 = new GraphNode(4, 2);
            GraphNode node48 = new GraphNode(6, 2);
            GraphNode node49 = new GraphNode(12, 2);
            GraphNode node50 = new GraphNode(-13, 3);
            GraphNode node51 = new GraphNode(-11, 3);
            GraphNode node52 = new GraphNode(-10, 3);
            GraphNode node53 = new GraphNode(-6, 4);
            GraphNode node54 = new GraphNode(-4, 4);
            GraphNode node55 = new GraphNode(-2, 4);
            GraphNode node56 = new GraphNode(1, 4);
            GraphNode node57 = new GraphNode(2, 4);
            GraphNode node58 = new GraphNode(4, 4);
            GraphNode node59 = new GraphNode(6, 4);
            GraphNode node60 = new GraphNode(-13, 5);
            GraphNode node61 = new GraphNode(-11, 5);
            GraphNode node62 = new GraphNode(-8, 5);
            GraphNode node63 = new GraphNode(-6, 5);
            GraphNode node64 = new GraphNode(-2, 5);
            GraphNode node65 = new GraphNode(1, 5);
            GraphNode node66 = new GraphNode(6, 5);
            GraphNode node67 = new GraphNode(12, 5);
            GraphNode node68 = new GraphNode(-10, 0);
            GraphNode node69 = new GraphNode(-10, 5);
            GraphNode node70 = new GraphNode(-7, -6);

            node1.SetNeighbors(node2, null, null, null);
            node2.SetNeighbors(null, node1, node10, null);
            node3.SetNeighbors(node4, node70, node13, null);
            node4.SetNeighbors(null, node3, node14, null);
            node5.SetNeighbors(node6, null, node15, null);
            node6.SetNeighbors(null, node5, node17, null);
            node7.SetNeighbors(node8, null, node19, null);
            node8.SetNeighbors(null, node7, node39, null);
            node9.SetNeighbors(node10, null, node21, null);
            node10.SetNeighbors(node11, node9, node22, node2);
            node11.SetNeighbors(node12, node10, node23, null);
            node12.SetNeighbors(node13, node11, node24, null);
            node13.SetNeighbors(node14, node12, node25, node3);
            node14.SetNeighbors(node15, node13, null, node4);
            node15.SetNeighbors(node16, node14, null, node5);
            node16.SetNeighbors(node17, node15, node27, null);
            node17.SetNeighbors(node18, node16, null, node6);
            node18.SetNeighbors(node19, node17, node28, null);
            node19.SetNeighbors(node20, node18, null, node7);
            node20.SetNeighbors(null, node19, node30, null);
            node21.SetNeighbors(node22, null, null, node9);
            node22.SetNeighbors(node23, node21, node34, node10);
            node23.SetNeighbors(node24, node22, node35, node11);
            node24.SetNeighbors(node25, node23, node36, node12);
            node25.SetNeighbors(node26, node24, node37, node13);
            node26.SetNeighbors(node27, node25, node46, null);
            node27.SetNeighbors(null, node26, node31, node16);
            node28.SetNeighbors(node29, null, null, node18);
            node29.SetNeighbors(node30, node28, node38, null);
            node30.SetNeighbors(null, node29, null, node20);
            node31.SetNeighbors(node32, null, null, node27);
            node32.SetNeighbors(null, node31, node48, null);
            node33.SetNeighbors(node68, null, node50, null);
            node34.SetNeighbors(node35, node68, null, node22);
            node35.SetNeighbors(node36, node34, null, node23);
            node36.SetNeighbors(node37, node35, null, node24);
            node37.SetNeighbors(null, node36, null, node25);
            node38.SetNeighbors(node39, null, null, node29);
            node39.SetNeighbors(null, node38, node49, node8);
            node40.SetNeighbors(node41, null, node52, node68);
            node41.SetNeighbors(node42, node40, node62, null);
            node42.SetNeighbors(node43, node41, node53, null);
            node43.SetNeighbors(node44, node42, node54, null);
            node44.SetNeighbors(node45, node43, node55, null);
            node45.SetNeighbors(node46, node44, node57, null);
            node46.SetNeighbors(node47, node45, null, node26);
            node47.SetNeighbors(node48, node46, node58, null);
            node48.SetNeighbors(node49, node47, node59, node32);
            node49.SetNeighbors(null, node48, node67, node39);
            node50.SetNeighbors(node51, null, node60, node33);
            node51.SetNeighbors(node52, node50, node61, null);
            node52.SetNeighbors(null, node51, null, node40);
            node53.SetNeighbors(node54, null, node63, node42);
            node54.SetNeighbors(node55, node53, null, node43);
            node55.SetNeighbors(null, node54, node64, node44);
            node56.SetNeighbors(node57, null, node65, null);
            node57.SetNeighbors(node58, node56, null, node45);
            node58.SetNeighbors(node59, node57, null, node47);
            node59.SetNeighbors(null, node58, node66, node48);
            node60.SetNeighbors(node61, null, null, node50);
            node61.SetNeighbors(node69, node60, null, node51);
            node62.SetNeighbors(node63, null, null, node41);
            node63.SetNeighbors(null, node62, null, node53);
            node64.SetNeighbors(node65, null, null, node55);
            node65.SetNeighbors(null, node64, null, node56);
            node66.SetNeighbors(node67, null, null, node59);
            node67.SetNeighbors(null, node66, null, node49);
            node68.SetNeighbors(node34, node33, node40, null);
            node69.SetNeighbors(null, node61, null, null);
            node70.SetNeighbors(node3, null, null, null);


            hardBrickMap.Add(node1);
            hardBrickMap.Add(node2);
            hardBrickMap.Add(node3);
            hardBrickMap.Add(node4);
            hardBrickMap.Add(node5);
            hardBrickMap.Add(node6);
            hardBrickMap.Add(node7);
            hardBrickMap.Add(node8);
            hardBrickMap.Add(node9);
            hardBrickMap.Add(node10);
            hardBrickMap.Add(node11);
            hardBrickMap.Add(node12);
            hardBrickMap.Add(node13);
            hardBrickMap.Add(node14);
            hardBrickMap.Add(node15);
            hardBrickMap.Add(node16);
            hardBrickMap.Add(node17);
            hardBrickMap.Add(node18);
            hardBrickMap.Add(node19);
            hardBrickMap.Add(node20);
            hardBrickMap.Add(node21);
            hardBrickMap.Add(node22);
            hardBrickMap.Add(node23);
            hardBrickMap.Add(node24);
            hardBrickMap.Add(node25);
            hardBrickMap.Add(node26);
            hardBrickMap.Add(node27);
            hardBrickMap.Add(node28);
            hardBrickMap.Add(node29);
            hardBrickMap.Add(node30);
            hardBrickMap.Add(node31);
            hardBrickMap.Add(node32);
            hardBrickMap.Add(node33);
            hardBrickMap.Add(node34);
            hardBrickMap.Add(node35);
            hardBrickMap.Add(node36);
            hardBrickMap.Add(node37);
            hardBrickMap.Add(node38);
            hardBrickMap.Add(node39);
            hardBrickMap.Add(node40);
            hardBrickMap.Add(node41);
            hardBrickMap.Add(node42);
            hardBrickMap.Add(node43);
            hardBrickMap.Add(node44);
            hardBrickMap.Add(node45);
            hardBrickMap.Add(node46);
            hardBrickMap.Add(node47);
            hardBrickMap.Add(node48);
            hardBrickMap.Add(node49);
            hardBrickMap.Add(node50);
            hardBrickMap.Add(node51);
            hardBrickMap.Add(node52);
            hardBrickMap.Add(node53);
            hardBrickMap.Add(node54);
            hardBrickMap.Add(node55);
            hardBrickMap.Add(node56);
            hardBrickMap.Add(node57);
            hardBrickMap.Add(node58);
            hardBrickMap.Add(node59);
            hardBrickMap.Add(node60);
            hardBrickMap.Add(node61);
            hardBrickMap.Add(node62);
            hardBrickMap.Add(node63);
            hardBrickMap.Add(node64);
            hardBrickMap.Add(node65);
            hardBrickMap.Add(node66);
            hardBrickMap.Add(node67);
            hardBrickMap.Add(node68);
            hardBrickMap.Add(node69);
            hardBrickMap.Add(node70);
        }
    }

    public static void BuildColdIceMap()
    {
        map = coldIceMap;
        if (coldIceMap.Count == 0)
        {
            GraphNode node1 = new GraphNode(-13, -6);
            GraphNode node2 = new GraphNode(-12, -6);
            GraphNode node3 = new GraphNode(-10, -6);
            GraphNode node4 = new GraphNode(-7, -6);
            GraphNode node5 = new GraphNode(-5, -6);
            GraphNode node6 = new GraphNode(1, -6);
            GraphNode node7 = new GraphNode(2, -6);
            GraphNode node8 = new GraphNode(7, -6);
            GraphNode node9 = new GraphNode(11, -6);
            GraphNode node10 = new GraphNode(12, -6);
            GraphNode node11 = new GraphNode(-7, -4);
            GraphNode node12 = new GraphNode(-2, -4);
            GraphNode node13 = new GraphNode(2, -4);
            GraphNode node14 = new GraphNode(7, -4);
            GraphNode node15 = new GraphNode(11, -4);
            GraphNode node16 = new GraphNode(12, -4);
            GraphNode node17 = new GraphNode(-12, -2);
            GraphNode node18 = new GraphNode(-10, -2);
            GraphNode node19 = new GraphNode(-7, -2);
            GraphNode node20 = new GraphNode(-2, -2);
            GraphNode node21 = new GraphNode(2, -2);
            GraphNode node22 = new GraphNode(5, -2);
            GraphNode node23 = new GraphNode(9, -2);
            GraphNode node24 = new GraphNode(11, -2);
            GraphNode node25 = new GraphNode(-7, 0);
            GraphNode node26 = new GraphNode(-4, 0);
            GraphNode node27 = new GraphNode(-2, 0);
            GraphNode node28 = new GraphNode(0, 0);
            GraphNode node29 = new GraphNode(3, 0);
            GraphNode node30 = new GraphNode(5, 0);
            GraphNode node31 = new GraphNode(7, 0);
            GraphNode node32 = new GraphNode(9, 0);
            GraphNode node33 = new GraphNode(11, 0);
            GraphNode node34 = new GraphNode(12, 0);
            GraphNode node35 = new GraphNode(-13, 1);
            GraphNode node36 = new GraphNode(-12, 1);
            GraphNode node37 = new GraphNode(-10, 1);
            GraphNode node38 = new GraphNode(3, 2);
            GraphNode node39 = new GraphNode(6, 2);
            GraphNode node40 = new GraphNode(9, 2);
            GraphNode node41 = new GraphNode(12, 2);
            GraphNode node42 = new GraphNode(-13, 3);
            GraphNode node43 = new GraphNode(-9, 3);
            GraphNode node44 = new GraphNode(-7, 3);
            GraphNode node45 = new GraphNode(0, 3);
            GraphNode node46 = new GraphNode(3, 3);
            GraphNode node47 = new GraphNode(-13, 5);
            GraphNode node48 = new GraphNode(-9, 5);
            GraphNode node49 = new GraphNode(-4, 5);
            GraphNode node50 = new GraphNode(0, 5);
            GraphNode node51 = new GraphNode(3, 5);
            GraphNode node52 = new GraphNode(6, 5);
            GraphNode node53 = new GraphNode(9, 5);
            GraphNode node54 = new GraphNode(12, 5);
            GraphNode node55 = new GraphNode(7, 2);

            node1.SetNeighbors(node2, null, null, null);
            node2.SetNeighbors(node3, node1, node17, null);
            node3.SetNeighbors(node4, node2, node18, null);
            node4.SetNeighbors(node5, node3, node11, null);
            node5.SetNeighbors(null, node4, null, null);
            node6.SetNeighbors(node7, null, null, null);
            node7.SetNeighbors(node8, node6, node13, null);
            node8.SetNeighbors(node9, node7, node14, null);
            node9.SetNeighbors(node10, node8, node15, null);
            node10.SetNeighbors(null, node9, null, null);
            node11.SetNeighbors(node12, null, node19, node4);
            node12.SetNeighbors(node13, node11, node20, null);
            node13.SetNeighbors(node14, node12, node21, node7);
            node14.SetNeighbors(node15, node13, node31, node8);
            node15.SetNeighbors(node16, node14, node24, node9);
            node16.SetNeighbors(null, node15, null, null);
            node17.SetNeighbors(node18, null, node36, node2);
            node18.SetNeighbors(node19, node17, node37, node3);
            node19.SetNeighbors(node20, node18, node25, node11);
            node20.SetNeighbors(null, node19, node27, node12);
            node21.SetNeighbors(node22, null, null, node13);
            node22.SetNeighbors(null, node21, node30, null);
            node23.SetNeighbors(node24, null, node32, null);
            node24.SetNeighbors(null, node23, node33, node15);
            node25.SetNeighbors(node26, null, node44, node19);
            node26.SetNeighbors(node27, node25, node49, null);
            node27.SetNeighbors(null, node26, null, node20);
            node28.SetNeighbors(node29, null, node45, null);
            node29.SetNeighbors(null, node28, node38, null);
            node30.SetNeighbors(node31, null, null, node22);
            node31.SetNeighbors(node32, node30, node55, node14);
            node32.SetNeighbors(null, node31, null, node23);
            node33.SetNeighbors(node34, null, null, node24);
            node34.SetNeighbors(null, node33, node41, null);
            node35.SetNeighbors(node36, null, node42, null);
            node36.SetNeighbors(node37, node35, null, node17);
            node37.SetNeighbors(null, node36, null, node18);
            node38.SetNeighbors(node39, null, node46, node29);
            node39.SetNeighbors(node55, node38, node52, null);
            node40.SetNeighbors(node41, node55, node53, null);
            node41.SetNeighbors(null, node40, node54, node34);
            node42.SetNeighbors(node43, null, node47, node35);
            node43.SetNeighbors(node44, node42, node48, null);
            node44.SetNeighbors(null, node43, null, node25);
            node45.SetNeighbors(node46, null, node50, node28);
            node46.SetNeighbors(null, node45, node51, node38);
            node47.SetNeighbors(null, null, null, node42);
            node48.SetNeighbors(node49, null, null, node43);
            node49.SetNeighbors(node50, node48, null, node26);
            node50.SetNeighbors(null, node49, null, node45);
            node51.SetNeighbors(node52, null, null, node46);
            node52.SetNeighbors(node53, node51, null, node39);
            node53.SetNeighbors(node54, node52, null, node40);
            node54.SetNeighbors(null, node53, null, node41);
            node55.SetNeighbors(node40, node39, null, node31);


            coldIceMap.Add(node1);
            coldIceMap.Add(node2);
            coldIceMap.Add(node3);
            coldIceMap.Add(node4);
            coldIceMap.Add(node5);
            coldIceMap.Add(node6);
            coldIceMap.Add(node7);
            coldIceMap.Add(node8);
            coldIceMap.Add(node9);
            coldIceMap.Add(node10);
            coldIceMap.Add(node11);
            coldIceMap.Add(node12);
            coldIceMap.Add(node13);
            coldIceMap.Add(node14);
            coldIceMap.Add(node15);
            coldIceMap.Add(node16);
            coldIceMap.Add(node17);
            coldIceMap.Add(node18);
            coldIceMap.Add(node19);
            coldIceMap.Add(node20);
            coldIceMap.Add(node21);
            coldIceMap.Add(node22);
            coldIceMap.Add(node23);
            coldIceMap.Add(node24);
            coldIceMap.Add(node25);
            coldIceMap.Add(node26);
            coldIceMap.Add(node27);
            coldIceMap.Add(node28);
            coldIceMap.Add(node29);
            coldIceMap.Add(node30);
            coldIceMap.Add(node31);
            coldIceMap.Add(node32);
            coldIceMap.Add(node33);
            coldIceMap.Add(node34);
            coldIceMap.Add(node35);
            coldIceMap.Add(node36);
            coldIceMap.Add(node37);
            coldIceMap.Add(node38);
            coldIceMap.Add(node39);
            coldIceMap.Add(node40);
            coldIceMap.Add(node41);
            coldIceMap.Add(node42);
            coldIceMap.Add(node43);
            coldIceMap.Add(node44);
            coldIceMap.Add(node45);
            coldIceMap.Add(node46);
            coldIceMap.Add(node47);
            coldIceMap.Add(node48);
            coldIceMap.Add(node49);
            coldIceMap.Add(node50);
            coldIceMap.Add(node51);
            coldIceMap.Add(node52);
            coldIceMap.Add(node53);
            coldIceMap.Add(node54);
            coldIceMap.Add(node55);
        }
    }

    public static int GetDimension()
    {
        return map.Count;
    }
}
