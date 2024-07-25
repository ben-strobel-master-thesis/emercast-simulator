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

            List<GameObject> CityParts = new List<GameObject>();
            
            CityParts.Add(GenerateCityPart(Resources.Load<TextAsset>("gml-data/688_5334"), targetScene));
            CityParts.Add(GenerateCityPart(Resources.Load<TextAsset>("gml-data/688_5336"), targetScene));
            CityParts.Add(GenerateCityPart(Resources.Load<TextAsset>("gml-data/688_5338"), targetScene));
            CityParts.Add(GenerateCityPart(Resources.Load<TextAsset>("gml-data/690_5334"), targetScene));
            CityParts.Add(GenerateCityPart(Resources.Load<TextAsset>("gml-data/690_5336"), targetScene));
            CityParts.Add(GenerateCityPart(Resources.Load<TextAsset>("gml-data/690_5338"), targetScene));

            var parent = new GameObject { name = "City", tag = "GeneratedBuildingObject" };
            SceneManager.MoveGameObjectToScene(parent, targetScene.EditingScene);
            var center = Vector3.zero;
            foreach (var part in CityParts)
            {
                center += part.transform.position;
            }

            center /= CityParts.Count;
            parent.transform.position = center;

            foreach (var part in CityParts)
            {
                part.transform.parent = parent.transform;
            }
        }
        
        private static GameObject GenerateCityPart(TextAsset gmlResource, SubScene targetScene)
        {
            var gmlFile = gmlResource.text;
            var lines = gmlFile.Split("\r\n");
            Debug.Log("Lines in GML file: " + lines.Length);
            var city = ParserGml.LoadGml(lines);
            Debug.Log("Parsed " + city.Count + " buildings");
        
            var center = Vector3.zero;
            var parent = new GameObject { name = "CityPart " + gmlResource.name, tag = "GeneratedBuildingObject" };
            SceneManager.MoveGameObjectToScene(parent, targetScene.EditingScene);

            List<GameObject> gos = new List<GameObject>();
            
            var counter = 0;
            foreach (var b in city)
            {
                var go = b.CreateGameObject(targetScene.EditingScene);
                center += go.transform.position;
                gos.Add(go);
                counter++;
            }
        
            center /= counter;
            parent.transform.position = center;

            foreach (var go in gos)
            {
                go.transform.parent = parent.transform;
            }
                    
            EditorSceneManager.MarkSceneDirty(targetScene.EditingScene);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            return parent;
        }
    }
}
