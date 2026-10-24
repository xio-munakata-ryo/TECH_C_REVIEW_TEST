// TargetSpawner.cs
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private float spawnInterval = 1.2f;
    [SerializeField] private Vector2 spawnAreaMin = new Vector2(-6f, -3f);
    [SerializeField] private Vector2 spawnAreaMax = new Vector2(6f, 3f);

    private CancellationTokenSource cts;

    private void Start()
    {
        cts = new CancellationTokenSource();
        SpawnLoopAsync(cts.Token).Forget();
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }

    private async UniTaskVoid SpawnLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(spawnInterval), cancellationToken: cancellationToken);

                Vector3 spawnPos = new Vector3(
                    UnityEngine.Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                    UnityEngine.Random.Range(spawnAreaMin.y, spawnAreaMax.y),
                    0f
                );

                Instantiate(targetPrefab, spawnPos, Quaternion.identity);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}



// // TargetSpawner.cs
// using System;
// using System.Threading;
// using Cysharp.Threading.Tasks;
// using UnityEngine;

// public class TargetSpawner : MonoBehaviour
// {
//     [SerializeField] private GameObject targetPrefab;
//     [SerializeField] private float spawnInterval = 1.2f;
//     [SerializeField] private Vector2 spawnAreaMin = new Vector2(-6f, -3f);
//     [SerializeField] private Vector2 spawnAreaMax = new Vector2(6f, 3f);

//     private CancellationTokenSource cts;

//     private void Start()
//     {
//         cts = new CancellationTokenSource();
//         SpawnLoopAsync(cts.Token).Forget();
//     }

//     private void OnDestroy()
//     {
//         cts?.Cancel();
//         cts?.Dispose();
//     }

//     private async UniTaskVoid SpawnLoopAsync(CancellationToken cancellationToken)
//     {
//         try
//         {
//             while (!cancellationToken.IsCancellationRequested)
//             {
//                 await UniTask.Delay(TimeSpan.FromSeconds(spawnInterval), cancellationToken: cancellationToken);

//                 Vector3 spawnPos = new Vector3(
//                     UnityEngine.Random.Range(spawnAreaMin.x, spawnAreaMax.x),
//                     UnityEngine.Random.Range(spawnAreaMin.y, spawnAreaMax.y),
//                     0f
//                 );

//                 Instantiate(targetPrefab, spawnPos, Quaternion.identity);
//             }
//         }
//         catch (OperationCanceledException)
//         {
//         }
//     }
// }