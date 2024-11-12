// IT'S ALL FOR EXTRA TASK 2

using Raylib_cs;
using rl = Raylib_cs.Raylib;
using System.Collections;

namespace Game;

class StateDB: Hashtable {
    public override int GetHashCode() {
        return base.GetHashCode();
    }
}

class StateGen(Color selectedColor) {
    public Queue<State?> OpenNodes = new(new State[]{ State.TARGET_STATE });
    public HashSet<State?> CloseNodes;

    public void genDBStates() {
        while (this.OpenNodes.Count > 0) {
            var curState = this.OpenNodes.Dequeue();
            foreach (var state in curState.Discovery()) {
                State? find; 

                find = this.OpenNodes.FirstOrDefault(el => state.EqualsByColor(el, selectedColor), null);
                if (find != null) continue;
                find = this.CloseNodes.FirstOrDefault(el => state.EqualsByColor(el, selectedColor), null);
                if (find != null) continue;

                this.OpenNodes.Append(state);
            }
        }

        Console.WriteLine("Count of All Possible States: " + this.CloseNodes.Count);
        this.writeStatesToFile("db/red_subtask.txt");
    }
    
    private void writeStatesToFile(string file) {
        var sw = new StreamWriter(file);
        {
            foreach (var state in this.CloseNodes) {
                sw.WriteLine(string.Join(" ", state.GetColorPositions(selectedColor)));
            }
        }
        sw.Close();
    }
}