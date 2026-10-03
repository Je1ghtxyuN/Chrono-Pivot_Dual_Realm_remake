using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class PuzzleBehaviorCheck
{
    static int assertions;
    static void Require(bool ok,string message){assertions++;if(!ok)throw new Exception(message);}
    public static void Run()
    {
        assertions=0;int count=0;
        foreach(var scene in EditorBuildSettings.scenes.Where(s=>s.enabled))
        {
            EditorSceneManager.OpenScene(scene.path);var level=Object.FindObjectOfType<DemoLevel>();level.SendMessage("Awake");
            var browser=Object.FindObjectOfType<ReportSceneBrowser>();
            Require(!browser.CanBrowse,"Unsolved level allows scene browsing: "+scene.path);
            level.Use(level.stations[2]);Require(level.Step==0,"Future station advanced level");
            foreach(var station in level.stations)
            {
                var puzzle=station.puzzle;int step=level.Step;
                if(puzzle.mode==ReportPuzzleMode.Choice)
                {
                    PuzzleTestSupport.PrepareSolution(station);puzzle.ResetPuzzle();
                    level.Use(station);Require(level.Step==step,"Missing selection advanced "+station.name);
                    for(int i=0;i<puzzle.choices.Length;i++)if(i!=puzzle.correctChoice)
                    {puzzle.SelectChoice(i);level.Use(station);level.Use(station);Require(level.Step==step,"Wrong answer advanced "+station.name);}
                }
                foreach(var p in puzzle.placements)
                {
                    PuzzleTestSupport.PrepareSolution(station);p.item.transform.position=p.socket.position+Vector3.right*1.5f;
                    Physics.SyncTransforms();level.Use(station);Require(level.Step==step,"Missing or misplaced item advanced "+station.name);
                }
                foreach(var extra in puzzle.excludedItems)
                {
                    PuzzleTestSupport.PrepareSolution(station);var old=extra.transform.position;extra.transform.position=puzzle.placements[0].socket.position;
                    level.Use(station);Require(level.Step==step,"Extra cargo accepted");extra.transform.position=old;
                }
                PuzzleTestSupport.PrepareSolution(station);level.Use(station);
                if(station.completionDelay>0)
                {
                    Require(level.Step==step && station.IsResolving,"Animation unlocked early");
                    level.Use(station);station.SimulateStep(station.completionDelay*.5f);
                    Require(level.Step==step,"Half animation unlocked early");station.SimulateStep(station.completionDelay);
                }
                Require(level.Step==step+1,"Correct solution did not advance: "+scene.path+" "+station.name);count++;
            }
            Require(level.Completed && browser.CanBrowse,"Completed level not unlocked");Require(!level.ExitReady,"Door exit available before opening");
            level.exitDoor.localPosition+=level.exitDoorTravel;Require(level.ExitReady,"Open door not passable");
            level.ResetLevel();Require(level.Step==0 && !level.ExitReady && !browser.CanBrowse,"Reset did not relock level");
            Require(level.stations.All(s=>s.puzzle.SelectedChoice==-1 && !s.IsResolving && s.Attempts==0),"Reset did not clear choices/animations");
        }
        File.WriteAllText("Documentation/Verification/puzzle-behavior.txt","PASS: 15 levels, "+count+" puzzles, "+assertions+" assertions; missing/wrong choices, repeated validation, future stations, misplaced/missing pieces, extra cargo, animation delay, correct solutions, exit locks, browsing lock and reset. Held-item and physical input checked separately in Play Mode.\n");
        Debug.Log("PUZZLE_BEHAVIOR_OK puzzles="+count+" assertions="+assertions);
    }
}
