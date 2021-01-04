using UnityEngine;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
using UnityEngine.Rendering;


#if UNITY_POST_PROCESSING_STACK_V2
namespace Mewlist.MassiveClouds
{
    public class AbstractMassiveCloudsEffectRenderer<T> : PostProcessEffectRenderer<T>, IFullScreenDrawable
        where T : AbstractMassiveCloudsEffectSettings
    {
        public override DepthTextureMode GetCameraFlags()
        {
            return DepthTextureMode.Depth;
        }

        private DynamicRenderTexture screenTexture;
        private PostProcessRenderContext currentContext;

        private void OnPreRender()
        {
        }

        public override void Release()
        {
            var massiveCloudsRenderer = settings.rendererParameter.value;
            if (massiveCloudsRenderer)
                massiveCloudsRenderer.Clear();

            if (screenTexture != null)
                screenTexture.Dispose();
            screenTexture = null;

            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {
            var massiveCloudsRenderer = settings.rendererParameter.value;
            if (!massiveCloudsRenderer) return;

            var commandBuffer = context.command;
            if (commandBuffer == null) return;

            if (screenTexture == null)
                screenTexture = new DynamicRenderTexture(context.camera);
           
            currentContext = context;
            screenTexture.Update(context.camera, 1f);

            var sunLightSource = Object.FindObjectOfType<MassiveCloudsSunLightSource>();
            massiveCloudsRenderer.UpdateClouds(sunLightSource ? sunLightSource.Light : null, null);
            commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, screenTexture.GetRenderTexture(context.camera));
            commandBuffer.SetGlobalFloat("_MassiveCloudsProbeScale", 1f);
            commandBuffer.SetGlobalFloat("_SkyIntensity", 1.0f);
            massiveCloudsRenderer.BuildCommandBuffer(commandBuffer, context.camera, screenTexture.GetRenderTexture(context.camera), this);
            currentContext = null;
        }

        public void Draw(CommandBuffer commandBuffer, RenderTargetIdentifier source)
        {
            CommandBufferUtility.BlitProcedural(commandBuffer, source, currentContext.destination);
        }
    }
}
#endif