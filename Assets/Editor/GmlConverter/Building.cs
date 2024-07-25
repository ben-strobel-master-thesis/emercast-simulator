using System.Collections.Generic;
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

    public void Draw(Scene scene)
    {
        foreach(var p in listPolys)
        {
            var go = p.Draw();
            SceneManager.MoveGameObjectToScene(go, scene);
        }
    }
    
    public void DrawTest(Scene scene)
    {
        foreach(var p in listPolys)
        {
            var go = p.DrawNew(GetFloorCenter());
            SceneManager.MoveGameObjectToScene(go, scene);
        }
    }
    
    public Vector3 GetFloorCenter()
    {
        double xSum = 0;
        double zSum = 0;
        int counter = 0;
        foreach (var poly in listPolys)
        {
            foreach (var point in poly.GetListPoints())
            {
                xSum += (double)point.x;
                zSum += (double)point.z;
                counter++;
            }
        }
        return new Vector3((float)(xSum / counter), 0, (float)(zSum / counter));
    }
    
    public void DrawNew(Scene scene)
    {
        GameObject go = new GameObject();
        go.name = "Building";
        go.transform.position = GetFloorCenter();
        var posX = go.transform.position.x;
        var posZ = go.transform.position.y;
        go.tag = "GeneratedBuildingObject";
        Mesh msh = new Mesh();

        List<Vector3> listPoints = new List<Vector3>();
        
        foreach (var p in listPolys)
        {
            listPoints.AddRange(p.GetListPoints());
        }

        Vector3[] vertices = new Vector3[listPoints.Count];
        for (int i = 0; i < listPoints.Count; i++)
            vertices[i] = listPoints[i]-new Vector3(posX, 0, posZ);


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
        mshFilter.mesh = msh;
        // mshFilter.mesh.Optimize();
        SceneManager.MoveGameObjectToScene(go, scene);
    }
}
