using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class BoardBack : MonoBehaviour
{
    [Header("Board Size")]
    [SerializeField] private int boardW;
    [SerializeField] private int boardH;

    [Header("Board Visuals")]
    [SerializeField] private Sprite backSprite;

    [Header("Board Options")]
    [SerializeField] private int lineLength;
    [SerializeField] private float fadingRate;
    [SerializeField] private float alphaTreshold;


    [Header("Debug")]
    [SerializeField] private SArray<BoardBlock> arrayS;
    //blocks activated by the moving pieces
    [SerializeField] private List<Vector2Int> activeBlocks;
    [SerializeField] private bool[] linesToCheck;
    [SerializeField] private List<Vector2Int> lineBlocks;
    [SerializeField] private List<PieceView> piecesToSplit;


    public void Reset()
    {
        for (int y = 0; y < boardH; y++)
        {
            for (int x = 0; x < boardW; x++)
            {
                arrayS[x, y].state = BlockState.Empty;

                arrayS[x, y].Color = new Color(0f, 0f, 0f, 0f);

                var blockView = arrayS[x, y];
                var rb = blockView.gameObject.GetComponent<Rigidbody2D>();
                rb.simulated = false;
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                blockView.transform.localPosition = BlockPositionRelative(new Vector2Int(x, y));
                blockView.transform.SetParent(transform, false);
                blockView.transform.rotation = Quaternion.identity;
            }
        }

        Array.Clear(linesToCheck, 0, boardH);
        activeBlocks.Clear();
        lineBlocks.Clear();
        piecesToSplit.Clear();
    }

    private void Generate()
    {
        var blockSize = BlockSize();

        for (int y = 0; y < boardH; y++)
        {
            for (int x = 0; x < boardW; x++)
            {
                var blockView = Instantiate(Tetro.Instance.boardBlockPrefab, transform, false);

                blockView.state = BlockState.Empty;
                blockView.piece = null;
                arrayS[x, y] = blockView;

                blockView.Sprite = backSprite;
                blockView.Color = new Color(0f, 0f, 0f, 0f);
                blockView.block = new Vector2Int();
                blockView.Size = blockSize;
                blockView.transform.localPosition = BlockPositionRelative(new Vector2Int(x, y));
            }
        }
    }

    //TODO what happens if Game Over during Blowup
    public void Blowup()
    {
        for (int y = 0; y < boardH; y++)
        {
            for (int x = 0; x < boardW; x++)
            {
                if (arrayS[x, y].state != BlockState.Empty)
                {
                    arrayS[x, y].gameObject.GetComponent<Rigidbody2D>().simulated = true;
                    arrayS[x,y ].transform.SetParent(null);
                }
            }
        }
    }

    //once created in editor use as a prefab
    private void Awake()
    {
#if UNITY_EDITOR
        arrayS = new SArray<BoardBlock>(boardW, boardH);
        linesToCheck = new bool[boardH];

        activeBlocks = new List<Vector2Int>();
        lineBlocks = new List<Vector2Int>();
        piecesToSplit = new List<PieceView>();
#endif
    }

    private void Start()
    {
#if UNITY_EDITOR
        Generate();
#endif
        float size = Tetro.Instance.blockSize;
        float s = size * 0.5f;

        var polygonCollider = GetComponent<PolygonCollider2D>();
        polygonCollider.points = new []{
            new Vector2(-boardW, -boardH) * s,
            new Vector2(-boardW, boardH) * s,
            new Vector2(boardW, boardH) * s,
            new Vector2(boardW, -boardH) * s
        };

    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        OnTriggerStay2D(col);
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        OnTriggerStay2D(col);
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Piece"))
        {
            var activePiece = col.GetComponent<PieceView>();
            if (activePiece == null)
            {
                return;
            }

            var vel = activePiece.GetComponent<Rigidbody2D>().velocity;
            var aaa = (Math.Sin(Math.Atan2(vel.x, vel.y) * 4) + 1) * 0.5;
            activePiece.Color = Color.HSVToRGB((float)aaa, 1f, 1f);


            var angle = Tetro.DiscretizeAngle(col.transform.eulerAngles.z);
            var oldEuler = col.transform.eulerAngles;
            col.transform.eulerAngles = new Vector3(0.0f, 0.0f, angle);


            var blocks = activePiece.blockViews;
            foreach (var block in blocks)
            {
                block.Show();

                Vector2Int gc = ToGridCoords(block.transform.position);
                block.block = gc;
                if (InBounds(gc))
                {
                    if (arrayS[gc].piece != null)
                    {
                        if (activePiece == arrayS[gc].piece)
                        {
                            Debug.LogWarning("Same Piece Collision!");
                        }
                    }
                    else
                    {
                        activeBlocks.Add(new Vector2Int(gc.x, gc.y));
                        linesToCheck[gc.y] = true;
                        ActivateBlock(gc, activePiece.Color);
                        arrayS[gc].piece = activePiece;
                    }
                    block.Hide();
                }
            }
            col.gameObject.transform.eulerAngles = oldEuler;
        }
    }

    private void FixedUpdate()
    {
        Sweep();
        foreach (var activeBlock in activeBlocks)
        {
            DeactivateBlock(activeBlock);
            arrayS[activeBlock].piece = null;
        }
        activeBlocks.Clear();
    }

    private void Update()
    {
        for (int y = 0; y < boardH; y++)
        {
            for (int x = 0; x < boardW; x++)
            {
                var color = arrayS[x, y].Color;
                color.a = Math.Max(0f, color.a - Time.deltaTime * fadingRate);
                if (color.a < alphaTreshold)
                {
                    arrayS[x, y].state = BlockState.Empty;
                }
                arrayS[x, y].Color = color;
            }
        }
    }

    private void Highlight(Vector2Int v)
    {
        if (InBounds(v))
        {
            arrayS[v].Color = (arrayS[v].Color * new Color(2.1f, 2.1f, 2.1f, 1.0f));
        }
    }

    private void ActivateBlock(Vector2Int v, Color color)
    {
        if (InBounds(v))
        {
            arrayS[v].Color = color;

            if (arrayS[v].state == BlockState.Empty)
            {
                arrayS[v].state = BlockState.New;
            }
            else if (arrayS[v].state == BlockState.New && arrayS[v].piece == null)
            {
                arrayS[v].state= BlockState.Set;
            }
        }
    }

    private void DeactivateBlock(BlockView block)
    {
        Color color = block.Color;
        color.a = 0.5f;
        block.Color = color;
    }

    private void DeactivateBlock(Vector2Int v)
    {
        if (InBounds(v))
        {
            DeactivateBlock(arrayS[v]);
        }
    }

    private void CheckLine()
    {
        if (lineBlocks.Count >= lineLength)
        {
            piecesToSplit.Clear();
            int removedActiveBlocks = 0;
            foreach (var block in lineBlocks)
            {
                if (arrayS[block].state == BlockState.New && activeBlocks.Contains(new Vector2Int(block.x, block.y)))
                {
                    if (arrayS[block].piece)
                    {
                        arrayS[block].piece.RemoveBlock(new Vector2Int(block.x, block.y));
                        removedActiveBlocks++;
                        if (piecesToSplit.Contains(arrayS[block].piece) == false)
                        {
                            piecesToSplit.Add(arrayS[block].piece);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Null Piece!");
                    }
                }
            }

            foreach (var piece in piecesToSplit)
            {
                piece.Split();
            }

            SgLib.ScoreManager.Instance.AddScore(lineLength * (5 - removedActiveBlocks));

            StartCoroutine(BlowLine(lineBlocks.ToArray()));
            SgLib.SoundManager.Instance.PlaySound(SgLib.SoundManager.Instance.rewarded);
        }
    }

    private void Sweep()
    {
        lineBlocks.Clear();
        for (int y=0; y<boardH; y++)
        {
            if (linesToCheck[y])
            {
                for (int x=0; x<boardW; x++)
                {
                    if (arrayS[x, y].Color.a > alphaTreshold && arrayS[x, y].state != BlockState.Busy)
                    {
                        lineBlocks.Add(new Vector2Int(x, y));
                    }
                    else
                    {
                        CheckLine();
                        lineBlocks.Clear();
                    }
                }
                CheckLine();
                lineBlocks.Clear();
                linesToCheck[y] = false;
            }
        }
    }

    private IEnumerator BlowLine(Vector2Int[] line)
    {
        foreach (var block in line)
        {
            arrayS[block].state = BlockState.Busy;
        }

        for (float a =0.5f; a <= 1f; a += 0.1f)
        {
            foreach (var block in line)
            {
                arrayS[block].Color = new Color(a, a, a, a);
            }
            yield return new WaitForSeconds(.05f);
        }

        foreach (var block in line)
        {
            arrayS[block].Color = new Color(0f, 0f, 0f, 0f);
            arrayS[block].state = BlockState.Empty;
        }
    }

    private bool InBounds(Vector2Int v)
    {
        return (v.x >= 0 && v.x < boardW && v.y >= 0 && v.y < boardH);
    }

    private Vector2Int ToGridCoords(Vector3 v)
    {
        var blockSize = BlockSize();
        Vector3 gridCorner = BlockPosition(new Vector2Int(0, 0)) - 0.5f * new Vector3(blockSize, blockSize);
        Vector3 d = v - gridCorner;
        return new Vector2Int((int)Math.Floor(d.x / blockSize), (int)Math.Floor(d.y / blockSize));
    }

    private Vector3 BlockPosition(Vector2Int block)
    {
        return BlockPositionRelative(block) + transform.position;
    }

    private Vector3 BlockPositionRelative(Vector2Int block)
    {
        var size = BlockSize();
        var position = new Vector3(block.x * size, block.y * size);
        return position - PivotOffset() + 0.5f * new Vector3(size, size);
    }

    private float BlockSize()
    {
        return Tetro.Instance.blockSize;
    }

    private Vector3 PivotOffset()
    {
        float realWidth = boardW * BlockSize();
        float realHeight = boardH * BlockSize();

        return new Vector3(realWidth * 0.5f, realHeight * 0.5f);
    }


    public enum BlockState
    {
        Empty,
        Busy,
        New,
        Set
    }
}
