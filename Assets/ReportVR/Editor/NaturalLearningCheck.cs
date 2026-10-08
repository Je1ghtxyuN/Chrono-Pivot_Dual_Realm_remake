using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class NaturalLearningCheck
{
    public static void Validate()
    {
        foreach(var scene in EditorBuildSettings.scenes.Where(s=>s.enabled))
        {
            EditorSceneManager.OpenScene(scene.path);
            var level=Object.FindObjectOfType<DemoLevel>();
            foreach(var station in level.stations)
                if(station.puzzle.mode!=ReportPuzzleMode.Physical || station.puzzle.learningTask==null || station.puzzle.choices.Length>0)
                    throw new Exception("Learning objective still uses a multiple-choice button: "+scene.path+" "+station.instruction);
            if(Object.FindObjectsOfType<Transform>(true).Any(t=>t.name.Contains("Trajectory") && t.gameObject.activeInHierarchy))
                throw new Exception("Static unexplained trajectory remains: "+scene.path);
            foreach(var task in Object.FindObjectsOfType<ReportLearningTask>())
                if(task.requirements.Length==0 && task.GetComponent<ReportPuzzle>().placements.Length==0)
                    throw new Exception("Empty experiment auto-completes: "+scene.path);
        }
        if(!AppDomain.CurrentDomain.GetAssemblies().SelectMany(a=>a.GetTypes()).Any(t=>t.Name=="ReportThrowTrack"))
            throw new Exception("No actual physics throw tracking");
        Debug.Log("NATURAL_LEARNING_SCENES_OK");
    }
    public static void First()
    {
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
        var level=Object.FindObjectOfType<DemoLevel>();
        foreach(var station in level.stations)
            if(station.puzzle.mode!=ReportPuzzleMode.Physical || station.puzzle.learningTask==null || station.puzzle.choices.Length>0)
                throw new Exception("First level still uses answers rather than experiments");
        level.Use(level.stations[0]);if(level.Step!=0)throw new Exception("Status click bypasses material placement");
        var pickup=Object.FindObjectOfType<ReportPickupMeasure>();
        if(pickup==null || pickup.samples.Length!=3 || pickup.samples.Distinct().Count()!=3)throw new Exception("No distinct material pickup goal");
        if(level.stations[0].puzzle.placements.Length!=0)throw new Exception("Pickup still requires placement");
        if(pickup.samples.Select(s=>s.GetComponent<Rigidbody>().mass).Distinct().Count()!=3)throw new Exception("Material experiment has identical sample masses");
        foreach(var sample in pickup.samples)
            if(Mathf.Abs(sample.GetComponent<Renderer>().bounds.size.x-.14f)>.005f)throw new Exception("Material does not fit player hand: "+sample.name);
        if(Object.FindObjectOfType<ReportMechanicalLauncher>()!=null)throw new Exception("Calibrated launcher still misleads free throwing");
        var target=Object.FindObjectOfType<ReportThrowTarget>();
        if(target==null || target.acceptedItems.Length!=3 || target.requiredStation!=level.stations[1] || target.launchArea!=null)throw new Exception("Free throw target invalid");
        if(Object.FindObjectOfType<ReportSunlightMeasure>()==null)throw new Exception("No direct sun/shadow interaction");
        var arc=Object.FindObjectsOfType<Transform>(true).First(t=>t.name=="StaticIntroThrowTrajectory");
        if(arc.gameObject.activeSelf)throw new Exception("Unexplained static parabola remains");
        Debug.Log("NATURAL_FIRST_SCENE_OK");
    }
}
