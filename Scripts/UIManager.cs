using System.Collections;

using UnityEngine;
using UnityEngine.UI;

using SgLib;


public class UIManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Text inGameScore;
    [SerializeField] private GameObject menuScoreGO;
    [SerializeField] private GameObject buttonsGO;
    [Header("Layout Menu")]
    [SerializeField] private Text menuScore;
    [SerializeField] private Text menuBestScore;
    [SerializeField] private Button muteBtn;
    [SerializeField] private Button unmuteBtn;

    private Animator scoreAnimator;


    public void HandlePlayButton()
    {
        HideMenu();
        ScoreManager.Instance.Reset();
        inGameScore.gameObject.SetActive(true);
        UpdateInGameScoreDisplay();
        gameManager.StartGame();
    }

    public void ToggleSound()
    {
        SoundManager.Instance.ToggleMute();
        UpdateMuteButtons();
    }

    public void Reset()
    {
        inGameScore.gameObject.SetActive(false);
        Invoke("ShowMenu", 1f);
    }

    public void ShowMenu()
    {
        buttonsGO.SetActive(true);
        menuScoreGO.SetActive(true);
        UpdateMuteButtons();
        UpdateMenuScoreDisplay();
    }

    public void HideMenu()
    {
        buttonsGO.SetActive(false);
        menuScoreGO.SetActive(false);
    }

    private void OnEnable()
    {
        ScoreManager.ScoreUpdated += OnScoreUpdated;
    }

    private void OnDisable()
    {
        ScoreManager.ScoreUpdated -= OnScoreUpdated;
    }

    private void Start()
    {
        scoreAnimator = inGameScore.GetComponent<Animator>();
        inGameScore.gameObject.SetActive(false);
        inGameScore.text = ScoreManager.Instance.Score.ToString();
        ShowMenu();
    }

    private void OnScoreUpdated(int newScore)
    {
        UpdateInGameScoreDisplay();
        scoreAnimator.Play("NewScore");
    }

    private void UpdateInGameScoreDisplay()
    {
        inGameScore.text = ScoreManager.Instance.Score.ToString();
    }


    private void UpdateMenuScoreDisplay()
    {
        menuScore.text = ScoreManager.Instance.Score.ToString();
        menuBestScore.text = ScoreManager.Instance.HighScore.ToString();
    }

    private void UpdateMuteButtons()
    {
        if (SoundManager.Instance.IsMuted())
        {
            unmuteBtn.gameObject.SetActive(false);
            muteBtn.gameObject.SetActive(true);
        }
        else
        {
            unmuteBtn.gameObject.SetActive(true);
            muteBtn.gameObject.SetActive(false);
        }
    }
}
