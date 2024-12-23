using System.Data;
using System.Diagnostics.Metrics;

namespace Game;

public struct SearchInfo {
    public int Iters;
    public int CurOpenNodeCount;
    public int MaxOpenNodeCount;
    public int CurCloseNodeCount;
    public int MaxCloseNodeCount;
    public int MaxNodeCount;
    
    public SearchInfo() {
        this.Iters = 0;

        this.CurOpenNodeCount = 0;
        this.MaxOpenNodeCount = 0;

        this.CurCloseNodeCount = 0;
        this.MaxCloseNodeCount = 0;

        this.MaxNodeCount = 0;
    }

    public void Update<T1, T2>(IEnumerable<T1> openNodes, IEnumerable<T2> closeNodes) {
        this.Iters++;
        this.CurOpenNodeCount = openNodes.Count();
        this.MaxOpenNodeCount = Math.Max(this.MaxOpenNodeCount, openNodes.Count());
        this.CurCloseNodeCount = closeNodes.Count();
        this.MaxCloseNodeCount = Math.Max(this.MaxCloseNodeCount, closeNodes.Count());
        this.MaxNodeCount = Math.Max(this.MaxNodeCount, openNodes.Count() + closeNodes.Count());
    }

    public override string ToString() {
        return 
$@"Iter: {this.Iters}
Cur O: {this.CurOpenNodeCount}
Cur C: {this.CurCloseNodeCount}
Max O: {this.MaxOpenNodeCount}
Max C: {this.MaxCloseNodeCount}
Max O + C: {this.MaxNodeCount}
";
    }
}

public interface ISearch {
    public List<State>? Search();
    public string GetStatistic();
}

// LAB №1
public class WidthFirstSearch(
    State start, State target,
    Func<State, List<State>> discovery
): ISearch {
    public Queue<State> OpenNodes = new(new State[] { start });
    public HashSet<State> CloseNodes = new();

    public SearchInfo Info = new();

    public List<State>? Search() {
        while (this.OpenNodes.Count > 0) {
            this.Info.Update(this.OpenNodes, this.CloseNodes);
            var node = this.OpenNodes.Dequeue();

            if (node.Equals(target)) {
                return node.GetPath();
            }
            this.CloseNodes.Add(node);

            foreach (var state in discovery(node)) {
                if (this.OpenNodes.Any(it => it.Equals(state))) continue;
                if (this.CloseNodes.Any(it => it.Equals(state))) continue;
                this.OpenNodes.Enqueue(state);
            }
        }

        return null;
    }

    public string GetStatistic() => this.Info.ToString();
}
public class DepthFirstSearch(
    State start, State target,
    Func<State, List<State>> discovery
): ISearch {
    public Stack<State> OpenNodes = new (new State[] { start });
    public HashSet<State> CloseNodes = new();

    public SearchInfo Info;

    public List<State>? Search() {
        while (this.OpenNodes.Count > 0) {
            this.Info.Update(this.OpenNodes, this.CloseNodes);

            var node = this.OpenNodes.Pop();
            if (node.Equals(target)) {
                return node.GetPath();
            }
            this.CloseNodes.Add(node);

            foreach (var state in discovery(node)) {
                if (this.OpenNodes.Any(it => node.Equals(state))) continue;
                if (this.CloseNodes.Any(it => node.Equals(state))) continue;
                this.OpenNodes.Push(state);
            }
        }

        return null;
    }

    public string GetStatistic() => this.Info.ToString();
}

