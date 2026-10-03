using UnityEngine;

public enum ReportPuzzleButtonKind {Choice, Hint}
public sealed class ReportPuzzleButton : MonoBehaviour
{
    public ReportPuzzle puzzle;
    public ReportPuzzleButtonKind kind;
    public int choiceIndex;
    public Renderer indicator;
    public void Press()
    {
        if(kind==ReportPuzzleButtonKind.Hint)puzzle.RequestHint();else puzzle.SelectChoice(choiceIndex);
    }
    public void Refresh()
    {
        if(indicator==null)return;
        var block=new MaterialPropertyBlock();block.SetColor("_Color",puzzle.SelectedChoice==choiceIndex && kind==ReportPuzzleButtonKind.Choice?new Color(.96f,.71f,.3f):new Color(.45f,.7f,.64f));indicator.SetPropertyBlock(block);
    }
}
