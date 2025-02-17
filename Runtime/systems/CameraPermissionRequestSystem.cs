using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using System.Threading.Tasks;
using VideoKit;
using System;
using static VideoKit.MediaDevice;

public partial class CameraPermissionRequestSystem : TaskSystem
{
    protected override bool AutoCreateTask => true;

    private class Flag : IComponentData
    {
        public Task<PermissionStatus> Value;
    }

    protected override ComponentType FlagType => typeof(Flag);

    protected override ComponentType[] RequireForUpdate => new ComponentType[] { };

    protected override void OnSystemUpdate()
    {
        // No additional update logic needed
    }

    protected override void OnTaskComplete(EntityManager em, Entity entity, Task result)
    {
        Debug.Log($"Camera permission task completed for entity {entity.Index}");
        if (result is Task<PermissionStatus> permissionTask &&
            permissionTask.Result == PermissionStatus.Authorized)
        {
            Debug.Log("Camera permission granted");
            em.AddComponent<CameraPermissionGranted>(entity);
        }
        else
        {
            Debug.LogWarning("Camera permission denied or task failed");
            em.AddComponent<CameraPermissionDenied>(entity);
        }
    }

    protected override void OnTaskFailed(EntityManager em, Entity entity, AggregateException exception)
    {
        Debug.LogError($"Failed to get camera permission: {exception.Message}");
        em.AddComponent<CameraPermissionDenied>(entity);
    }

    protected override Task Setup(EntityManager em, Entity entity)
    {
        Debug.Log("Setting up camera permission request task...");
        var taskComponent = new Flag
        {
            Value = CameraDevice.CheckPermissions(request: true)
        };

        em.AddComponentData(entity, taskComponent);
        return taskComponent.Value;
    }
}
