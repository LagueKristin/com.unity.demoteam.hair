using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class GreyOutScene : EditorWindow
{
    private bool isGreyedOut = false;
    private Camera sceneViewCamera;
    private Material greyscaleMaterial;

    [MenuItem("Window/Grey Out Scene")]
    public static void ShowWindow()
    {
        GetWindow<GreyOutScene>("Grey Out Scene");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Grey Out"))
        {
            GreyOut();
        }
        if (GUILayout.Button("Restore"))
        {
            Restore();
        }
    }

    private void GreyOut()
    {
        isGreyedOut = true;

        // Get the current Scene view camera
        sceneViewCamera = SceneView.lastActiveSceneView.camera;

        // Create a new material to apply the greyscale post-processing effect
        greyscaleMaterial = new Material(Shader.Find("Custom/GreyOutScene"));

        // Attach the post-processing effect material to the Scene view camera
        sceneViewCamera.AddCommandBuffer(CameraEvent.AfterImageEffects, CreateCommandBuffer());

        // Force the Scene view to repaint
        SceneView.RepaintAll();
    }

    private void Restore()
    {
        isGreyedOut = false;

        // Remove the post-processing effect material from the Scene view camera
        sceneViewCamera.RemoveCommandBuffer(CameraEvent.AfterImageEffects, CreateCommandBuffer());

        // Release the resources used by the greyscale material
        DestroyImmediate(greyscaleMaterial);

        // Force the Scene view to repaint
        SceneView.RepaintAll();
    }

    private CommandBuffer CreateCommandBuffer()
    {
        // Create a new command buffer to apply the greyscale post-processing effect
        CommandBuffer commandBuffer = new CommandBuffer();
        commandBuffer.Blit(null, BuiltinRenderTextureType.CameraTarget, greyscaleMaterial);

        return commandBuffer;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        // If the Scene view is currently in greyscale mode, update the Scene view camera's settings
        if (isGreyedOut)
        {
            sceneView.camera.clearFlags = CameraClearFlags.SolidColor;
            sceneView.camera.backgroundColor = Color.gray;
            sceneView.camera.allowHDR = false;
            sceneView.camera.allowMSAA = false;
            sceneView.camera.depthTextureMode = DepthTextureMode.None;
        }
    }
}