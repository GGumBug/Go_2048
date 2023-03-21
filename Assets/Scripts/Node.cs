using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction { None = -1, Right = 0, Down, Left, Up}

public class Node : MonoBehaviour
{
    public Block            placedBlock;
    public Vector2          localPosition;
    public bool             combined = false;

    public Vector2Int       Point           { private set; get; }
    // ?[] �� ����ִ� ���� 0�� �ƴ� null�� ���� �� �ִ� Nullable���·� ������ش�.
    public Vector2Int?[]     NeighborNodes   { private set; get; } // ���� ��忡 ������ ����� ���� ��ǥ (������ null)

    private Board           board;

    public void SetUp(Board board, Vector2Int?[] neighborNodes, Vector2Int point)
    {
        this.board      = board;
        NeighborNodes   = neighborNodes;
        Point           = point;
    }

    public Node FindTarget(Node originalNode, Direction direction, Node falNode=null)
    {
        if (NeighborNodes[(int)direction].HasValue == true)
        {
            Vector2Int  point           = NeighborNodes[(int)direction].Value;
            Node        neighborNodes   = board.NodeList[point.y * board.BlockCount.x + point.x];

            if (neighborNodes != null && neighborNodes.combined)
            {
                return this;
            }

            if (neighborNodes.placedBlock != null && originalNode.placedBlock != null)
            {
                if (neighborNodes.placedBlock.Numeric == originalNode.placedBlock.Numeric)
                {
                    return neighborNodes;
                }
                else
                {
                    return falNode;
                }
            }

            if (neighborNodes.placedBlock != null && originalNode.placedBlock != null)
            {
                return falNode;
            }

            // ������忡 ����� ������ ����Լ� ȣ�� (���� ĭ �̵��� ����)
            return neighborNodes.FindTarget(originalNode, direction, neighborNodes);
        }

        return falNode;
    }
}
