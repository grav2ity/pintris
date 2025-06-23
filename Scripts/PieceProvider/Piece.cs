using System;

using UnityEngine;

public enum PieceType
{
    I, J, L, O, S, T, Z
}

[Serializable]
public class Piece
{
    private const int tetro = 4;

    //block at 0 is considered to be origin == rotation invariant
    private Vector2Int[] blocks = new Vector2Int[tetro];

    public Piece(Vector2Int[] blocks, PieceType type)
    {
        Array.Copy(blocks, this.blocks, tetro);
        Type = type;
    }

    public Vector2Int[] Blocks
    {
        get => blocks;
    }

    public PieceType Type { get; private set; }
}
