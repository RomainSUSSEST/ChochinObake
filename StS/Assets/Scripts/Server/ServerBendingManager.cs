using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class ServerBendingManager : ServerManager<ServerBendingManager>
{
    #region Constants

    private const string BENDING_FEATURE = "ENABLE_BENDING";

    private const string PLANET_FEATURE = "ENABLE_BENDING_PLANET";

    private static readonly int BENDING_AMOUNT =
      Shader.PropertyToID("_BendingAmount");

    #endregion

    #region Attributes

    [SerializeField]
    private bool enablePlanet = default;

    [SerializeField] [Range(0.0005f, 0.1f)]
        private float BendingAmount = 0.015f;

    private float _prevAmount;

    #endregion

    #region Manager Implementation

    protected override IEnumerator InitCoroutine()
    {
        yield break;
    }

    #endregion

    #region LifeCycle

    protected override void Awake()
    {
        if (Application.isPlaying)
        {
            base.Awake();
            Shader.EnableKeyword(BENDING_FEATURE);

            if (enablePlanet)
                Shader.EnableKeyword(PLANET_FEATURE);
            else
                Shader.DisableKeyword(PLANET_FEATURE);
        }
        else
            Shader.DisableKeyword(BENDING_FEATURE);

        UpdateBendingAmount();
    }

    private void Update()
    {
        if (Math.Abs(_prevAmount - BendingAmount) > Mathf.Epsilon)
            UpdateBendingAmount();
    }

    #endregion

    #region Event Subs

    public override void SubscribeEvents()
    {
        base.SubscribeEvents();

        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    public override void UnsubscribeEvents()
    {
        base.UnsubscribeEvents();

        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    #region Event's call back

    private static void OnBeginCameraRendering(ScriptableRenderContext ctx, Camera cam)
    {
        cam.cullingMatrix = Matrix4x4.Ortho(-99, 99, -99, 99, 0.001f, 99) *
                            cam.worldToCameraMatrix;
    }

    private static void OnEndCameraRendering(ScriptableRenderContext ctx, Camera cam)
    {
        cam.ResetCullingMatrix();
    }

    #endregion

    #endregion

    #region Tools

    private void UpdateBendingAmount()
    {
        _prevAmount = BendingAmount;
        Shader.SetGlobalFloat(BENDING_AMOUNT, BendingAmount);
    }

    #endregion
}
