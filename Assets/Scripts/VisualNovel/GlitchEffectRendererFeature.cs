using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class GlitchRendererFeature : ScriptableRendererFeature
{
	[System.Serializable]
	public class GlitchSettings
	{
		public Material glitchMaterial;
		[Range(0f, 1f)] public float intensity = 0.5f;
	}

	public GlitchSettings settings = new GlitchSettings();

	class GlitchPass : ScriptableRenderPass
	{
		private Material glitchMaterial;
		private RTHandle tempTexture;
		private float intensity;

		public GlitchPass(Material material, float intensity)
		{
			this.glitchMaterial = material;
			this.intensity = intensity;
			renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			var descriptor = renderingData.cameraData.cameraTargetDescriptor;
			descriptor.depthBufferBits = 0;
			RenderingUtils.ReAllocateIfNeeded(ref tempTexture, descriptor, name: "_TempGlitchTex");
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			if (glitchMaterial == null)
				return;

			CommandBuffer cmd = CommandBufferPool.Get("Glitch Effect");

			// Pass intensity to shader if it has a property for it
			if (glitchMaterial.HasProperty("_Intensity"))
				glitchMaterial.SetFloat("_Intensity", intensity);

			// Get current camera target
			var cameraTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

			// Blit with glitch shader
			Blitter.BlitCameraTexture(cmd, cameraTarget, tempTexture, glitchMaterial, 0);
			Blitter.BlitCameraTexture(cmd, tempTexture, cameraTarget);

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}


		//public override void RecordRenderGraph(RenderGraph renderGraph, FrameResources frameResources, ref RenderingData renderingData)
		//{
		//	if (glitchMaterial == null)
		//		return;

		//	// Input texture (camera color)
		//	var input = renderGraph.ImportTexture(renderingData.cameraData.renderer.cameraColorTargetHandle);

		//	// Create a temporary render texture for output
		//	var output = renderGraph.CreateTexture(new TextureDesc(renderingData.cameraData.cameraTargetDescriptor)
		//	{
		//		colorFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm,
		//		name = "_TempGlitchTex"
		//	});

		//	// Add a render pass
		//	renderGraph.AddRenderPass("Glitch Effect", out var passData, (RenderGraphContext ctx) =>
		//	{
		//		var cmd = ctx.cmd;

		//		if (glitchMaterial.HasProperty("_Intensity"))
		//			glitchMaterial.SetFloat("_Intensity", intensity);

		//		// Blit with glitch material
		//		cmd.Blit(input, output, glitchMaterial);

		//		// Blit back to camera color target
		//		cmd.Blit(output, input);
		//	});
		//}

		public override void OnCameraCleanup(CommandBuffer cmd)
		{
			// no manual release needed; RTHandles are managed
		}
	}

	GlitchPass glitchPass;

	public override void Create()
	{
		if (settings.glitchMaterial != null)
			glitchPass = new GlitchPass(settings.glitchMaterial, settings.intensity);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (settings.glitchMaterial != null)
			renderer.EnqueuePass(glitchPass);
	}
}
