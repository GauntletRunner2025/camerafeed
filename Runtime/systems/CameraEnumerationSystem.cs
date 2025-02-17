using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using System.Threading.Tasks;
using System;
using VideoKit;
using System.Linq;

public partial class CameraEnumerationSystem : TaskSystem
{
    protected override bool AutoCreateTask => true;

    private class Flag : IComponentData
    {
        public Task<VideoKit.CameraDevice[]> Value;
    }

    protected override ComponentType FlagType => typeof(Flag);

    protected override ComponentType[] RequireForUpdate => new ComponentType[]
    {
        typeof(CameraPermissionGranted)
    };

    protected override Task Setup(EntityManager em, Entity entity)
    {
        Debug.Log("Starting camera enumeration...");
        var taskComponent = new Flag
        {
            Value = VideoKit.CameraDevice.Discover()
        };

        em.AddComponentData(entity, taskComponent);
        return taskComponent.Value;
    }

    protected override void OnTaskComplete(EntityManager em, Entity entity, Task result)
    {
        if (result is Task<VideoKit.CameraDevice[]> task)
        {
            var devices = task.Result;
            Debug.Log($"Camera enumeration complete. Found {devices?.Length ?? 0} devices");

            if (devices == null)
            {
                Debug.LogError("No camera devices found");
                return;
            }

            using EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

            foreach (VideoKit.CameraDevice device in devices)
            {
                if (device == null)
                {
                    Debug.LogWarning("Found null camera device");
                    continue;
                }

                var cameraEntity = ecb.CreateEntity();
                ecb.AddComponent(cameraEntity, new CameraDeviceComponent { Value = device });
                if (device.frontFacing)
                {
                    ecb.AddComponent<FrontFacingCamera>(cameraEntity);
                    Debug.Log($"Created front-facing camera entity for {device.name}");
                }
                else
                {
                    ecb.AddComponent<RearFacingCamera>(cameraEntity);
                    Debug.Log($"Created rear-facing camera entity for {device.name}");
                }
            }

            ecb.Playback(EntityManager);
        }
        else
        {
            Debug.LogError("Task result was not of expected type Task<CameraDevice[]>");
        }
    }

    protected override void OnTaskFailed(EntityManager em, Entity entity, AggregateException exception)
    {
        Debug.LogError($"Failed to enumerate cameras: {exception.Message}");
    }

    protected override void OnSystemUpdate() { }
}
