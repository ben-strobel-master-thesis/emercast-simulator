using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class EmercastGenerateBuildings : UnityEditor.Editor
    {
        [MenuItem(("Emercast/Generate Buildings"))]
        public static void GenerateBuildings()
        {
            // Would be better to stream but works with big file, so good enough
            var buildingsCSV = Resources.Load<TextAsset>("exported_buildings_50").text;
            var lines = buildingsCSV.Split("\r\n");
            Debug.Log(lines.Length);
            foreach (var line in lines)
            {
                var segments = line.Split("|");
                if(segments.Length != 4) continue;
                var id = segments[0];
                var height = segments[1];
                var stories = segments[2];
                var shape = segments[3];
                Debug.Log(id);
            }
        }
    }
}
