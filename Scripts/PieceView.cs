using System;
using System.Collections.Generic;

using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class PieceView : MonoBehaviour
{
    public PieceType pieceType;

    public List<BlockView> blockViews;


    public delegate void PieceEventHandler(PieceView view);

    public static event PieceEventHandler PieceLavaEvent = delegate {};
    public static event PieceEventHandler PieceDisappearEvent = delegate {};
    public static event PieceEventHandler PieceDestroyEvent = delegate {};
    public static event PieceEventHandler PieceSplitEvent = delegate {};
    public static event PieceEventHandler PieceRemoveBlockEvent = delegate {};

    public Rigidbody2D rb;


    public int BlockCount
    {
        get => blockViews.Count;
    }

    public Color Color { get; set; }


    public void GeneratePiece(Piece piece) => GeneratePiece(piece.Blocks, piece.Type);

    public void GeneratePiece(Vector2Int[] blocks, PieceType type)
    {
        var blockSize = BlockSize();
        Color = Tetro.Instance.blockColors[(int)type];

        foreach (var block in blocks)
        {
            var blockView = PoolManager.Instance.blockPool.Get();
            blockView.block = block;
            blockView.Color = Color;
            blockView.Sprite = BlockSprite();
            blockView.Size = blockSize;

            blockView.transform.position = BlockPosition(block, blockSize);

            blockViews.Add(blockView);

            blockView.transform.SetParent(transform, false);
        }
    }

    public void Assemble(BlockView[] blocks, PieceType type)
    {
        Color = Tetro.Instance.blockColors[(int)type];

        foreach (var block in blocks)
        {
            blockViews.Add(block);
            block.transform.SetParent(transform);
        }
    }

    public void RemoveBlock(Vector2Int v)
    {
        foreach (var blockView in blockViews)
        {
            if (blockView.block == v)
            {
                PieceRemoveBlockEvent(this);
                PoolManager.Instance.blockPool.Release(blockView);
            }
        }
        var removedCount = blockViews.RemoveAll(blockView => blockView.block == v);
        if (removedCount > 0 && blockViews.Count == 0)
        {
            PieceDisappearEvent(this);
        }

    }

    public void SetBlockViews(Piece piece)
    {
        SetBlockViews(piece.Blocks);
    }

    public void SetBlockViews(Vector2Int[] blocks)
    {
        for (int i=0; i<blocks.Length; i++)
        {
            blockViews[i].block = blocks[i];
        }
    }

    public void DestroyPiece()
    {
        PieceDestroyEvent(this);
        Clear();
    }

    public void Split()
    {
        if (blockViews.Count <= 1 || blockViews.Count == 4)
        {
            return;
        }
        else if (blockViews.Count == 2)
        {
            if (Continuous(blockViews[0].block, blockViews[1].block))
            {
                return;
            }
            else
            {
                SpawnPieceFromBlocks(new []{blockViews[0]});
                SpawnPieceFromBlocks(new []{blockViews[1]});
            }
        }
        else if (blockViews.Count == 3)
        {
            bool c01, c12, c20;
            c01 = Continuous(blockViews[0].block, blockViews[1].block);
            c12 = Continuous(blockViews[1].block, blockViews[2].block);
            c20 = Continuous(blockViews[2].block, blockViews[0].block);

            if((c01 && c12) || (c12 && c20) || (c20 && c01))
            {
                return;
            }
            else if(c01)
            {
                SpawnPieceFromBlocks(new []{blockViews[0], blockViews[1]});
                SpawnPieceFromBlocks(new []{blockViews[2]});
            }
            else if(c12)
            {
                SpawnPieceFromBlocks(new []{blockViews[0]});
                SpawnPieceFromBlocks(new []{blockViews[1], blockViews[2]});
            }
            else if(c20)
            {
                SpawnPieceFromBlocks(new []{blockViews[1]});
                SpawnPieceFromBlocks(new []{blockViews[2], blockViews[0]});
            }
            else
            {
                SpawnPieceFromBlocks(new []{blockViews[0]});
                SpawnPieceFromBlocks(new []{blockViews[1]});
                SpawnPieceFromBlocks(new []{blockViews[2]});
            }
        }
        else
        {
            //ERROR
            return;
        }

        PieceDisappearEvent(this);

        bool Continuous(Vector2Int a, Vector2Int b) => Vector2.Distance(a, b) > 1 ? false : true;
    }


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Dead"))
        {
            PieceLavaEvent(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Dead"))
        {
            PieceLavaEvent(this);
        }
    }

    private void Clear()
    {
        foreach (var blockView in blockViews)
        {
            PoolManager.Instance.blockPool.Release(blockView);
        }
        blockViews.Clear();
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void SpawnPieceFromBlocks(BlockView[] blocks)
    {
        var piece = Tetro.Instance.SpawnPiece(pieceType, blocks,
                                              transform.position,
                                              transform.rotation,
                                              rb.velocity, rb.angularVelocity);
        PieceSplitEvent(piece);
    }

    private Vector3 BlockPosition(Vector2Int block, float blockSize)
    {
        return new Vector3(block.x * blockSize, block.y * blockSize);
    }

    private float BlockSize()
    {
        return Tetro.Instance.blockSize;
    }

    private Sprite BlockSprite()
    {
        return Tetro.Instance.blockUSprite;
    }
}
