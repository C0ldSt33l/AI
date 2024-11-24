namespace Game;

public class Test {
    static public void GenStarStates() {
        using (var file = File.AppendText("report//start_states.txt")) {
            for (int step = 2, depth = 2, i = 0; i < 5; i++, depth += step) {
                var states = new List<State>();
                while (states.Count < 10) {
                    var state = State.AddSomeChaosTo(State.TARGET_STATE, (uint)depth);
                    if (states.Contains(state)) continue;
                    states.Add(state);
                }

                file.WriteLine("Depth: " + depth);
                foreach (var state in states) {
                    file.WriteLine(state);
                    file.WriteLine();
                }
                file.WriteLine("--------------------------------");
            }
        }
    }
}