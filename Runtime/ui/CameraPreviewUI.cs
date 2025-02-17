using UnityEngine.UIElements;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
struct CameraActivated : IComponentData { }
struct NeedPreviewTextureHookupToUIElement : IComponentData { }
class CameraPreviewUIElement : IComponentData
{
    public VisualElement Value;
}

public partial class CameraPreviewUI : VisualElementUI
{
    protected override string Name => "camera-preview";

    protected override ElementSourceMode SourceMode => ElementSourceMode.ExistsInTree;

    protected override WaitModeEnum WaitMode => WaitModeEnum.DoNotWait;

    protected override void DoUpdate()
    {
        //We are ready so start polling for a camera we can auto-activate

        using EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        foreach (var (item, entity) in SystemAPI
            .Query<CameraDeviceComponent>()
            .WithAll<PreviewResolutionSet>()
            .WithNone<ActiveCamera>()
            .WithEntityAccess())
        {
            Debug.Log($"[{this.GetType().Name}] Found camera device {item.Value.name} and requested activation");

            //Set as actiove
            ecb.AddComponent<ActiveCamera>(entity);
            //Emit event
            var eventEntity = ecb.CreateEntity();
            ecb.AddComponent<CameraActivated>(eventEntity);
            ecb.AddComponent<Request>(eventEntity);
            this.Enabled = false;
            break;
        }

        ecb.Playback(EntityManager);
    }


    protected override void Initialize(VisualElement root, VisualElement element)
    {
        var e = EntityManager.CreateEntity();
        EntityManager.AddComponentData(e, new CameraPreviewUIElement { Value = element });
    }

}
