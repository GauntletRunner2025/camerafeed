using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

public partial class HookUpPreviewTextureToUIElement : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<CameraPreviewTexture>();
        RequireForUpdate<CameraPreviewUIElement>();
    }

    protected override void OnStartRunning()
    {
        var cameraPreviewTexture = SystemAPI.ManagedAPI.GetSingleton<CameraPreviewTexture>();
        var cameraPreviewUIElement = SystemAPI.ManagedAPI.GetSingleton<CameraPreviewUIElement>();

        if (cameraPreviewTexture == null || cameraPreviewUIElement == null)
        {
            Debug.LogError($"[{this.GetType().Name}] Missing singleton");
            return;
        }

        cameraPreviewUIElement.Value.style.backgroundImage = Background.FromTexture2D(cameraPreviewTexture.Value);
        return;
    }

    protected override void OnUpdate()
    {

    }
}