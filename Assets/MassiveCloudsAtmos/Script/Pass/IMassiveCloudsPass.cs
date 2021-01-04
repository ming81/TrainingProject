using UnityEngine;
using UnityEngine.Rendering;

namespace Mewlist.MassiveClouds
{
    public interface IMassiveCloudsPass<T>
    {
        void Update(T context);

        void BuildCommandBuffer(T context, Camera targetCamera, CommandBuffer commandBuffer, FlippingRenderTextures renderTextures);
        void Clear();
    }
}