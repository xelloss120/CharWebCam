using UnityEngine;

public class RS_Kotonoha : RealSense
{
    public GameObject Head;
    public GameObject EyeL;
    public GameObject EyeR;
    
    // モーフ取得のため
    MMD4MecanimModel Model;

    void Start()
    {
        EyesPosX = 20;
        EyesPosY = 7;

        Init();

        // 初期表情
        Model = GetComponent<MMD4MecanimModel>();
        Model.GetMorph("じと目").weight = 0.7f;
        Model.GetMorph("眼球縮小").weight = 0.2f;
    }

    void Update()
    {
        // 値が揃ってから参照
        if (!Ready)
        {
            return;
        }

        // 体移動
        Body.transform.position = BodyPos;

        // 頭向き
        Head.transform.localEulerAngles = new Vector3(-HeadAng.z, -HeadAng.x, HeadAng.y);

        // 視線
        Vector3 eyesAng = new Vector3(EyesPos.y, EyesPos.x, 0);
        EyeL.GetComponent<MMD4MecanimBone>().userEulerAngles = eyesAng;
        EyeR.GetComponent<MMD4MecanimBone>().userEulerAngles = eyesAng;

        // 目パチ
        Model.GetMorph("まばたき").weight = EyesClose / 100;
        Model.GetMorph("じと目").weight = 0.7f - (EyesClose * 0.007f);

        // 眉上
        Model.GetMorph("上").weight = BrowRai / 100;

        // 眉下
        Model.GetMorph("下").weight = BrowLow / 100;
        Model.GetMorph("困る").weight = BrowLow / 100;
        Model.GetMorph("上まぶた閉").weight = BrowLow / 100;

        // 笑顔
        Model.GetMorph("まばたき3").weight = Smile / 100;
        Model.GetMorph("笑い").weight = Smile / 100;

        // キス
        Model.GetMorph("眼球縮小").weight = Kiss * 0.02f + 0.2f;

        // 表情競合対策
        if (Smile != 0 || BrowLow != 0)
        {
            Model.GetMorph("まばたき").weight = 0;

            float ret = Mathf.Max(Smile, BrowLow);
            Model.GetMorph("じと目").weight = 0.7f - (ret * 0.007f);
        }
        if(Smile > BrowLow / 2)
        {
            Model.GetMorph("下").weight = 0;
            Model.GetMorph("上まぶた閉").weight = 0;
        }
    }
}
