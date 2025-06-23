using UnityEngine;
using UnityEngine.Pool;


public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    public ObjectPool<BlockView> blockPool { get; private set; }
    public ObjectPool<PieceView> piecePool { get; private set; }

    private const int defBlocksCap = 1000;
    private const int maxBlocksCap = 1000;

    private const int defPieceCap = 2000;
    private const int maxPieceCap = 2000;


    private void Awake()
    {
        Instance = this;

        blockPool = new ObjectPool<BlockView>(() => {
            return Instantiate(Tetro.Instance.blockPrefab);
        }, block => {
            block.gameObject.SetActive(true);
            block.GetComponent<SpriteRenderer>().enabled = true;
        }, block => {
            block.gameObject.SetActive(false);
            block.transform.SetParent(null, false);
        }, block => {
            Destroy(block.gameObject);
        }, true, defBlocksCap, maxBlocksCap);

        piecePool = new ObjectPool<PieceView>(() => {
            return Instantiate(Tetro.Instance.piecePrefab);
        }, piece => {
            piece.gameObject.SetActive(true);
        }, piece => {
            piece.blockViews.Clear();
            var rb = piece.gameObject.GetComponent<Rigidbody2D>();
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            piece.gameObject.SetActive(false);
        }, piece => {
            Destroy(piece.gameObject);
        }, true, defPieceCap, maxPieceCap);
    }
}
