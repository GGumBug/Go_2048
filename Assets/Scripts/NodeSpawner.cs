using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject      nodePrefab;
    [SerializeField]
    private RectTransform   nodeRect;
    [SerializeField]
    private GridLayoutGroup GridLayoutGroup;

    public List<Node> SpawnNodes(Board board, Vector2Int blockCount, float blockSize)
    {
        GridLayoutGroup.cellSize = new Vector2(blockSize, blockSize);

        //블록판의 가로 세로 개수를 정하는 Vector2Int
        List<Node> nodeList = new List<Node>(blockCount.x * blockCount.y);

        // 그리드 레이아웃으로 인해 왼쪽 위부터 가로로 하나씩 오브젝트가 쌓인다.
        for (int y = 0; y < blockCount.y; y++)
        {
            for (int x = 0; x < blockCount.x; x++)
            {
                GameObject clone = Instantiate(nodePrefab, nodeRect.transform);

                // 현재 노드 위치 값
                Vector2Int point = new Vector2Int(x, y);

                // 인접 노드 정보 저장 (인접 노드 없으면 null 저장)
                Vector2Int?[] neighborNodes = new Vector2Int?[4];

                // 그리드 레이아웃 그룹의 StartCorner가 UpperLeft일 경우 y축은 up방향이 -, down 방향이 + 이기 때문에
                // down 변수에 Vector2Int.up 값을 up 변수에 Vector2Int.down 값을 더해준다.
                Vector2Int right = point + Vector2Int.right;
                Vector2Int down = point + Vector2Int.up;
                Vector2Int left = point + Vector2Int.left;
                Vector2Int up = point + Vector2Int.down;
                

                if (IsValid(right, blockCount)) neighborNodes[0] = right;
                if (IsValid(down, blockCount))  neighborNodes[1] = down;
                if (IsValid(left, blockCount))  neighborNodes[2] = left;
                if (IsValid(up, blockCount))    neighborNodes[3] = up;

                // 생성 된 노드의 위치값을 SetUp으로 노드에 부여한다.
                Node node = clone.GetComponent<Node>();
                node.SetUp(board, neighborNodes, point);

                clone.name = $"[{node.Point.y}, {node.Point.x}]";

                nodeList.Add(node);
            }
        }

        return nodeList;
    }

    private bool IsValid(Vector2Int point, Vector2Int blockCount)
    {
        if (point.x == -1 || point.x == blockCount.x || point.y == blockCount.y || point.y == -1)
        {
            return false;
        }

        return true;
    }
}
