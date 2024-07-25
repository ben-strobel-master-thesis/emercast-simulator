using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Building
{
    private List<Polygon> listPolys = new List<Polygon>();   

    public void AddPoly(float[] listFloat)
    {
        List<Vector3> tmp = new List<Vector3>();

        if (listFloat.Length % 3 == 0)
        {
            for (int i = 0; i < listFloat.Length; i = i + 3)
            {
                tmp.Add(new Vector3(listFloat[i], listFloat[i + 2], listFloat[i+1])/100 );
            }
            listPolys.Add(new Polygon(tmp));
        }
    }
    
    public Vector3 GetFloorCenter()
    {
        double xSum = 0;
        double yMin = listPolys.Count == 0 ? 0 : double.MaxValue;
        double zSum = 0;
        int counter = 0;
        foreach (var poly in listPolys)
        {
            foreach (var point in poly.GetListPoints())
            {
                xSum += (double)point.x;
                yMin = Math.Min(yMin, point.y);
                zSum += (double)point.z;
                counter++;
            }
        }
        
        return new Vector3((float)(xSum / counter), (float)yMin, (float)(zSum / counter));
    }

    public GameObject CreateGameObject(Scene scene)
    {
        var center = GetFloorCenter();
        var listPoints = new List<Vector3>();

        foreach (var p in listPolys)
        {
            listPoints.AddRange(p.GetListPoints());
        }
        
        var go = new GameObject
        {
            name = "Building",
            tag = "GeneratedBuildingObject",
            transform =
            {
                position = center
            }
        };
        var msh = new Mesh();

        Vector3[] vertices = new Vector3[listPoints.Count];
        for (int i = 0; i < listPoints.Count; i++)
            vertices[i] = listPoints[i]-center;


        int[] triangles = new int[ (int)Mathf.Ceil(listPoints.Count / 2) * 3 *2];

        int lastIndex = 0;
        for(int i = 0; i < listPoints.Count-1; i = i +2)
        {
            if( i == listPoints.Count - 1)
            {
                triangles[lastIndex] = i;
                lastIndex++;
                triangles[lastIndex]  = 0;
                lastIndex++;
                triangles[lastIndex] = 1;
                lastIndex++;
            }
            else if( i == listPoints.Count -2)
            {
                triangles[lastIndex] = i;
                lastIndex++;
                triangles[lastIndex] = i+1;
                lastIndex++;
                triangles[lastIndex] = 0;
                lastIndex++;
            }
            else
            {
                // Draws each triangle clockwise and anticlockwise
                triangles[lastIndex] = i;
                lastIndex++;
                triangles[lastIndex] = i + 1;
                lastIndex++;
                triangles[lastIndex] = i + 2;
                lastIndex++;

                triangles[lastIndex] = i;
                lastIndex++;
                triangles[lastIndex] = i + 2;
                lastIndex++;
                triangles[lastIndex] = i + 1;
                lastIndex++;

            }
        }

        msh.vertices = vertices;
        msh.triangles = triangles;

        MeshFilter mshFilter = go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>().material.color = Color.white;
        mshFilter.sharedMesh = msh;
        mshFilter.sharedMesh.Optimize();
        SceneManager.MoveGameObjectToScene(go, scene);
        return go;
    }
    
}
