using System;
using System.Collections.Generic;


public class BalancedRandomPieceProvider : IPieceProvider
{
    private Random random = new Random();
    private List<int> pool = new List<int>();
    private const int NumDuplicates = 4;

    public Piece GetPiece() =>
        AvailablePieces.NewPiece((PieceType)GetPopulatedPool().TakeFirst());

    private List<int> GetPopulatedPool()
    {
        if (pool.Count == 0)
        {
            PopulatePool();
        }
        return pool;
    }

    private void PopulatePool()
    {
        for (var index = 0; index < Enum.GetNames(typeof(PieceType)).Length ; ++index)
        {
            pool.Add(index, NumDuplicates);
        }
        pool.Shuffle(random);
    }
}
