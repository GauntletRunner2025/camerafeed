using Unity.Entities;
using UnityEngine;
using VideoKit;

struct PreviewResolutionSet : IComponentData { }
public partial class CameraPreviewResolution : SystemBase
{
    private int width = 1920;
    private int height = 1080;

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