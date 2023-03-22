using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State { Wait = 0, Processing, End}

public class Board : MonoBehaviour
{
    [SerializeField]
    private NodeSpawner     nodeSpawner;
    [SerializeField]
    private TouchController touchController;
    [SerializeField]
    private UIController    uIController;
    [SerializeField]
    private GameObject      blockPrefab;
    [SerializeField]
    private Transform       blockRect;

    public List<Node>       NodeList      { private set; get; }
    public Vector2Int       BlockCount    { private set; get; }

    private List<Block>     blockList;

    private State           state = State.Wait;
    private int             currentScore;
    private int             highScore;
    private float           blockSize;

    private void Awake()
    {
        int count = PlayerPrefs.GetInt("BlockCount");
        BlockCount = new Vector2Int(count, count);

        blockSize = (1080 - 85 - 25 * (BlockCount.x - 1)) / BlockCount.x;

        currentScore = 0;
        uIController.UpdateCurrentScore(currentScore);

        highScore = PlayerPrefs.GetInt("HighScore");
        uIController.UpdateHighScore(highScore);

        NodeList    = nodeSpawner.SpawnNodes(this, BlockCount, blockSize);

        blockList   = new List<Block>();
    }

    private void Start()
    {
        //�׸��� ���̾ƿ��� ���� ���ĵ� UI�� ������ġ�� �ٸ��� ���̶���� ���� �����ǰ��ֱ� ������
        // �����带�Ͽ� ��ġ�� ��������� �Ѵ�.
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(nodeSpawner.GetComponent<RectTransform>());

        foreach (Node node in NodeList)
        {
            // Vector2 ���� localPosition�� ������ �� ��ġ���� �������ش�.
            node.localPosition = node.GetComponent<RectTransform>().localPosition;
        }

        SpawnBlockToRandomNode();
        SpawnBlockToRandomNode();
    }

    private void Update()
    {
        if (state == State.Wait)
        {
            Direction direction = touchController.UpdateTouch();

            if (direction != Direction.None)
            {
                AllBlocksProcess(direction);
            }
        }
        else
        {
            UpdateState();
        }
    }

    private void SpawnBlockToRandomNode()
    {
        //NodeList�� ����� placedBlock�� null�� ��带 ��� emptyNodes�� �־��ش�. �� ��ġ���� ���� Node
        List<Node> emptyNodes = NodeList.FindAll(x => x.placedBlock == null);

        // emptyNodes�� ������
        if (emptyNodes.Count != 0)
        {
            // emptyNodes�� ������ node�� ������ �����ͼ�
            int         index = Random.Range(0, emptyNodes.Count);
            Vector2Int  point = emptyNodes[index].Point;
            // �ش� ��� ��ġ�� ���� ����
            SpawnBlock(point.x, point.y);
        }
        else
        {
            if (IsGameOver())
            {
                OnGameOver();
            }
        }
    }

    private void SpawnBlock(int x, int y)
    {
        // y*BlockCount.x+x ������ index�� ã�� ��
        if (NodeList[y * BlockCount.x + x].placedBlock != null) return;

        GameObject  clone   = Instantiate(blockPrefab, blockRect);
        Block       block   = clone.GetComponent<Block>();
        Node        node    = NodeList[y * BlockCount.x + x];

        clone.GetComponent<RectTransform>().sizeDelta = new Vector2(blockSize, blockSize);

        clone.GetComponent<RectTransform>().localPosition = node.localPosition;

        block.Setup();

        node.placedBlock    = block;

        blockList.Add(block);
    }

