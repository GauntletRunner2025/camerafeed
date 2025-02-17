using Unity.Entities;
using UnityEngine;

public partial class CameraPreviewListener : Listener
{
    public override ComponentType EventType => ComponentType.ReadOnly<CameraShouldPreview>();

    public override ComponentType HandledFlagType => ComponentType.ReadOnly<CameraPreviewHandled>();

    public override bool OnEvent(EntityManager em, Entity e)
    {
        foreach (var (device, entity) in SystemAPI
            .Query<CameraDeviceComponent>()
            .WithAll<ActiveCamera>()
            .WithEntityAccess())
        {

            device.Value.previewResolution = new(1280, 720);

            Debug.Log($"Previewing active camera: {device.Value.name}");
            return true;
        }

        Debug.LogWarning("Preview requested but no active camera found");
        return false;
    }
}
