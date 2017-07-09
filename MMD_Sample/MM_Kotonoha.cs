public class MM_Kotonoha : MouthMove
{
    void Start()
    {
        Init();
    }

    void Update()
    {
        float vol = GetVolume();
        GetComponent<MMD4MecanimModel>().GetMorph("あ").weight = SmoothMouth.SmoothValue(vol < 1 ? 0 : vol / 20);
    }
}
