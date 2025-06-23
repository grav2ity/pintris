using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using SgLib;



public class GameManager : MonoBehaviour
{
    public static event System.Action<GameState, GameState> GameStateChanged = delegate {};


    [Header("Gameplay References")]
    public GameObject ballPoint;
    [SerializeField] private UIManager uIManager;
    [SerializeField] private GameObject leftFlipper;
    [SerializeField] private GameObject rightFlipper;
    [SerializeField] private List<BoardBack> boards;

    [Header("Gameplay Config")]
    [SerializeField] private float torqueForce;
    // [SerializeField] private int scoreToIncreaseDifficulty;

    [Tooltip("Game Over immunity time")]
    [SerializeField] private float gracePeriod;
    [SerializeField] private float timeToAddPiece;
    [SerializeField] private bool gameOverBlowUp;

    [Header("Shake")]
    [SerializeField] private Vector2 shakeForce;
    [SerializeField] private int maxShakeCount;
    [SerializeField] private float shakeTime;

    private int shakeCount;
    private float lastShakeTime;

    private bool leftFlipperAction;
    private bool rightFlipperAction;

    private Rigidbody2D leftFlipperRigid;
    private Rigidbody2D rightFlipperRigid;

    private new Camera camera;

    private IPieceProvider pieceProvider;
    private List<PieceView> piecesInPlay = new List<PieceView>();
    private int blocksInPlay;

    private float startTime;
    private int dPieceBlocksCount;

    private Coroutine spawn;

    private PieceView.PieceEventHandler pieceLavaEvent;
    private PieceView.PieceEventHandler pieceDisappearEvent;
    private PieceView.PieceEventHandler pieceDestroyEvent;
    private PieceView.PieceEventHandler pieceSplitEvent;
    private PieceView.PieceEventHandler pieceRemoveBlockEvent;

    private GameState _gameState = GameState.InMenu;

    public GameState GameState
    {
        get
        {
            return _gameState;
        }
        private set
        {
            if (value != _gameState)
            {
                GameState oldState = _gameState;
                _gameState = value;

                GameStateChanged(_gameState, oldState);
            }
        }
    }


    public void StartGame()
    {
        GameState = GameState.Playing;
        startTime = Time.time;

        GetComponent<ColorChanger>()?.ChangeColor();

        foreach (var board in boards)
        {
            board.Reset();
        }

        dPieceBlocksCount = 0;
        blocksInPlay = 0;
        SpawnPiece();
        spawn = StartCoroutine(Spawn());
    }

    private void OnEnable()
    {
        PieceView.PieceLavaEvent += pieceLavaEvent;
        PieceView.PieceDisappearEvent += pieceDisappearEvent;
        PieceView.PieceDestroyEvent += pieceDestroyEvent;
        PieceView.PieceSplitEvent += pieceSplitEvent;
        PieceView.PieceRemoveBlockEvent += pieceRemoveBlockEvent;
    }

    private void OnDisable()
    {
        PieceView.PieceLavaEvent -= pieceLavaEvent;
        PieceView.PieceDisappearEvent -= pieceDisappearEvent;
        PieceView.PieceDestroyEvent -= pieceDestroyEvent;
        PieceView.PieceSplitEvent -= pieceSplitEvent;
        PieceView.PieceRemoveBlockEvent -= pieceRemoveBlockEvent;
    }

    private void Awake()
    {
#if UNITY_EDITOR
        Application.targetFrameRate = 60;
#endif

        camera = Camera.main;

        pieceProvider = new BalancedRandomPieceProvider();

        pieceLavaEvent = (pieceView) => {
            RemovePiece(pieceView);
            pieceView.DestroyPiece();
            PoolManager.Instance.piecePool.Release(pieceView);
            CheckGameOver();
        };

        pieceDisappearEvent = (pieceView) => {
            RemovePiece(pieceView);
            PoolManager.Instance.piecePool.Release(pieceView);
        };

        pieceSplitEvent = (pieceView) => {
            AddPiece(pieceView);
        };

        pieceRemoveBlockEvent = (_) => {
            dPieceBlocksCount++;
            blocksInPlay--;
            if (blocksInPlay == 0)
            {
                SpawnPiece();
                dPieceBlocksCount = 0;
            }
            else if (dPieceBlocksCount % 4 == 0)
            {
                SpawnPiece();
            }
        };

    }

