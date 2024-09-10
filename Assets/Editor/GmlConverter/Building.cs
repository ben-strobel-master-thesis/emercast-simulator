using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Building
{
    private List<Polygon> listPolys = new List<Polygon>();   

    public void AddPoly(double[] listDouble, int xOffset, int yOffset)
    {
        List<Vector3> tmp = new List<Vector3>();

        for (int i = 0; i < listDouble.Length; i = i + 3)
        {
            tmp.Add(new Vector3((float)(listDouble[i] - xOffset), (float)listDouble[i + 2], (float)(listDouble[i + 1] - yOffset)));
        }
        listPolys.Add(new Polygon(tmp));
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

    public GameObject CreatePrefabAndInstantiate(Material material, Scene scene, String path)
    {
        var center = GetFloorCenter();
        var listPoints = new List<Vector3>();

        foreach (var poly in listPolys)
        {
            listPoints.AddRange(poly.GetListPoints());
        }
        
        var go = new GameObject
        {
            name = "Building",
            tag = "GeneratedBuildingObject",
            isStatic = true
        };
        var msh = new Mesh();

        Vector3[] vertices = new Vector3[listPoints.Count];
        for (int i = 0; i < listPoints.Count; i++)
            vertices[i] = listPoints[i]-center;
        
        int[] triangles = new int[(int)Mathf.Ceil(listPoints.Count / 2) * 3 *2];

        var o = 0;
        var p = 0;
        for (int i = 0; i < listPolys.Count(); i++)
        {
            var points = listPolys[i].GetListPoints();
            
            for (var j = points.Count-1; j > 1; j--)
            {
                triangles[o] = p + 0;
                triangles[o+1] = p + j;
                triangles[o+2] = p + j-1;
                o += 3;
            }

            p += points.Count;
        }

        msh.vertices = vertices;
        msh.triangles = triangles;
        msh.RecalculateNormals();
        
        AssetDatabase.CreateAsset(msh, path+"Mesh.asset");
        
        var mshFilter = go.AddComponent<MeshFilter>();
        mshFilter.mesh = msh;

        var mshRenderer = go.AddComponent<MeshRenderer>();
        mshRenderer.material = material;
        
        PrefabUtility.SaveAsPrefabAssetAndConnect(go, path+".prefab", InteractionMode.AutomatedAction);
        SceneManager.MoveGameObjectToScene(go, scene);
        
        return go;
    }
    
}
