using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using VRM;

public class RS_VRM : RealSense
{
    enum CustomBlendShape
    {
        Surprise,
    }

    readonly float DistanceFromCamera = 0.75f;
    readonly float IgnoreBlinkThreshold = 10;
    readonly IDictionary<object, BlendShapeKey> BlendShapeKeys = new object[] {
        BlendShapePreset.Neutral,
        BlendShapePreset.Blink,
        CustomBlendShape.Surprise,
        BlendShapePreset.Sorrow,
        BlendShapePreset.Joy,
        BlendShapePreset.Angry,
    }.ToDictionary(name => name, name => name is BlendShapePreset preset
        ? BlendShapeKey.CreateFromPreset(preset)
        : BlendShapeKey.CreateUnknown(name.ToString()));

    Transform Head;
    Transform TargetLookedAt;
    VRMBlendShapeProxy BlendShapeProxy;

    void Start()
    {
        MoveFaceInFrontOfCamera();
        Pose();
        SetupLipSync();

        EyesPosX = 20;
        EyesPosY = 7;
        BodyPosYOffset = transform.position.y;

        var lookAtHead = GetComponent<VRMLookAtHead>();
        Head = lookAtHead.Head;
        TargetLookedAt = new GameObject().transform;
        TargetLookedAt.parent = Head;
        TargetLookedAt.localPosition = new Vector3(0, 0, 1);
        BlendShapeProxy = GetComponent<VRMBlendShapeProxy>();

        Init();
    }

    /// <summary>
    /// 顔を原点のカメラ前へ
    /// </summary>
    void MoveFaceInFrontOfCamera()
    {
        var firstPerson = GetComponent<VRMFirstPerson>();
        var firstPersonPosition = firstPerson.FirstPersonBone.position + firstPerson.FirstPersonOffset;
        transform.position = -firstPersonPosition + new Vector3(0, 0, DistanceFromCamera);
        transform.localRotation = Quaternion.Euler(0, 180, 0);
    }

    /// <summary>
    /// アバターのポーズ
    /// </summary>
    void Pose()
    {
        var animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("WAIT");
        animator.applyRootMotion = true;
        animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
        animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
    }

    /// <summary>
    /// リップシンク
    /// </summary>
    void SetupLipSync()
    {
        gameObject.AddComponent<AudioSource>();
        var mm = gameObject.AddComponent<MM_VRM>();
        mm.Text = Canvas.Text;
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
        transform.position = BodyPos;

        // 頭向き
        Head.localEulerAngles = new Vector3(-HeadAng.z, -HeadAng.x, HeadAng.y);

        // 視線
        TargetLookedAt.localPosition = new Vector3((EyesPos * 0.5f).x, (EyesPos * 0.5f).y, TargetLookedAt.localPosition.z);

        // 初期表情
        BlendShapeProxy.AccumulateValue(BlendShapeKeys[BlendShapePreset.Neutral], 1);

        // 目パチ
        BlendShapeProxy.AccumulateValue(
            BlendShapeKeys[BlendShapePreset.Blink],
            // 表情競合対策
            new[] { BrowRai, BrowLow, Smile, Kiss }.Any(param => param > IgnoreBlinkThreshold) ? 0 : EyesClose / 100
        );

        // 眉上
        BlendShapeProxy.AccumulateValue(BlendShapeKeys[CustomBlendShape.Surprise], BrowRai / 100);

        // 眉下
        BlendShapeProxy.AccumulateValue(BlendShapeKeys[BlendShapePreset.Sorrow], BrowLow / 100);

        // 笑顔
        BlendShapeProxy.AccumulateValue(BlendShapeKeys[BlendShapePreset.Joy], Smile / 100);

        // キス
        BlendShapeProxy.AccumulateValue(BlendShapeKeys[BlendShapePreset.Angry], Kiss / 100);
    }

    void LateUpdate()
    {
        // MM_VRM 側でのリップシンク設定を待ってから適用する
        BlendShapeProxy.Apply();
    }
}
