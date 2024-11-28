// IT'S ALL FOR EXTRA TASK 2

using Raylib_cs;
using rl = Raylib_cs.Raylib;
using System.Collections;

namespace Game;

class DB(Color selectedColor) {
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

    public void calculateDBStatesPathLengths() {
        var positions = File.ReadAllLines("db//color_pos.txt");
        var targets = new State[] {
            new(new char[4, 4] {
                { 'R', 'R', 'R', 'R', },
                { '?', '?', '?', '?', },
                { '?', '?', '?', '?', },
                { '?', '?', '?', '?', },
            }),
            new(new char[4, 4] {
                { '?', '?', '?', '?', },
                { 'G', 'G', 'G', 'G', },
                { '?', '?', '?', '?', },
                { '?', '?', '?', '?', },
            }),
            new(new char[4, 4] {
                { '?', '?', '?', '?', },
                { '?', '?', '?', '?', },
                { 'Y', 'Y', 'Y', 'Y', },
                { '?', '?', '?', '?', },
            }),
            new(new char[4, 4] {
                { '?', '?', '?', '?', },
                { '?', '?', '?', '?', },
                { '?', '?', '?', '?', },
                { 'B', 'B', 'B', 'B', },
            }),
        };

        for (var i = 0; i < 4; i++) {
            var dict = new Dictionary<string, int>();
            var states = new State[positions.Length];
            (Color color, string file) = i switch {
                0 => (Color.Red, "red"),
                1 => (Color.Green, "green"),
                2 => (Color.Yellow, "yellow"),
                3 => (Color.Blue, "blue"),
            };
            for (var j = 0; j < positions.Length; j++) {
                states[j] = new State(
                    Array.ConvertAll(positions[j].Trim().Split(" "), Int32.Parse),
                    color
                );
            }
            for (var j = 0; j < positions.Length; j++) {
                Console.WriteLine(j);
                dict[positions[j]] = new BiDirectionalSearch(
                    states[j], targets[i],
                    State.FullDiscovery,
                    State.FullDiscovery
                ).Search().Count - 1;
            }
            foreach ((string pos, int len) in dict) {
                File.AppendAllText("db//" + file + "_subtask.txt", pos + ":" + len + "\n");
            }
        }
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