// LAB №2
public class BiDirectionalSearch(
    State start, State target,
    Func<State, List<State>> discovery,
    Func<State, List<State>> revDiscovery
): ISearch {
    public Queue<State> StartOpenNodes = new(
        new State[] { start }
    );
    public HashSet<State> StartCloseNodes = new();

    public Queue<State> EndOpenNodes = new(
        new State[] { target }
    );
    public HashSet<State> EndCloseNodes = new();

    public struct BiDirInfo {
        public int Iters;

        public int StartIters;
        public int StartCurOpenNodes;
        public int StartMaxOpenNodes;
        public int StartCurCloseNodes;
        public int StartMaxCloseNodes;
        public int StartMaxNodes;

        public int EndIters;
        public int EndCurOpenNodes;
        public int EndMaxOpenNodes;
        public int EndCurCloseNodes;
        public int EndMaxCloseNodes;
        public int EndMaxNodes;

        public int MaxNodeCount;

        public BiDirInfo() {}
        public void UpdateStart(IEnumerable<State> openNodes, IEnumerable<State> closeNodes) {
            this.StartIters++;
            this.StartCurOpenNodes = openNodes.Count();
            this.StartMaxOpenNodes = Math.Max(this.StartMaxOpenNodes, openNodes.Count());
            this.StartCurCloseNodes = closeNodes.Count();
            this.StartMaxCloseNodes = Math.Max(this.StartMaxCloseNodes, closeNodes.Count());
            this.StartMaxNodes = Math.Max(this.StartMaxNodes, openNodes.Count() + closeNodes.Count());

            this.Iters++;
            this.MaxNodeCount = Math.Max(this.MaxNodeCount, this.StartCurOpenNodes + this.StartCurCloseNodes + this.EndCurOpenNodes + this.EndCurCloseNodes);
        }
        public void UpdateEnd(IEnumerable<State> openNodes, IEnumerable<State> closeNodes) {
            this.EndIters++;
            this.EndCurOpenNodes = openNodes.Count();
            this.EndMaxOpenNodes = Math.Max(this.EndMaxOpenNodes, openNodes.Count());
            this.EndCurCloseNodes = closeNodes.Count();
            this.EndMaxCloseNodes = Math.Max(this.EndMaxCloseNodes, closeNodes.Count());
            this.EndMaxNodes = Math.Max(this.EndMaxNodes, openNodes.Count() + closeNodes.Count());

            this.Iters++;
            this.MaxNodeCount = Math.Max(this.MaxNodeCount, this.StartCurOpenNodes + this.StartCurCloseNodes + this.EndCurOpenNodes + this.EndCurCloseNodes);
        }

        public override string ToString() {
            return
$@"COMMON
Iters: {this.Iters}
Max O + C: {this.MaxNodeCount}
START
Iters: {this.StartIters}
Cur O: {this.StartCurOpenNodes}
Cur C: {this.StartCurCloseNodes}
Max O: {this.StartMaxOpenNodes}
Max C: {this.StartMaxCloseNodes}
Max O + C: {this.StartMaxNodes}
END
Iters: {this.EndIters}
Cur O: {this.EndCurOpenNodes}
Cur C: {this.EndCurCloseNodes}
Max O: {this.EndMaxOpenNodes}
Max C: {this.EndMaxCloseNodes}
Max O + C: {this.EndMaxNodes}
";
        }

    }
    public BiDirInfo Info;

    public List<State>? Search() {
        if (start.Equals(target)) return new() { start };

        while(this.StartOpenNodes.Count() > 0 || this.EndOpenNodes.Count() > 0) {
            if (this.EndOpenNodes.Count > this.StartOpenNodes.Count && this.StartOpenNodes.Count != 0) {
                var newOpenNodes = new Queue<State>();
                // Console.WriteLine("start open before: " + this.StartOpenNodes.Count);
                foreach (var node in this.StartOpenNodes) {
                    this.Info.UpdateStart(this.StartOpenNodes.Concat(newOpenNodes), this.StartCloseNodes);
                    this.StartCloseNodes.Add(node);

                    foreach(var state in discovery(node)) {
                        var end = this.EndOpenNodes.FirstOrDefault(el => el.Equals(state), null);
                        if (end != null) return this._getPath(state, end);

                        if (
                            this.StartOpenNodes.Any(it => it.Equals(state)) ||
                            this.StartCloseNodes.Any(it => it.Equals(state))
                        ) continue;
                        newOpenNodes.Enqueue(state);
                    }
                }
                this.StartOpenNodes = newOpenNodes;
                // Console.WriteLine("start open after: " + this.StartOpenNodes.Count);
            } else {
                var newOpenNodes = new Queue<State>();
                // Console.WriteLine("end open before: " + this.StartOpenNodes.Count);
                foreach (var node in this.EndOpenNodes) {
                    this.Info.UpdateEnd(this.EndOpenNodes.Concat(newOpenNodes), this.EndCloseNodes);
                    this.EndCloseNodes.Add(node);

                    // Console.WriteLine(revDiscovery(node).Count);
                    foreach(var state in revDiscovery(node)) {
                        var start = this.StartOpenNodes.FirstOrDefault(el => el.Equals(state), null);
                        if (start != null) return this._getPath(start, state);

                        if (
                            this.EndOpenNodes.Any(it => it.Equals(state)) ||
                            this.EndCloseNodes.Any(it => it.Equals(state))
                        ) continue;
                        newOpenNodes.Enqueue(state);
                    }
                }
                this.EndOpenNodes = newOpenNodes;
                // Console.WriteLine("end open after: " + newOpenNodes.Count);
                // Console.WriteLine("end open after: " + this.EndOpenNodes.Count);
            }
        }

        // this._printNodes();
        return new();
    }

    public string GetStatistic() => this.Info.ToString();

    private List<State>? _getPath(State start, State end) {
        List<State>
            startPath = start.GetPath(),
            endPath = end.GetPath();

        endPath.Reverse();
        endPath.RemoveAt(0);

        return startPath.Concat(endPath).ToList();
    }

    private void _printNodes() {
        Console.WriteLine("START");
        Console.WriteLine("open: " + this.StartOpenNodes.Count);
        foreach (var node in this.StartOpenNodes) {
            Console.WriteLine(node);
            Console.WriteLine();
        }
        Console.WriteLine("close: " + this.StartCloseNodes.Count);
        foreach (var node in this.StartCloseNodes) {
            Console.WriteLine(node);
            Console.WriteLine();
        }

        Console.WriteLine("END");
        Console.WriteLine("open: " + this.EndOpenNodes.Count);
        foreach (var node in this.EndOpenNodes) {
            Console.WriteLine(node);
            Console.WriteLine();
        }
        Console.WriteLine("close: " + this.EndCloseNodes.Count);
        foreach (var node in this.EndCloseNodes) {
            Console.WriteLine(node);
            Console.WriteLine();
        }
    }
}