    private void AllBlocksProcess(Direction direction)
    {
        if (direction == Direction.Right)
        {
            for (int y = 0; y < BlockCount.y; y++)
            {
                for (int x = (BlockCount.x-2); x >= 0; -- x)
                {
                    BlockProcess(NodeList[y * BlockCount.x + x], direction);
                }
            }

        }
        else if (direction == Direction.Down)
        {
            for (int y = (BlockCount.y-2); y >= 0; -- y)
            {
                for (int x = 0; x < BlockCount.x; x++)
                {
                    BlockProcess(NodeList[y * BlockCount.x + x], direction);
                }
            }
        }
        else if (direction == Direction.Left)
        {
            for (int y = 0; y < BlockCount.y; y++)
            {
                for (int x = 1; x < BlockCount.x; x++)
                {
                    BlockProcess(NodeList[y * BlockCount.x + x], direction);
                }
            }
        }
        else if (direction == Direction.Up)
        {
            for (int y = 1; y < BlockCount.y; y++)
            {
                for (int x = 0; x < BlockCount.x; x++)
                {
                    BlockProcess(NodeList[y * BlockCount.x + x], direction);
                }
            }
        }

        foreach (Block block in blockList)
        {
            if (block.Target != null)
            {
                state = State.Processing;
                block.StartMove();
            }
        }

        if (IsGameOver())
        {
            OnGameOver();
        }
    }

    private void BlockProcess(Node node, Direction direction)
    {
        if (node.placedBlock == null) return;

        Node neighborNode = node.FindTarget(node, direction);
        if (neighborNode != null)
        {
            if (node.placedBlock != null && neighborNode.placedBlock != null)
            {
                if (node.placedBlock.Numeric == neighborNode.placedBlock.Numeric)
                {
                    Combine(node, neighborNode);
                }
            }
            else if (neighborNode != null && neighborNode.placedBlock == null)
            {
                Move(node, neighborNode);
            }
        }
    }

    private void Move(Node from, Node to)
    {
        from.placedBlock.MoveToNode(to);

        if (from.placedBlock != null)
        {
            to.placedBlock = from.placedBlock;

            from.placedBlock = null;
        }
    }

    private void Combine(Node from, Node to)
    {
        from.placedBlock.CombineToNode(to);
        from.placedBlock    = null;
        to.combined         = true;
    }

    private void UpdateState()
    {
        bool targetAllNull = true;

        foreach (Block block in blockList)
        {
            if (block.Target != null)
            {
                targetAllNull = false;
                break;
            }
        }

        if (targetAllNull && state == State.Processing)
        {
            // ������Ʈ���� ���� ����Ʈ�� �����Ұ�� ������� ������ ���� �ٸ� ������� ��� �� �� �ֱ� ������
            // �����͸� �̵��� ���� ����� ���� �����ϴ�.
            List<Block> removeBlocks = new List<Block>();
            foreach (Block block in blockList)
            {
                if (block.NeedDestroy)
                {
                    removeBlocks.Add(block);
                }
            }

            removeBlocks.ForEach(x =>
            {
                currentScore += x.Numeric * 2;
                blockList.Remove(x);
                Destroy(x.gameObject);
            });

            state = State.End;
        }

        if (state == State.End)
        {
            state = State.Wait;

            SpawnBlockToRandomNode();

            NodeList.ForEach(x => x.combined = false);

            uIController.UpdateCurrentScore(currentScore);
        }
    }

    private bool IsGameOver()
    {
        foreach (Node node in NodeList)
        {
            if (node.placedBlock == null) return false;

            for (int i = 0; i < node.NeighborNodes.Length; i++)
            {
                if (node.NeighborNodes[i] == null) continue;

                Vector2Int point = node.NeighborNodes[i].Value;
                Node neighborNode = NodeList[point.y * BlockCount.x + point.x];

                if (node.placedBlock != null && neighborNode.placedBlock != null)
                {
                    if ( node.placedBlock.Numeric == neighborNode.placedBlock.Numeric)
                    {
                        return false;
                    }
                }
            }
        }

        return true;

    }

    private void OnGameOver()
    {
        //Debug.Log("GameOver");

        if (currentScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
        }

        uIController.OnGameOver();
    }
}
