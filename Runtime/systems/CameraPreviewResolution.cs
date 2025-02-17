using Unity.Entities;
using UnityEngine;
using VideoKit;

public partial class CameraPreviewResolution : SystemBase
{
    private struct PreviewResolutionSet : IComponentData { }
    private int width = 1920;
    private int height = 1080;

    public void SetResolution(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    protected override void OnUpdate()
    {
        using EntityCommandBuffer ecb = new(Unity.Collections.Allocator.TempJob);

        foreach (var (device, entity) in SystemAPI
            .Query<CameraDeviceComponent>()
            .WithNone<PreviewResolutionSet>()
            .WithEntityAccess())
        {
            device.Value.previewResolution = (width, height);
            Debug.Log($"Set {device.Value.name} preview resolution to {width}x{height}");
            ecb.AddComponent<PreviewResolutionSet>(entity);
        }

        ecb.Playback(EntityManager);
    }
}
