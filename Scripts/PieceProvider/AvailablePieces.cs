using UnityEngine;


public static class AvailablePieces
{
    public static Piece NewPiece(PieceType type)
    {
        return type switch
        {
            PieceType.I => IPiece,
            PieceType.J => JPiece,
            PieceType.L => LPiece,
            PieceType.O => OPiece,
            PieceType.S => SPiece,
            PieceType.T => TPiece,
            PieceType.Z => ZPiece,
            _ => ZPiece
        };
    }

    private static Piece OPiece = new Piece(
        new [] {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(0, 1)
                         }, PieceType.O);

    private static Piece TPiece = new Piece(
        new [] {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1)
                          }, PieceType.T);

    private static Piece SPiece = new Piece(
        new [] {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 1)
                         }, PieceType.S);

    private static Piece ZPiece = new Piece(
        new [] {
            new Vector2Int(0, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1)
                         }, PieceType.Z);

    private static Piece JPiece = new Piece(
        new [] {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(-1, -1)
                         }, PieceType.J);

    private static Piece LPiece = new Piece(
        new [] {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(-1, 1)
                         }, PieceType.L);

    private static Piece IPiece = new Piece(
        new [] {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(-2, 0)
                         }, PieceType.I);
}
