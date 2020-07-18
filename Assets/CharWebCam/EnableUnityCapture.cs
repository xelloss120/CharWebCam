using UnityEngine;

public class EnableUnityCapture : MonoBehaviour
{
    void Start()
    {
        if (!CommandLineArgs.VirtualCamera)
        {
            return;
        }

        // 無効なUnityCaptureコンポーネントは、終了時にUnityCapture.OnDestory()でNullReferenceExceptionを発生させる
        var capture = gameObject.AddComponent<UnityCapture>();
        capture.ResizeMode = UnityCapture.EResizeMode.LinearResize;
    }
}