    private void Start()
    {
        GameState = GameState.InMenu;

        boards = new List<BoardBack>(FindObjectsOfType<BoardBack>());

        ScoreManager.Instance.Reset();

        leftFlipperRigid = leftFlipper.GetComponent<Rigidbody2D>();
        rightFlipperRigid = rightFlipper.GetComponent<Rigidbody2D>();

        lastShakeTime = Time.time;
    }

    private void FixedUpdate()
    {
        if (leftFlipperAction)
            FlipperHold(leftFlipperRigid, torqueForce);
        if (rightFlipperAction)
            FlipperHold(rightFlipperRigid, -torqueForce);
    }

    private void Update()
    {
        if (GameState == GameState.Playing)
        {
#if UNITY_STANDALONE || UNITY_WEBGL || UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TableShake();
            }
#elif UNITY_ANDROID || UNITY_IOS
            if (Input.touchCount > 2)
            {
                for (int i=0; i<Input.touchCount; i++)
                {
                    if (Input.GetTouch(i).phase == TouchPhase.Began)
                    {
                        TableShake();
                        break;
                    }
                }
            }
#endif
#if  UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.S))
            {
                SpawnPiece();
            }
#endif

            leftFlipperAction = false;
            rightFlipperAction = false;

            var screenPos = Input.mousePosition;
            var viewPos = camera.ScreenToViewportPoint(screenPos);

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                PlayFlipperSound();
                leftFlipperAction = true;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                leftFlipperAction = true;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                PlayFlipperSound();
                rightFlipperAction = true;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                rightFlipperAction = true;
            }

            if (Input.GetMouseButtonDown(0))
            {
                PlayFlipperSound();
                if (viewPos.x > 0.5f)
                {
                    rightFlipperAction = true;
                }
                else
                {
                    leftFlipperAction = true;
                }
            }
            else if (Input.GetMouseButton(0))
            {
                if (viewPos.x > 0.5f)
                {
                    rightFlipperAction = true;
                }
                else
                {
                    leftFlipperAction = true;
                }
            }
            if (Input.GetMouseButtonUp(1))
            {
                TableShake();
            }
        }
    }

    private void PlayFlipperSound()
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.flipping);
    }

    private void FlipperHold(Rigidbody2D flipper, float torqueForce)
    {
        AddTorque(flipper, torqueForce);
    }

    private void TableShake()
    {
        if (lastShakeTime + shakeTime >= Time.time)
        {
            shakeCount++;
        }
        else
        {
            shakeCount = 0;
        }

        if (shakeCount < maxShakeCount)
        {
            lastShakeTime = Time.time;
            foreach (var piece in piecesInPlay)
            {
                piece.GetComponent<Rigidbody2D>().AddForce(shakeForce);
            }
            shakeCount++;
        }
    }

    private void GameOver()
    {
        leftFlipperAction = rightFlipperAction = false;
        GameState = GameState.GameOver;
        StopCoroutine(spawn);
        foreach (var board in boards)
        {
            if (gameOverBlowUp)
            {
                board.Blowup();
            }
        }
        uIManager.Reset();
    }

    private void AddTorque(Rigidbody2D rigid, float force)
    {
        rigid.AddTorque(force);
    }

    private void SpawnPiece()
    {
        var piece = pieceProvider.GetPiece();
        var pieceV = Tetro.Instance.SpawnPiece(piece.Type, ballPoint.transform.position, Quaternion.Euler(0f, 0f, Random.Range(0, 360)), new Vector2(0f, 3f));
        AddPiece(pieceV);
    }

    private void CheckGameOver()
    {
        if (piecesInPlay.Count == 0)
        {
            if (startTime + gracePeriod > Time.time)
            {
                SpawnPiece();
            }
            else
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.gameOver);
                GameOver();
            }
        }
    }

    private void AddPiece(PieceView piece)
    {
        if (piecesInPlay.Contains(piece))
        {
            Debug.LogWarning("Trying to add a duplicate piece!");
        }

        blocksInPlay += piece.blockViews.Count;
        piecesInPlay.Add(piece);
    }

    private void RemovePiece(PieceView piece)
    {
        if (piecesInPlay.Contains(piece) == false)
        {
            Debug.LogWarning("Trying to remove a non-existent piece!");
        }

        blocksInPlay -= piece.blockViews.Count;
        piecesInPlay.Remove(piece);
    }


    private IEnumerator Spawn()
    {
        float t = 0;
        while (true)
        {
            if (t < timeToAddPiece)
            {
                t += Time.deltaTime;
            }
            else
            {
                t = t - timeToAddPiece;
                SpawnPiece();
            }

            yield return null;
        }
    }
}

public enum GameState
{
    InMenu,
    Playing,
    Paused,
    GameOver
}
