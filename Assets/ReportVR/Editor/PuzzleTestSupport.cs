using UnityEngine;

// Test-only preparation; never attach this Editor class to a playable scene.
public static class PuzzleTestSupport
{
    public static void PrepareSolution(DemoStation station)
    {
        var puzzle=station.puzzle;if(puzzle==null || station.IsResolving)return;
        foreach(var placement in puzzle.placements)
        {
            placement.item.transform.position=placement.socket.position;
            var body=placement.item.GetComponent<Rigidbody>();body.position=placement.socket.position;body.velocity=Vector3.zero;body.angularVelocity=Vector3.zero;body.Sleep();
        }
        if(puzzle.mode==ReportPuzzleMode.Choice)puzzle.SelectChoice(puzzle.correctChoice);
        Physics.SyncTransforms();
    }
}
