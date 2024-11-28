// IT'S ALL FOR EXTRA TASK 2

using Raylib_cs;
using rl = Raylib_cs.Raylib;
using System.Collections;

namespace Game;

class StateGen(Color selectedColor) {
    public Queue<State?> OpenNodes = new(new State[]{ State.TARGET_STATE });
    public HashSet<State?> CloseNodes = new();

    public void genDBStates() {
        while (this.OpenNodes.Count > 0) {
            var curState = this.OpenNodes.Dequeue();
            this.CloseNodes.Add(curState);
            foreach (var state in State.FullDiscovery(curState)) {
                State? find; 

                find = this.OpenNodes.FirstOrDefault(el => state.EqualsByColor(el, selectedColor), null);
                if (find != null) continue;
                find = this.CloseNodes.FirstOrDefault(el => state.EqualsByColor(el, selectedColor), null);
                if (find != null) continue;

                this.OpenNodes.Enqueue(state);
            }
        }

        Console.WriteLine("Count of All Possible States: " + this.CloseNodes.Count);
        this.writeStatesToFile("db/red_subtask.txt");
    }
    
    private void writeStatesToFile(string file) {
        var sw = new StreamWriter(file);
        {
            foreach (var state in this.CloseNodes) {
                sw.WriteLine(state.GetColorPositions(selectedColor));
            }
        }
        sw.Close();
    }
}