// TargetObject.cs
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TargetObject : MonoBehaviour
{
    [SerializeField] private float lifeTime = 2.0f;
    private CancellationTokenSource cts;
    private bool isClicked = false;

    private void Start()
    {
        cts = new CancellationTokenSource();
        LifeTimerAsync(cts.Token).Forget();
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }

    public void OnHit()
    {
        if (isClicked) return;
        isClicked = true;

        GameController controller = UnityEngine.Object.FindAnyObjectByType<GameController>();
        if (controller != null)
        {
            controller.AddScore(1);
        }

        Destroy(gameObject);
    }

    private async UniTaskVoid LifeTimerAsync(CancellationToken cancellationToken)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(lifeTime), cancellationToken: cancellationToken);
            if (!isClicked && gameObject != null)
            {
                Destroy(gameObject);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}