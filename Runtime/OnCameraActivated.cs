using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using VideoKit;

public partial class OnCameraActivated : Listener
{
    struct Handled : IComponentData { }
    public override ComponentType EventType => typeof(CameraActivated);
    public override ComponentType HandledFlagType => typeof(Handled);

    // Create preview data
    NativeArray<byte> rgbaData;
    // Create the preview texture
    Texture2D texture;
    // Start streaming pixel buffers from the camera
    // cameraDevice.StartRunning(OnPixelBuffer);

    public override bool OnEvent(EntityManager em, Entity e)
    {
        //Find the ActiveCamera singleton
        Entity activeCameraEntity = SystemAPI.GetSingletonEntity<ActiveCamera>();
        //Get the CameraDevice
        CameraDeviceComponent cameraDevice = em.GetComponentData<CameraDeviceComponent>(activeCameraEntity);

        int width = cameraDevice.Value.previewResolution.width;
        int height = cameraDevice.Value.previewResolution.height;

        //Transformed pixcels
        rgbaData = new NativeArray<byte>(width * height * 4, Allocator.Persistent);
        texture = new Texture2D(width, height, TextureFormat.RGBA32, false);


        //Create a singleton containing the texture
        Entity textureEntity = em.CreateEntity();
        em.AddComponentData(textureEntity, new CameraPreviewTexture { Value = texture });

        cameraDevice.Value.StartRunning(OnNewFrame);

        Debug.Log("Camera activated!");
        return true;
    }

    private void OnNewFrame(PixelBuffer cameraBuffer)
    {
        Debug.Log("New frame received!");
        lock (texture)
        {
            // Create a destination `PixelBuffer` backed by our preview data
            using var previewBuffer = new PixelBuffer(
                cameraBuffer.width,
                cameraBuffer.height,
                PixelBuffer.Format.RGBA8888,
                rgbaData
            );
            // Copy the pixel data from the camera buffer to our preview buffer
            cameraBuffer.CopyTo(previewBuffer, rotation: PixelBuffer.Rotation._180);
        }
    }

    public override void SystemUpdate(EntityManager em)
    {

        if (null == texture)
        {
            return;
        }

        // Update the preview texture with the latest preview data
        lock (texture)
            texture.GetRawTextureData<byte>().CopyFrom(rgbaData);

        // Upload the texture data to the GPU for display
        texture.Apply();

    }
}

internal class CameraPreviewTexture : IComponentData
{
    public Texture2D Value;
}