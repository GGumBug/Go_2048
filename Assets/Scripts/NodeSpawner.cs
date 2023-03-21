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

        //������� ���� ���� ������ ���ϴ� Vector2Int
        List<Node> nodeList = new List<Node>(blockCount.x * blockCount.y);

        // �׸��� ���̾ƿ����� ���� ���� ������ ���η� �ϳ��� ������Ʈ�� ���δ�.
        for (int y = 0; y < blockCount.y; y++)
        {
            for (int x = 0; x < blockCount.x; x++)
            {
                GameObject clone = Instantiate(nodePrefab, nodeRect.transform);

                // ���� ��� ��ġ ��
                Vector2Int point = new Vector2Int(x, y);

                // ���� ��� ���� ���� (���� ��� ������ null ����)
                Vector2Int?[] neighborNodes = new Vector2Int?[4];

                // �׸��� ���̾ƿ� �׷��� StartCorner�� UpperLeft�� ��� y���� up������ -, down ������ + �̱� ������
                // down ������ Vector2Int.up ���� up ������ Vector2Int.down ���� �����ش�.
                Vector2Int right = point + Vector2Int.right;
                Vector2Int down = point + Vector2Int.up;
                Vector2Int left = point + Vector2Int.left;
                Vector2Int up = point + Vector2Int.down;
                

                if (IsValid(right, blockCount)) neighborNodes[0] = right;
                if (IsValid(down, blockCount))  neighborNodes[1] = down;
                if (IsValid(left, blockCount))  neighborNodes[2] = left;
                if (IsValid(up, blockCount))    neighborNodes[3] = up;

                // ���� �� ����� ��ġ���� SetUp���� ��忡 �ο��Ѵ�.
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
