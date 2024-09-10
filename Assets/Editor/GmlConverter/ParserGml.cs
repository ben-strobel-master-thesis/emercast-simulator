using System.Collections.Generic;
using UnityEngine;
using System;

public class ParserGml
{
    static double[] StringArrayToDoubleArray(string[] stringArray)
    {
        // Skipping last point, since it just repeats first point
        var res = new double[stringArray.Length-3];
        
        for (var i = 0; i < res.Length; i++)
        {
            res[i] = double.Parse(stringArray[i].Replace('.', ','));
        }

        return res;
    }


    public static List<Building> LoadGml(string[] lines, int xOffset, int yOffset)
    {
        List<Building> city = new List<Building>();
        Building buildingtmp = new Building();

        foreach (string l in lines )
        {
            // When reaching end of building tag, Add the current building to the city
            if (l.Contains("</bldg:Building"))
            {                
                city.Add(buildingtmp);
                buildingtmp = new Building();
            }

            if ( l.Contains("<gml:posList"))
            {
                var tmp = l.Substring(l.IndexOf(">")+1);
                tmp = tmp.Substring(0, tmp.Length - "</gml:posList>".Length);
                var valueString = tmp.Split(' ');
                var doubleValues = StringArrayToDoubleArray(valueString);
                buildingtmp.AddPoly(doubleValues, xOffset, yOffset);               
            }
        }
        return city;
    }
}

