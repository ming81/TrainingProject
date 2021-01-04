using UnityEngine;
using UnityEngine.Rendering;

namespace Mewlist.MassiveClouds
{
    [ExecuteInEditMode]
    public class MassiveCloudsCameraEffect : MonoBehaviour, IFullScreenDrawable
    {
        [SerializeField] private AbstractMassiveClouds massiveClouds = null;
        [SerializeField] private CameraEvent cameraEvent = CameraEvent.AfterSkybox;
        [SerializeField] private Light sun = null;
        [SerializeField] private Transform moon = null;

        private CommandBuffer        commandBuffer;
        private CameraEvent          currentCameraEvent = CameraEvent.AfterSkybox;

        private Camera TargetCamera { get { return GetComponent<Camera>(); } }
        private AbstractMassiveClouds MassiveClouds { get { return massiveClouds; } }

        public void Draw(CommandBuffer commandBuffer, RenderTargetIdentifier source)
        {
            CommandBufferUtility.BlitProcedural(commandBuffer, source, BuiltinRenderTextureType.CameraTarget);
        }

        private void Start()
        {
            if (!Application.isPlaying)
                DynamicGI.UpdateEnvironment();
        }

        private void SetupCamera()
        {
            currentCameraEvent = cameraEvent;
            TargetCamera.forceIntoRenderTexture = true;
            if ((TargetCamera.depthTextureMode & DepthTextureMode.Depth) == 0)
            {
                TargetCamera.depthTextureMode |= DepthTextureMode.Depth;
            }
        }

        private void Create()
        {
            Clear();
            SetupCamera();

            if (commandBuffer == null)
                commandBuffer = new CommandBuffer {name = "MassiveClouds"};
            TargetCamera.AddCommandBuffer(currentCameraEvent, commandBuffer);
        }

        private void OnPreRender()
        {
            if (commandBuffer == null) return;
            commandBuffer.Clear();
            if (!MassiveClouds) return;
           
            MassiveClouds.UpdateClouds(sun, moon);
            commandBuffer.SetGlobalFloat("_MassiveCloudsProbeScale", 1f);
            commandBuffer.SetGlobalFloat("_SkyIntensity", 1.0f);
            MassiveClouds.BuildCommandBuffer(commandBuffer, TargetCamera, BuiltinRenderTextureType.CameraTarget, this);
        }

        private void Clear()
        {
            if (commandBuffer != null)
            {
                TargetCamera.RemoveCommandBuffer(currentCameraEvent, commandBuffer);
            }
            
            if (MassiveClouds) MassiveClouds.Clear();

            commandBuffer = null;
        }

        private void Update()
        {
            if (!MassiveClouds)
            {
                Clear();
                return;
            }
//            DynamicGI.UpdateEnvironment();

            if (commandBuffer == null) Create();
        }

        private void OnDisable()
        {
            Clear();
        }
    }
}