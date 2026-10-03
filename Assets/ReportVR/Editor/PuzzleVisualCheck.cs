using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class PuzzleVisualCheck
{
    public static void Capture()
    {
        int index=0;
        foreach(var scene in EditorBuildSettings.scenes.Where(s=>s.enabled))
        {
            EditorSceneManager.OpenScene(scene.path);var level=Object.FindObjectOfType<DemoLevel>();
            var font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},48);var material=new Material(Object.FindObjectOfType<DemoInteractor>().worldText);
            var texts=Object.FindObjectsOfType<TextMesh>(true);foreach(var text in texts){text.font=font;text.GetComponent<Renderer>().sharedMaterial=material;}
            level.Refresh();font.RequestCharactersInTexture(string.Concat(texts.Select(t=>t.text)),48);material.mainTexture=font.material.mainTexture;
            foreach(var station in level.stations)
                ReportLevelAudit.Photograph("Documentation/PuzzlePreviews/"+index+"_"+station.index+".png",station.viewingPoint+Vector3.up*1.65f-station.transform.forward*.6f,station.transform.position+Vector3.up*1.45f,false,15);
            Object.DestroyImmediate(material);Object.DestroyImmediate(font);index++;
        }
        Debug.Log("PUZZLE_PREVIEWS_OK scenes="+index);
    }
}
