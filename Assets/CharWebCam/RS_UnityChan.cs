using UnityEngine;

public class RS_UnityChan : RealSense
{
    public GameObject Head;
    public MeshRenderer EyeL;
    public MeshRenderer EyeR;
    public SkinnedMeshRenderer EYE_DEF;
    public SkinnedMeshRenderer EL_DEF;

    void Start()
    {
        Init();
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
        Head.transform.localEulerAngles = new Vector3(HeadAng.x, HeadAng.y, HeadAng.z + 10);

        // 視線
        EyeL.material.SetTextureOffset("_MainTex", EyesPos);
        EyeR.material.SetTextureOffset("_MainTex", EyesPos);

        // 目パチ
        EYE_DEF.SetBlendShapeWeight(6, EyesClose);
        EL_DEF.SetBlendShapeWeight(6, EyesClose);

        // 笑顔
        EYE_DEF.SetBlendShapeWeight(0, Smile);
        EL_DEF.SetBlendShapeWeight(0, Smile);

        // 表情優先の目パチ無効
        if (Smile != 0)
        {
            EYE_DEF.SetBlendShapeWeight(6, 0);
            EL_DEF.SetBlendShapeWeight(6, 0);
        }
    }
}
