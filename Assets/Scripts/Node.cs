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
    // ?[] 는 비어있는 값이 0이 아닌 null로 가질 수 있는 Nullable상태로 만들어준다.
    public Vector2Int?[]     NeighborNodes   { private set; get; } // 현재 노드에 인접한 노드의 격자 좌표 (없으면 null)

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

            // 인접노드에 블록이 없으면 재귀함수 호출 (여러 칸 이동을 위해)
            return neighborNodes.FindTarget(originalNode, direction, neighborNodes);
        }

        return falNode;
    }
}
