using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[RequireComponent(typeof(MeshRenderer))]
public class GameOfLife : MonoBehaviour
{
    //display material
    Material _material;

    //Simulation buffers
    [HideInInspector]
    public RenderTexture buffer_A;
    [HideInInspector]
    public RenderTexture buffer_B;

    //Simulation settings
    public GameOfLifeSettings settings;

    //compute shaders
    public ComputeShader clearCompute;
    public ComputeShader lifeCompute;
    public ComputeShader drawCompute;
    public ComputeShader copyCompute;

    //compute kernels
    private int clearKernel;
    private int lifeKernel;
    private int drawKernel;
    private int copyKernel;

    //pre-calculated property IDs
    private int resultPropertyName = Shader.PropertyToID("Result");
    private int inputPropertyName = Shader.PropertyToID("Input");
    private int pointAPropertyName = Shader.PropertyToID("PointA");
    private int pointBPropertyName = Shader.PropertyToID("PointB");
    private int colorPropertyName = Shader.PropertyToID("IColor");
    private int mainTexturePropertyName = Shader.PropertyToID("MainTex");

    //Simulation variables
    private float _simulationTimer;
    private int _previousResolution;

    /// <summary>
    /// Init
    /// </summary>
    void Start()
    {
        _material = GetComponent<MeshRenderer>().material;
        buffer_A = CreateBufferTexture(settings);
        buffer_B = CreateBufferTexture(settings);
        _material.SetTexture(mainTexturePropertyName,buffer_A);

        try
        {
            clearKernel = clearCompute.FindKernel("CSMain");
            lifeKernel = lifeCompute.FindKernel("CSMain");
            drawKernel = drawCompute.FindKernel("CSMain");
            copyKernel = copyCompute.FindKernel("CSMain");
        }
        catch
        {
            Debug.LogError("Missing a compute shader! Please fill all references!");
        }
        ClearBuffer(buffer_A);
    }

    /// <summary>
    /// Simulate Game Of Life every frame, as many times needed, and check updated settings
    /// </summary>
    void Update()
    {
        if (_previousResolution != settings.resolution)
        {
            ResizeCanvas();
            _previousResolution = settings.resolution;
        }
        if (settings.running) {
            while (_simulationTimer > 1)
            {
                Simulate();
                _simulationTimer -= 1;
            }
            _simulationTimer += Time.deltaTime * settings.simulationSpeed;
        }
        
    }

    /// <summary>
    /// Resize the canvas according to the settings
    /// </summary>
    private void ResizeCanvas() {
        buffer_A.Release();
        buffer_B.Release();
        buffer_A.width = settings.resolution;
        buffer_A.height = settings.resolution;
        buffer_B.width = settings.resolution;
        buffer_B.height = settings.resolution;
        buffer_A.Create();
        buffer_B.Create();
    }
    /// <summary>
    /// Simulate a generation of Game Of Life
    /// </summary>
    public void Simulate()
    {
        lifeCompute.SetTexture(lifeKernel,inputPropertyName,buffer_A);
        lifeCompute.SetTexture(lifeKernel,resultPropertyName,buffer_B);
        int threadGroups = GetThreadGroupSize();
        lifeCompute.Dispatch(lifeKernel,threadGroups,threadGroups,1);
        SwapBuffers();
    }
    /// <summary>
    /// Switch out buffer A and buffer B as render targets
    /// </summary>
    private void SwapBuffers()
    {
        RenderTexture buff1 = buffer_A;
        buffer_A = buffer_B;
        buffer_B = buff1;
        _material.SetTexture(mainTexturePropertyName, buffer_A);
    }
    /// <summary>
    /// Draw a point in Game Of Life
    /// </summary>
    /// <param name="point">Coordinate on the canvas</param>
    /// <param name="color">Color to draw (0 = dead cell; 1 = live cell)</param>
    public void DrawPoint(int2 point, int color)
    {
        drawCompute.SetVector(pointAPropertyName,(Vector2)(float2)point);
        drawCompute.SetVector(pointBPropertyName,(Vector2)(float2)point);
        drawCompute.SetInt(colorPropertyName, color);
        drawCompute.SetTexture(drawKernel,inputPropertyName,buffer_A);
        drawCompute.SetTexture(drawKernel, resultPropertyName, buffer_B);
        int threadGroups = GetThreadGroupSize();
        drawCompute.Dispatch(drawKernel,threadGroups,threadGroups,1);
        SwapBuffers();
    }
    /// <summary>
    /// Fill an area in Game of Life
    /// </summary>
    /// <param name="pointA">First coordinate on the canvas</param>
    /// <param name="pointB">Second coordinate on the canvas</param>
    /// <param name="color">Color to draw (0 = dead cell; 1 = live cell)</param>
    public void DrawArea(int2 pointA, int2 pointB, int color)
    {
        if (pointA.x > pointB.x)
        {
            int x = pointA.x;
            pointA.x = pointB.x;
            pointB.x = x;
        }
        if (pointA.y > pointB.y)
        {
            int y = pointA.y;
            pointA.y = pointB.y;
            pointB.y = y;
        }

        drawCompute.SetVector(pointAPropertyName, (Vector2)(float2)pointA);
        drawCompute.SetVector(pointBPropertyName, (Vector2)(float2)pointB);
        drawCompute.SetInt(colorPropertyName,color);
        drawCompute.SetTexture(drawKernel, inputPropertyName, buffer_A);
        drawCompute.SetTexture(drawKernel, resultPropertyName, buffer_B);
        int threadGroups = GetThreadGroupSize();
        drawCompute.Dispatch(drawKernel, threadGroups, threadGroups, 1);
        SwapBuffers();
    }

    /// <summary>
    /// Utility for clearing a buffer texture
    /// </summary>
    /// <param name="buffer"></param>
    private void ClearBuffer(RenderTexture buffer)
    {
        int threadGroupSize = GetThreadGroupSize();
        clearCompute.SetTexture(clearKernel,resultPropertyName,buffer);
        clearCompute.Dispatch(clearKernel, threadGroupSize, threadGroupSize, 1);
    }


    /// <summary>
    /// Utility to get the needed thread group size for the given resolution
    /// </summary>
    /// <returns></returns>
    private int GetThreadGroupSize()
    {
        return Mathf.CeilToInt(settings.resolution / 8.0f);
    }

    /// <summary>
    /// Create a new buffer texture based on resolution
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    private static RenderTexture CreateBufferTexture(GameOfLifeSettings settings)
    {
        RenderTexture rt = new RenderTexture(settings.resolution,settings.resolution,1,RenderTextureFormat.R8);
        rt.enableRandomWrite = true;
        rt.filterMode = FilterMode.Point;
        return rt;
    }
    /// <summary>
    /// Release buffers when the application quits
    /// </summary>
    private void OnApplicationQuit()
    {
        buffer_A.Release();
        buffer_B.Release();
    }
}

/// <summary>
/// Stored settings of Game Of Life
/// </summary>
[System.Serializable]
public struct GameOfLifeSettings
{
    public int resolution;
    public float simulationSpeed;
    public bool running;
}
