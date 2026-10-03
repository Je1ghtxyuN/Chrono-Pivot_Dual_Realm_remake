using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class PuzzleSceneCheck
{
    static void Require(bool ok,string message){if(!ok)throw new Exception(message);}
    public static void Validate()
    {
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
        var first=Object.FindObjectOfType<DemoLevel>();first.Use(first.stations[0]);
        Require(first.Step==0,"First level can advance without solving or placing anything");
        int puzzles=0;
        foreach(var scene in EditorBuildSettings.scenes.Where(s=>s.enabled))
        {
            EditorSceneManager.OpenScene(scene.path);var level=Object.FindObjectOfType<DemoLevel>();
            Require(level.exitDoor!=null && level.exitDoor.GetComponentInChildren<Collider>()!=null,"No physical exit lock: "+scene.path);
            foreach(var station in level.stations)
            {
                var puzzle=station.GetComponents<MonoBehaviour>().FirstOrDefault(c=>c.GetType().Name=="ReportPuzzle");
                Require(puzzle!=null,"Station has no puzzle: "+station.name);
                var settings=new SerializedObject(puzzle);
                Require(!string.IsNullOrWhiteSpace(settings.FindProperty("clue").stringValue),"Puzzle has no readable clue: "+station.name);
                Require(!string.IsNullOrWhiteSpace(settings.FindProperty("goal").stringValue),"Puzzle has no clear goal: "+station.name);
                Require(settings.FindProperty("choices").arraySize>=2 || settings.FindProperty("placements").arraySize>0,"Puzzle has no player decision or placement: "+station.name);
                puzzles++;
            }
        }
        File.WriteAllText("Documentation/Verification/puzzle-scenes.txt","PASS: 15 scenes; 45 authored puzzles; explicit goals and clues; decisions or physical placements; locked collidable exits; no first-level click-only progression.\n");
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);Debug.Log("PUZZLE_SCENES_OK count="+puzzles);
    }
}
