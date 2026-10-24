// GameController.cs
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button startButton;

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 30f;
    [SerializeField] private int targetScore = 10;

    private int score = 0;
    private float timeRemaining;
    private bool isPlaying = false;
    private CancellationTokenSource cts;

    private void Awake()
    {
        cts = new CancellationTokenSource();
    }

    private void Start()
    {
        scoreText.text = "Score: 0";
        timerText.text = $"Time: {gameDuration:F1}";
        messageText.text = "Press Start to Play";
        
        startButton.onClick.AddListener(() => StartGameAsync(cts.Token).Forget());
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }

    public void AddScore(int amount)
    {
        if (!isPlaying) return;
        score += amount;
        scoreText.text = $"Score: {score}";
    }

    private async UniTaskVoid StartGameAsync(CancellationToken cancellationToken)
    {
        startButton.gameObject.SetActive(false);
        score = 0;
        timeRemaining = gameDuration;
        scoreText.text = "Score: 0";
        messageText.text = "GO!";
        isPlaying = true;

        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: cancellationToken);
        messageText.text = "";

        try
        {
            while (timeRemaining > 0 && isPlaying)
            {
                timeRemaining -= Time.deltaTime;
                if (timeRemaining < 0) timeRemaining = 0;

                timerText.text = $"Time: {timeRemaining:F1}";

                if (score >= targetScore)
                {
                    break;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }

        isPlaying = false;

        if (score >= targetScore)
        {
            messageText.text = "CLEAR! Click to Restart";
        }
        else
        {
            messageText.text = "GAME OVER. Click to Restart";
        }

        await UniTask.Delay(TimeSpan.FromSeconds(1.5f), cancellationToken: cancellationToken);

        startButton.gameObject.SetActive(true);
        startButton.GetComponentInChildren<TextMeshProUGUI>().text = "Restart";
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(() => ReloadScene());
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

// // GameController.cs
// using System;
// using System.Threading;
// using Cysharp.Threading.Tasks;
// using TMPro;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;

// public class GameController : MonoBehaviour
// {
//     [Header("UI References")]
//     [SerializeField] private TextMeshProUGUI scoreText;
//     [SerializeField] private TextMeshProUGUI timerText;
//     [SerializeField] private TextMeshProUGUI messageText;
//     [SerializeField] private Button startButton;

//     [Header("Game Settings")]
//     [SerializeField] private float gameDuration = 30f;
//     [SerializeField] private int targetScore = 10;

//     private int score = 0;
//     private float timeRemaining;
//     private bool isPlaying = false;
//     private CancellationTokenSource cts;

//     private void Awake()
//     {
//         cts = new CancellationTokenSource();
//     }

//     private void Start()
//     {
//         scoreText.text = "Score: 0";
//         timerText.text = $"Time: {gameDuration:F1}";
//         messageText.text = "Press Start to Play";
        
//         startButton.onClick.AddListener(() => StartGameAsync(cts.Token).Forget());
//     }

//     private void OnDestroy()
//     {
//         cts?.Cancel();
//         cts?.Dispose();
//     }

//     public void AddScore(int amount)
//     {
//         if (!isPlaying) return;
//         score += amount;
//         scoreText.text = $"Score: {score}";
//     }

//     private async UniTaskVoid StartGameAsync(CancellationToken cancellationToken)
//     {
//         startButton.gameObject.SetActive(false);
//         score = 0;
//         timeRemaining = gameDuration;
//         scoreText.text = "Score: 0";
//         messageText.text = "GO!";
//         isPlaying = true;

//         await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: cancellationToken);
//         messageText.text = "";

//         try
//         {
//             while (timeRemaining > 0 && isPlaying)
//             {
//                 timeRemaining -= Time.deltaTime;
//                 if (timeRemaining < 0) timeRemaining = 0;

//                 timerText.text = $"Time: {timeRemaining:F1}";

//                 if (score >= targetScore)
//                 {
//                     break;
//                 }

//                 await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
//             }
//         }
//         catch (OperationCanceledException)
//         {
//             return;
//         }

//         isPlaying = false;

//         if (score >= targetScore)
//         {
//             messageText.text = "CLEAR! Click to Restart";
//         }
//         else
//         {
//             messageText.text = "GAME OVER. Click to Restart";
//         }

//         await UniTask.Delay(TimeSpan.FromSeconds(1.5f), cancellationToken: cancellationToken);

//         startButton.gameObject.SetActive(true);
//         startButton.GetComponentInChildren<TextMeshProUGUI>().text = "Restart";
//         startButton.onClick.RemoveAllListeners();
//         startButton.onClick.AddListener(() => ReloadScene());
//     }

//     private void ReloadScene()
//     {
//         SceneManager.LoadScene(SceneManager.GetActiveScene().name);
//     }
// }