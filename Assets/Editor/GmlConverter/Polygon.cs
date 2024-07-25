using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygon 
{  
    List<Vector3> listPoints;

    public Polygon(List<Vector3> listPoints)
    {
        this.listPoints = listPoints;
    }

    public List<Vector3> GetListPoints()
    {
        return listPoints;
    }
}
