using System.Runtime.Versioning;
using Microsoft.Win32.SafeHandles;
using Raylib_cs;

namespace Game;

public static class Test {
    public static void GenStarStates() {
        var fileName = "report//start_states.txt";
            for (int step = 1, depth = 2, i = 0; i < 5; i++, depth += step) {
                var states = new List<State>();
                File.AppendAllText(fileName, "Depth: " + depth + "\n");
                while (states.Count < 10) {
                    var state = State.TARGET_STATE.AddSomeChaos((uint)depth);
                    if (states.Contains(state)) continue;
                    states.Add(state);
                }
                states.ForEach(it => {
                    File.AppendAllText(fileName, it.ToString() + "\n\n");
                });
                File.AppendAllText(fileName, "--------------------\n");
            }
    }

    public static void RunTests(string[] searches) {
        var dict = Test.GetStartStates();
        foreach (var name in searches) {
            foreach (var pair in dict) {
                var file = "report//" + name + ".txt";
                File.AppendAllText(file, "Depth: " + pair.Key + "\n");
                foreach (var state in pair.Value) {
                    ISearch search = name.ToLower() switch {
                        "width" => new WidthFirstSearch(state),
                        "depth" => new DepthFirstSearch(state),
                        "bidirectional" => new BiDirectionalSearch(state, State.TARGET_STATE),
                        "depth limited" => new DepthLimitedSearch(state),
                        "astar1" => new AStar(state, State.Heuristics1),
                        "astar2" => new AStar(state, State.Heuristics2),
                        "astar3" => new AStar(state, State.TheMostFoolishHeuristics),
                        _ => throw new Exception("Such search is not exist"),
                    };
                    var path = search.Search();
                    File.AppendAllText(file, search.GetStatistic());
                    File.AppendAllText(file, "Path length: " + (path.Count - 1) + "\n\n");
                }
            }
        }
    }

    private static Dictionary<uint, List<State>> GetStartStates(string file = "report//start_states.txt") {
        var dict = new Dictionary<uint, List<State>>();
        var lines = File.ReadAllLines(file);
        for (var i = 0; i < lines.Length; i++) {
            if (lines[i].Contains("D")) {
                var depthLine = lines[i].Substring(lines[i].IndexOf(' ') + 1);
                var depth = UInt32.Parse(depthLine);
                i += 1;
                
                var states = new List<State>();
                for (;!lines[i].Contains("-"); i += 5) {
                    var colors = new char[4,4];
                    var state = new State(lines[(i)..(i + 5)]);
                    states.Add(state);
                }
                dict[depth] = states;
            }
        }

        return dict;
    }
}