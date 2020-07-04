using VRM;

public class MM_VRM : MouthMove
{
    VRMBlendShapeProxy BlendShapeProxy;
    BlendShapeKey A = BlendShapeKey.CreateFromPreset(BlendShapePreset.A);

    void Start()
    {
        BlendShapeProxy = GetComponent<VRMBlendShapeProxy>();

        Init();
    }

    void Update()
    {
        float vol = GetVolume();
        BlendShapeProxy.ImmediatelySetValue(A, SmoothMouth.SmoothValue(vol < 1 ? 0 : vol / 20));
    }
}
