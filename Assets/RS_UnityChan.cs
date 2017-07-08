using UnityEngine;
using Intel.RealSense;
using Intel.RealSense.Face;

public class RS_UnityChan : RealSense
{
    // オブジェクト
    public GameObject Body;
    public GameObject Head;
    public MeshRenderer EyeL;
    public MeshRenderer EyeR;
    public SkinnedMeshRenderer EYE_DEF;
    public SkinnedMeshRenderer EL_DEF;

    // 初期表示位置(オフセット)
    float BodyY;

    void Start()
    {
        // RealSense初期化
        Init();

        // 初期表示位置(オフセット)の保持
        BodyY = Body.transform.position.y;
    }

    void Update()
    {
        // 値が揃ってから参照
        if (Landmark == null && FaceExp == null)
        {
            return;
        }

        // 体移動
        Body.transform.position = SmoothBody.SmoothValue(GetBodyPos(FaceRect));

        // 頭向き
        Head.transform.localEulerAngles = SmoothHead.SmoothValue(GetHeadAng(Landmark));

        // 視線
        Vector2 eyesPos = SmoothEyes.SmoothValue(GetEyesPos(Landmark));
        EyeL.material.SetTextureOffset("_MainTex", eyesPos);
        EyeR.material.SetTextureOffset("_MainTex", eyesPos);

        // 目パチ
        float eyeL = FaceExp[FaceExpression.EXPRESSION_EYES_CLOSED_LEFT].intensity;
        float eyeR = FaceExp[FaceExpression.EXPRESSION_EYES_CLOSED_RIGHT].intensity;
        float close = SmoothEyesClose.SmoothValue(Mathf.Max(eyeL, eyeR));
        EYE_DEF.SetBlendShapeWeight(6, close);
        EL_DEF.SetBlendShapeWeight(6, close);

        // 笑顔
        float smile = FaceExp[FaceExpression.EXPRESSION_SMILE].intensity;
        EYE_DEF.SetBlendShapeWeight(0, smile);
        EL_DEF.SetBlendShapeWeight(0, smile);

        // 表情優先の目パチ無効
        if (smile != 0)
        {
            EYE_DEF.SetBlendShapeWeight(6, 0);
            EL_DEF.SetBlendShapeWeight(6, 0);
        }
    }

    /// <summary>
    /// 体位置を取得
    /// </summary>
    /// <param name="rect">顔の矩形</param>
    /// <returns>体位置</returns>
    /// <remarks>
    /// https://software.intel.com/sites/landingpage/realsense/camera-sdk/v2016r3/documentation/html/index.html?doc_face_face_location_data.html
    /// </remarks>
    Vector3 GetBodyPos(RectI32 rect)
    {
        // 体位置に利用するため頭位置を取得
        float xMax = Resolution.width;
        float yMax = Resolution.height;
        float xPos = rect.x + (rect.w / 2);
        float yPos = rect.h + (rect.h / 2);
        float zPos = (yMax - rect.h);

        // 末尾の除算で調整
        xPos = (xPos - (xMax / 2)) / (xMax / 2) / 3;
        yPos = (yPos - (yMax / 2)) / (yMax / 2) / 3;
        zPos = zPos / 500;

        // 初期位置のオフセットを適用
        yPos += BodyY;

        // 顔の大きさと中心から初期位置分ずらして体位置に利用
        return new Vector3(-xPos, yPos, zPos);
    }

    /// <summary>
    /// 頭角度を取得
    /// </summary>
    /// <param name="points">顔の検出点</param>
    /// <returns>頭角度</returns>
    /// <remarks>
    /// https://software.intel.com/sites/landingpage/realsense/camera-sdk/v2016r3/documentation/html/index.html?doc_face_face_landmark_data.html
    /// </remarks>
    Vector3 GetHeadAng(LandmarkPoint[] points)
    {
        // 頭向きに利用するため顔の中心と左右端、唇下、顎下を取得
        Vector2 center = points[29].image;
        Vector2 left = points[68].image;
        Vector2 right = points[54].image;
        Vector2 mouth = points[42].image;
        Vector2 chin = points[61].image;

        // 末尾で調整(0.2は顔幅に対する唇下から顎までの比 / 300はその値に対する倍率 / 10.416はUnityちゃん初期値)
        float xAng = (Vector2.Distance(left, center) - Vector2.Distance(right, center)) / Vector2.Distance(left, right) * 70;
        float yAng = GetAngle(mouth, chin) - 90;
        float zAng = (Vector2.Distance(mouth, chin) / Vector2.Distance(left, right) - 0.2f) * 300 + 10.416f;

        // 唇下と顎下の点から角度計算して頭向きに利用
        return new Vector3(xAng, -yAng, zAng);
    }

    /// <summary>
    /// 瞳位置を取得
    /// </summary>
    /// <param name="points">顔の検出点</param>
    /// <returns>瞳位置</returns>
    /// <remarks>
    /// https://software.intel.com/sites/landingpage/realsense/camera-sdk/v2016r3/documentation/html/index.html?doc_face_face_landmark_data.html
    /// </remarks>
    Vector2 GetEyesPos(LandmarkPoint[] points)
    {
        // 左右の目の瞳と上下左右端を取得
        Vector2 lEye = points[77].image;
        Vector2 lLeft = points[22].image;
        Vector2 lRight = points[18].image;
        Vector2 lTop = points[20].image;
        Vector2 lBottom = points[24].image;
        Vector2 rEye = points[76].image;
        Vector2 rLeft = points[10].image;
        Vector2 rRight = points[14].image;
        Vector2 rTop = points[12].image;
        Vector2 rBottom = points[16].image;

        // 末尾で調整
        float tmp1, tmp2;
        tmp1 = GetCenterRatio(lRight, lEye, lLeft) * 0.8f;
        tmp2 = GetCenterRatio(rRight, rEye, rLeft) * 0.8f;
        float xPos = (tmp1 + tmp2) / 2;
        tmp1 = GetCenterRatio(lTop, lEye, lBottom) * 0.2f;
        tmp2 = GetCenterRatio(rTop, rEye, rBottom) * 0.2f;
        float yPos = (tmp1 + tmp2) / 2;

        // 唇下と顎下の点から角度計算して頭向きに利用
        return new Vector2(xPos, yPos);
    }
}
