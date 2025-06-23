using System.Collections.Generic;

using UnityEngine;


public class Tetro : MonoBehaviour
{
    public static Tetro Instance { get; private set; }

    public BlockView blockPrefab;
    public BoardBlock boardBlockPrefab;
    public PieceView piecePrefab;

    public Sprite[] blockSprites;
    public Color[] blockColors;

    public Sprite blockUSprite;

    public float blockSize;


    public static float DiscretizeAngle(float angle)
    {
        angle = angle % 360;
        if (angle >= 45 && angle < 135)
        {
            return 90;
        }
        else if (angle >= 135 && angle < 225)
        {
            return 180;
        }
        else if (angle >= 225 && angle < 315)
        {
            return 270;
        }
        else // (angle >= 315 || angle < 45)
        {
            return 0;
        }
    }

   public PieceView SpawnPiece(PieceType type, Vector3 position, Quaternion rotation, Vector2 force)
   {
       var pieceView = PoolManager.Instance.piecePool.Get();
       pieceView.transform.SetPositionAndRotation(position, rotation);
       pieceView.rb.AddRelativeForce(force, ForceMode2D.Impulse);

       pieceView.pieceType = type;
       pieceView.GeneratePiece(AvailablePieces.NewPiece(type));

       return pieceView;
   }

   public PieceView SpawnPiece(PieceType type, BlockView[] blocks, Vector3 position, Quaternion rotation, Vector2 velocity, float angularVelocity)
   {
       var pieceView = PoolManager.Instance.piecePool.Get();

       pieceView.transform.SetPositionAndRotation(position, rotation);
       pieceView.rb.velocity = velocity;
       pieceView.rb.angularVelocity = angularVelocity;

       pieceView.pieceType = type;
       pieceView.Assemble(blocks, type);

       return pieceView;
   }

   private void Awake()
   {
       Instance = this;
       blockColors = new Color[blockSprites.GetLength(0)];
       for (int i=0; i<blockSprites.GetLength(0); i++)
       {
           blockColors[i] = blockSprites[i].texture.GetPixel(5, 5);
       }
   }
}
