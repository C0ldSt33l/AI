namespace Game;

class Program {
    public static void Main(String[] args) {
        new Game().Update();

        // Test.RunTests(searches: new string[] { "AStarDB" });
        // Test.ImpossibleTest(new string[] { "width" });

//         new BiDirectionalSearch(
//             Test.GetStartStates()[6][0],
//             State.TARGET_STATE,
//             State.Discovery,
//             State.ReverseDiscovery
// ).Search();
        // var file = "bidir.txt";
        // var dict = Test.GetStartStates();
        // foreach (var (deapth, states) in dict) {
        //     File.AppendAllText(file, "Depth: " + deapth + '\n');
        //     foreach (var state in states) {
        //         var search = new BiDirectionalSearch(
        //             state, State.TARGET_STATE,
        //             State.Discovery,
        //             State.ReverseDiscovery
        //         );

        //         var pathLength = search.Search().Count - 1;
        //         File.AppendAllLines(file, new string[] {
        //             "Iters: " + search.Info.Iters,
        //             "Max O + C: " + search.Info.MaxNodeCount,
        //             ""
        //         });
        //     }
        // }
    }
}