public class DepthLimitedSearch(
    State start, State target,
    Func<State, List<State>> discovery
): ISearch {
    public Stack<(State node, int depth)> OpenNodes = new(
        new (State node, int depth)[] {
            new(start, 0)
    });
    public HashSet<State> CloseNodes = new();
    private int maxDepth = 1;

    public SearchInfo Info;

    public List<State>? Search() {
        State startState = OpenNodes.Peek().node;

        while (true) {
            while (this.OpenNodes.Count > 0) {
                this.Info.Update(this.OpenNodes,this.CloseNodes);
                var (node, depth) = this.OpenNodes.Pop();

                if (node.Equals(target)) {
                    return node.GetPath();
                }

                this.CloseNodes.Add(node);

                if (depth < maxDepth) {
                    foreach (var state in discovery(node)) {
                        if (this.OpenNodes.Any(it => it.node.Equals(state))) continue;
                        if (this.CloseNodes.Any(it => it.Equals(state))) continue;
                        this.OpenNodes.Push((state, depth + 1));
                    }
                }
            }

            maxDepth++;
            CloseNodes.Clear();
            OpenNodes.Clear();
            OpenNodes.Push((startState, 0));
        }
    }

    public string GetStatistic() => this.Info.ToString();
}


// LAB №3
public class AStar(
    State start, State target,
    Func<State, List<State>> discovery,
    Func<State, State, uint> heuristics
): ISearch {
    public List<(State state, uint val)> OpenNodes = new(
        new (State, uint)[] { 
                (start, heuristics(start, target)),
            }
        );
    public HashSet<(State state, uint val)> CloseNodes = new();

    public SearchInfo Info = new();

    public List<State> Search() {
        while (this.OpenNodes.Count > 0) {
            this.Info.Update(this.OpenNodes, this.CloseNodes);
            var item = this.OpenNodes.First();
            this.OpenNodes.RemoveAt(0);
            if (item.state.Equals(target)) {
                return item.state.GetPath();
            }
            this.CloseNodes.Add(item);

            var traveledPath = item.state.GetPath().Count - 1;
            foreach (var state in discovery(item.state)) {
                var newVal = (uint)(traveledPath + heuristics(state, target));

                var openNodeIndex = this.OpenNodes.FindIndex(((State, uint) it) => it.Item1.Equals(state));
                if (openNodeIndex > -1) {
                    if (newVal < this.OpenNodes[openNodeIndex].val) {
                        this.OpenNodes[openNodeIndex] = (state, newVal);
                    }
                    continue;
                }
                
                (State? state, uint val) inCloseNode = this.CloseNodes.FirstOrDefault(item => item.state.Equals(state), (null, 0));
                if (inCloseNode.state != null) {
                    if (newVal < inCloseNode.val) {
                        this.CloseNodes.Remove(inCloseNode);
                        this.OpenNodes.Add((state, newVal));
                    }
                    continue;
                }

                this.OpenNodes.Add((state, newVal));
            }
        
            this._sortOpenNodes();
        }

        return null;
    }

    public string GetStatistic() => this.Info.ToString();

    private void _sortOpenNodes() {
        this.OpenNodes.Sort(comparison: (first, second) => (int)first.val - (int)second.val );
    }
}