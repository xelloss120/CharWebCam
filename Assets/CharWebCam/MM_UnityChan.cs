using UnityEngine;

public class MM_UnityChan : MouthMove
{
    public SkinnedMeshRenderer Mouth;

    void Start()
    {
        Init();
    }

    void Update()
    {
        float vol = GetVolume();
        Mouth.SetBlendShapeWeight(6, SmoothMouth.SmoothValue(vol < 1 ? 0 : vol * 5));
    }
}
