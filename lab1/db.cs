// IT'S ALL FOR EXTRA TASK 2

using Raylib_cs;
using rl = Raylib_cs.Raylib;
using System.Collections;
using System.Data.Common;

namespace Game;

static class DB {
    public static void genDBStates() {
        var OpenNodes = new Queue<State?>(new[] { State.TARGET_STATE });
        var CloseNodes = new HashSet<State?>();
        while (OpenNodes.Count > 0) {
            var curState = OpenNodes.Dequeue();
            CloseNodes.Add(curState);
            foreach (var state in State.FullDiscovery(curState)) {
                State? find; 

                find = OpenNodes.FirstOrDefault(el => state.EqualsByColor(el, Color.Red), null);
                if (find != null) continue;
                find = CloseNodes.FirstOrDefault(el => state.EqualsByColor(el, Color.Red), null);
                if (find != null) continue;

                OpenNodes.Enqueue(state);
            }
        }

        Console.WriteLine("Count of All Possible States: " + CloseNodes.Count);
        DB.writeStatesToFile(CloseNodes, "db/red_subtask.txt");
    }

    public static void calculateDBStatesPathLengths() {
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
    
    private static void writeStatesToFile(HashSet<State?> closeNodes, string file) {
        var sw = new StreamWriter(file);
        {
            foreach (var state in closeNodes) {
                sw.WriteLine(state.GetColorPositions(Color.Red));
            }
        }
        sw.Close();
    }
}