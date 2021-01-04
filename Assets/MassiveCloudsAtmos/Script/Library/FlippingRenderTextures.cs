using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace Mewlist.MassiveClouds
{
    
    public class FlippingRenderTextures
    {
        private readonly int firstId;
        private readonly int secondId;
        public readonly int LowResolutionTmpId;
        public readonly int ScreenWidth;
        public readonly int ScreenHeight;

        private bool flipped;

        public readonly RenderTextureFormat FormatAlpha;

        public int From { get { return flipped ? firstId : secondId; } }
        public int To { get { return flipped ? secondId : firstId; } }

        public FlippingRenderTextures(
            Camera targetCamera,
            CommandBuffer commandBuffer,
            float resolution)
        {
            firstId = Shader.PropertyToID("_MassiveCloudsBufferFirst");
            secondId = Shader.PropertyToID("_MassiveCloudsBufferSecond");
            LowResolutionTmpId = Shader.PropertyToID("_MassiveCloudsBufferLow");
            
            flipped = false;

            ScreenWidth = targetCamera.pixelWidth;
            ScreenHeight = targetCamera.pixelHeight;

            FormatAlpha = targetCamera.allowHDR
                ? RenderTextureFormat.DefaultHDR
                : RenderTextureFormat.Default;

            CreateRenderTextures(targetCamera, commandBuffer, resolution);
        }

        public FlippingRenderTextures(
            Camera targetCamera,
            RenderTextureFormat formatAlpha,
            CommandBuffer commandBuffer,
            float resolution)
        {
            firstId = Shader.PropertyToID("_MassiveCloudsBufferFirst");
            secondId = Shader.PropertyToID("_MassiveCloudsBufferSecond");
            LowResolutionTmpId = Shader.PropertyToID("_MassiveCloudsBufferLow");

            flipped = false;

            ScreenWidth = targetCamera.pixelWidth;
            ScreenHeight = targetCamera.pixelHeight;

            FormatAlpha = formatAlpha;

            CreateRenderTextures(targetCamera, commandBuffer, resolution);
        } 

        private void CreateRenderTextures(
            Camera targetCamera,
            CommandBuffer commandBuffer,
            float resolution)
        {
            if (XRSettings.enabled)
            {
                var w = XRSettings.eyeTextureDesc.width;
                var h = XRSettings.eyeTextureDesc.height;
                commandBuffer.GetTemporaryRT(firstId, w, h, 0, FilterMode.Point, FormatAlpha);
                commandBuffer.GetTemporaryRT(secondId, w, h, 0, FilterMode.Point, FormatAlpha);
            }
            else
            {
                commandBuffer.GetTemporaryRT(firstId, targetCamera.pixelWidth, targetCamera.pixelHeight, 0,
                    FilterMode.Point, FormatAlpha);
                commandBuffer.GetTemporaryRT(secondId, targetCamera.pixelWidth, targetCamera.pixelHeight, 0,
                    FilterMode.Point, FormatAlpha);
            }

            commandBuffer.GetTemporaryRT(LowResolutionTmpId,
                Mathf.RoundToInt(targetCamera.pixelWidth * resolution),
                Mathf.RoundToInt(targetCamera.pixelHeight * resolution),
                0, FilterMode.Trilinear, FormatAlpha);
        }

        public RenderTexture CreateRenderTexture(RenderTextureDesc desc)
        {
            RenderTexture rt;
            if (XRSettings.enabled && XRSettings.eyeTextureDesc.vrUsage == VRTextureUsage.TwoEyes)
            {
                var rtDesc = new RenderTextureDescriptor(desc.Width * 2, desc.Height, FormatAlpha, 0)
                {
                    vrUsage = VRTextureUsage.TwoEyes,
                };
                rtDesc.autoGenerateMips = false;
                rt = new RenderTexture(rtDesc);
                rt.filterMode = FilterMode.Trilinear;
                rt.wrapMode = TextureWrapMode.Mirror;
            }
            else
            {
                var rtDesc = new RenderTextureDescriptor(desc.Width, desc.Height, FormatAlpha, 0)
                {
                    useMipMap = false,
                };
                rt = new RenderTexture(rtDesc);
                rt.filterMode = FilterMode.Trilinear;
                rt.wrapMode = TextureWrapMode.Mirror;
            }

            rt.name = "MassiveCloudsRT" + DateTime.Now.Millisecond;
            return rt;
        }

        public void Release(CommandBuffer commandBuffer)
        {
            commandBuffer.ReleaseTemporaryRT(firstId);
            commandBuffer.ReleaseTemporaryRT(secondId);
            commandBuffer.ReleaseTemporaryRT(LowResolutionTmpId);
        }

        public void Flip()
        {
            flipped = !flipped;
        }
    }
}