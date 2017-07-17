using UnityEngine;

public class RS_UnityChan : RealSense
{
    public GameObject Body;
    public GameObject Head;
    public MeshRenderer EyeL;
    public MeshRenderer EyeR;
    public SkinnedMeshRenderer BLW_DEF;
    public SkinnedMeshRenderer EYE_DEF;
    public SkinnedMeshRenderer EL_DEF;
    public SkinnedMeshRenderer MTH_DEF;

    void Start()
    {
        BodyPosYOffset = Body.transform.position.y;

        Init();
    }

    void Update()
    {
        // 値が揃ってから参照
        if (!Ready)
        {
            return;
        }

        // 各パラメータ表示
        UpdateParamText();

        // 体移動
        Body.transform.position = BodyPos;

        // 頭向き
        Head.transform.localEulerAngles = new Vector3(HeadAng.x, HeadAng.y, HeadAng.z + 10);

        // 視線
        EyeL.material.SetTextureOffset("_MainTex", EyesPos);
        EyeR.material.SetTextureOffset("_MainTex", EyesPos);

        // 目パチ
        EYE_DEF.SetBlendShapeWeight(6, EyesClose);
        EL_DEF.SetBlendShapeWeight(6, EyesClose);

        // 眉上
        BLW_DEF.SetBlendShapeWeight(2, BrowRai);
        EYE_DEF.SetBlendShapeWeight(2, BrowRai);
        EL_DEF.SetBlendShapeWeight(2, BrowRai);

        // 眉下
        BLW_DEF.SetBlendShapeWeight(3, BrowLow);
        EYE_DEF.SetBlendShapeWeight(3, BrowLow);
        EL_DEF.SetBlendShapeWeight(3, BrowLow);
        MTH_DEF.SetBlendShapeWeight(3, BrowLow);

        // 笑顔
        BLW_DEF.SetBlendShapeWeight(0, Smile);
        EYE_DEF.SetBlendShapeWeight(0, Smile);
        EL_DEF.SetBlendShapeWeight(0, Smile);
        MTH_DEF.SetBlendShapeWeight(1, Smile);

        // キス
        BLW_DEF.SetBlendShapeWeight(4, Kiss);
        EYE_DEF.SetBlendShapeWeight(4, Kiss);
        EL_DEF.SetBlendShapeWeight(4, Kiss);
        MTH_DEF.SetBlendShapeWeight(4, Kiss);

        // 表情競合対策
        if (Smile > 10)
        {
            BLW_DEF.SetBlendShapeWeight(3, 0);
            EYE_DEF.SetBlendShapeWeight(6, 0);
            EL_DEF.SetBlendShapeWeight(6, 0);
        }
        if (Kiss > 10)
        {
            BLW_DEF.SetBlendShapeWeight(3, 0);
            EYE_DEF.SetBlendShapeWeight(3, 0);
            EL_DEF.SetBlendShapeWeight(3, 0);
            MTH_DEF.SetBlendShapeWeight(3, 0);
        }
    }
}
