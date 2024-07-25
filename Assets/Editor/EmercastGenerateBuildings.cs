using System.Collections.Generic;
using Unity.Scenes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor
{
    public class EmercastGenerateBuildings : UnityEditor.Editor
    {
        [MenuItem(("Emercast/Generate Buildings"))]
        public static void GenerateBuildings()
        {
            var targetSceneObject = GameObject.FindGameObjectWithTag("GeneratorTargetScene");
            var targetScene = targetSceneObject.GetComponent<SubScene>();
            
            var oldBuildingObjects = GameObject.FindGameObjectsWithTag("GeneratedBuildingObject");
            foreach (var obo in oldBuildingObjects)
            {
                DestroyImmediate(obo);
            }
            
            // Would be better to stream but works with big file, so good enough
            var gmlResource = Resources.Load<TextAsset>("gml-data/688_5334");
            var gmlFile = gmlResource.text;
            var lines = gmlFile.Split("\r\n");
            Debug.Log("Lines in GML file: " + lines.Length);
            var city = ParserGml.LoadGml(lines);
            Debug.Log("Parsed " + city.Count + " buildings");
            var i = 0;
            city[0].DrawTest(targetScene.EditingScene);
            // city[1].Draw(targetScene.EditingScene);
            /*foreach (var b in city)
            {
                b.Draw(targetScene.EditingScene);
                Debug.Log("Created Building #" + i);
                i++;
            }*/
            
            EditorSceneManager.MarkSceneDirty(targetScene.EditingScene);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}
