using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
[RequireComponent(typeof(Camera))]
public class GameOfLifeEditor : MonoBehaviour
{
    public GameOfLife gameOfLife;
    public GameObject selection;
    private Material _selectionMaterial;
    private int _boundsNameID = Shader.PropertyToID("Bounds");
    Vector2 startPos;
    Vector2 endPos;

    Camera _camera;

    void Start()
    {
        _camera = GetComponent<Camera>();
        _selectionMaterial = selection.GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        ProcessInput();
    }
    private void ProcessInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            BeginSelection();
        }
        if (Input.GetMouseButton(0))
        {
            UpdateSelection();
        }
        if (Input.GetMouseButtonUp(0))
        {
            EndSelection();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            gameOfLife.settings.running = !gameOfLife.settings.running;
        }
    }

    private Vector2 GetInGameMousePos()
    {
        Vector3 wpos = _camera.ScreenToWorldPoint(Input.mousePosition);
        wpos.x = Mathf.Clamp(wpos.x, -0.5f, 0.5f);
        wpos.y = Mathf.Clamp(wpos.y, -0.5f, 0.5f);
        return new Vector2(wpos.x,wpos.y);
    }

    private void BeginSelection()
    {
        startPos = GetInGameMousePos();
        selection.SetActive(true);
        UpdateMaterial();
    }
    private void UpdateSelection()
    {
        endPos = GetInGameMousePos();
        UpdateMaterial();
    }
    private void EndSelection()
    {
        endPos = GetInGameMousePos();
        selection.SetActive(false);
        UpdateMaterial();
        int live = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 0 : 1;
        gameOfLife.DrawArea(new int2((int)((startPos.x + 0.5f) * gameOfLife.settings.resolution), (int)((startPos.y + 0.5f) * gameOfLife.settings.resolution))
            , new int2((int)((endPos.x + 0.5f) * gameOfLife.settings.resolution), (int)((endPos.y + 0.5f) * gameOfLife.settings.resolution)),live);
    }

    private Vector2 FloorRelativeToGame(Vector2 pos)
    {
        Vector2 unitPos = pos * gameOfLife.settings.resolution;
        unitPos = new Vector2(Mathf.FloorToInt(unitPos.x),Mathf.FloorToInt(unitPos.y));
        unitPos /= (float)gameOfLife.settings.resolution;
        return unitPos;
    }

    private Vector2 CeilRelativeToGame(Vector2 pos)
    {
        Vector2 unitPos = pos * gameOfLife.settings.resolution;
        unitPos = new Vector2(Mathf.CeilToInt(unitPos.x), Mathf.CeilToInt(unitPos.y));
        unitPos /= (float)gameOfLife.settings.resolution;
        return unitPos;
    }

    private void UpdateMaterial()
    {
        Vector2 startPosOrdered = new Vector2(Mathf.Min(startPos.x,endPos.x), Mathf.Min(startPos.y, endPos.y));
        Vector2 endPosOrdered = new Vector2(Mathf.Max(startPos.x, endPos.x), Mathf.Max(startPos.y, endPos.y));
        Vector3 startSrcPos = _camera.WorldToScreenPoint(FloorRelativeToGame(startPosOrdered));
        Vector3 endSrcPos = _camera.WorldToScreenPoint(CeilRelativeToGame(endPosOrdered));
        Vector4 bounds = new Vector4(startSrcPos.x, startSrcPos.y, endSrcPos.x, endSrcPos.y);
        _selectionMaterial.SetVector(_boundsNameID, bounds);
    }

}
