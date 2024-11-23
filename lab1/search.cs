using System.Runtime.CompilerServices;

namespace Game;

public struct SearchInfo {
    public int Iters;
    public int CurOpenNodeCount;
    public int MaxOpenNodeCount;
    public int MaxNodeCount;
    
    public SearchInfo() {
        this.Iters = 0;
        this.CurOpenNodeCount = 0;
        this.MaxOpenNodeCount = 0;
        this.MaxNodeCount = 0;
    }

    public void Update(int curOpenNodeCount, int maxNodeCount) {
        this.Iters++;
        this.CurOpenNodeCount = curOpenNodeCount;
        if (curOpenNodeCount > this.MaxOpenNodeCount) {
            this.MaxOpenNodeCount = curOpenNodeCount;
        }
        if (maxNodeCount > this.MaxNodeCount) {
            this.MaxNodeCount = maxNodeCount;
        }
    }

    public override string ToString() {
        return 
        $@"Iter count: {this.Iters}
O node count: {this.CurOpenNodeCount}
O max node count: {this.MaxOpenNodeCount}
O + C max node count: {this.MaxNodeCount}
";
    }
}

public interface ISearch {
    public List<State>? Search();
    public string GetStatistic();
}

// LAB №1
public class WidthFirstSearch(State StartState): ISearch {
    public Queue<State> OpenNodes = new(new State[] { StartState });
    public HashSet<State> CloseNodes = new();

    public SearchInfo Info = new();

    public string SaveFile = "report//width_search.txt";

    public List<State>? Search() {
        while (this.OpenNodes.Count > 0) {
            this.Info.Update(
                this.OpenNodes.Count,
                this.OpenNodes.Count + this.CloseNodes.Count
            );
            var node = this.OpenNodes.Dequeue();

            if (node.IsTargetState()) {
                Console.WriteLine(this.Info);
                Console.WriteLine("Search finished");

                return node.GetPath();
            }
            this.CloseNodes.Add(node);

            foreach (var state in node.Discovery()) {
                if (this.OpenNodes.Contains(state)) continue;
                if (this.CloseNodes.Contains(state)) continue;
                this.OpenNodes.Enqueue(state);
            }
        }

        Console.WriteLine("Search finished");
        return null;
    }

    public string GetStatistic() => this.Info.ToString();
}

public class DepthFirstSearch(State StartState): ISearch {
    public Stack<State> OpenNodes = new (new State[] { StartState });
    public HashSet<State> CloseNodes = new();

    public SearchInfo Info;

    public List<State>? Search() {
        while (this.OpenNodes.Count > 0) {
            this.Info.Update(
            this.OpenNodes.Count,
            this.OpenNodes.Count + this.CloseNodes.Count
            );

            var node = this.OpenNodes.Pop();

            if (node.IsTargetState()) {
                Console.WriteLine(this.Info);
                Console.WriteLine("Search finished");
                return node.GetPath();
            }
            this.CloseNodes.Add(node);

            foreach (var state in node.Discovery()) {
                if (this.OpenNodes.Any(n => node.Equals(state))) continue;
                if (this.CloseNodes.Contains(state)) continue;
                this.OpenNodes.Push(state);
            }
        }

        Console.WriteLine("Search finished");
        return null;
    }

    public string GetStatistic() => this.Info.ToString();
}

// LAB №2
public class BiDirectionalSearch(State start): ISearch {
    public Queue<State> StartOpenNodes = new(new State[] { start });
    public HashSet<State> StartCloseNodes = new();
    public SearchInfo StartInfo;

    public Queue<State> EndOpenNodes = new(new State[] { State.TARGET_STATE });
    public HashSet<State> EndCloseNodes = new();
    public SearchInfo EndInfo;

    public List<State>? Search() {
        while(this.StartOpenNodes.Count() > 0 || this.EndOpenNodes.Count() > 0) {
            var startNode = this.StartOpenNodes.Dequeue();
            var endNode = this.EndOpenNodes.Dequeue();

            this.StartCloseNodes.Add(startNode);
            this.EndCloseNodes.Add(endNode);

            foreach(var state in startNode.Discovery()) {
                if (this.StartOpenNodes.Contains(state)) continue;
                if (this.StartCloseNodes.Contains(state)) continue;
                this.StartOpenNodes.Enqueue(state);
            }
            foreach(var state in endNode.Discovery()) {
                if (this.EndOpenNodes.Contains(state)) continue;
                if (this.EndCloseNodes.Contains(state)) continue;
                this.EndOpenNodes.Enqueue(state);
            }

            this.StartInfo.Update(this.StartOpenNodes.Count, this.StartOpenNodes.Count + this.StartCloseNodes.Count);
            this.EndInfo.Update(this.EndOpenNodes.Count, this.EndOpenNodes.Count + this.StartCloseNodes.Count);

            if (this.EndOpenNodes.Contains(startNode)) {
                this._printInfo();

                endNode = this.EndOpenNodes.First(el => el.Equals(startNode));
                var path = this._getPath(startNode, endNode);
                return path;
            }

            if (this.StartOpenNodes.Contains(endNode)) {
                this._printInfo();

                startNode = this.StartOpenNodes.First(el => el.Equals(endNode));
                var path = this._getPath(startNode, endNode);
                return path;
            }
        }

        Console.WriteLine("Search finished");
        return null;
    }

    public string GetStatistic() => "Start\n" + this.StartInfo.ToString() + "End\n" + this.EndInfo.ToString();

    private List<State>? _getPath(State start, State end) {
                List<State>
                    startPath = start.GetPath(),
                    endPath = end.GetPath();

                endPath.Reverse();
                endPath.RemoveAt(0);

                return startPath.Concat(endPath).ToList();
    }

    private void _printInfo() {
                Console.WriteLine("Start:");
                Console.WriteLine(this.StartInfo);

                Console.WriteLine("End:");
                Console.WriteLine(this.EndInfo);

                Console.WriteLine("Search finished");
    }
}

public class DepthLimitedSearch : ISearch {
    public Stack<(State node, int depth)> OpenNodes = new();
    public HashSet<State> CloseNodes = new();
    private int maxDepth = 1;

    public SearchInfo Info;

    public DepthLimitedSearch(State startState) {
        OpenNodes.Push((startState, 0));
    }

    public List<State>? Search() {
        while (this.OpenNodes.Count > 0) {
            var (node, depth) = this.OpenNodes.Pop();

            if (node.IsTargetState()) {
                this.Info.Update(
                this.OpenNodes.Count(),
                this.OpenNodes.Count() + this.CloseNodes.Count()
                );
                Console.WriteLine(this.Info);
                Console.WriteLine("Search finished");
                return node.GetPath();
            }

            this.CloseNodes.Add(node);

            if (depth < maxDepth) {
                foreach (var state in node.Discovery()) {
                    if (this.OpenNodes.Any(n => n.node.Equals(state))) continue;
                    if (this.CloseNodes.Contains(state)) continue;
                    this.OpenNodes.Push((state, depth + 1));
                }
            }

            if (this.OpenNodes.Count == 0) {
                maxDepth++;
                CloseNodes.Clear();
                OpenNodes.Push((node, 0));
            }
            this.Info.Update(
            this.OpenNodes.Count(),
            this.OpenNodes.Count() + this.CloseNodes.Count()
            );
        }

        Console.WriteLine("Search finished");
        return null;
    }

    public string GetStatistic() => this.Info.ToString();
}


// LAB №3
public class AStar(State start, Func<State, State, uint> heuristics): ISearch {
    public List<(State state, uint val)> OpenNodes = new(
        new (State, uint)[] { 
                (start, heuristics(start, State.TARGET_STATE)),
            }
        );
    public HashSet<(State state, uint val)> CloseNodes = new();

    public SearchInfo Info = new();

    public List<State>? Search() {
        // Console.WriteLine("hash: " + start.GetHashCode());
        while (this.OpenNodes.Count > 0) {
            this.Info.Update(
                this.OpenNodes.Count,
                this.OpenNodes.Count + this.CloseNodes.Count
            );
            var item = this.OpenNodes.First();
            this.OpenNodes.RemoveAt(0);
            if (item.state.IsTargetState()) {
                Console.WriteLine(this.Info);
                Console.WriteLine("Search finished");
                return item.state.GetPath();
            }
            this.CloseNodes.Add(item);

            var traveledPath = (uint)item.state.GetPath().Count;
            foreach (var state in item.state.Discovery()) {
                var newVal = traveledPath + heuristics(state, State.TARGET_STATE);

                var openNodeIndex = this.OpenNodes.FindIndex(((State, uint) item) => item.Item1.Equals(state));
                if (openNodeIndex > -1 && newVal < this.OpenNodes[openNodeIndex].val) {
                    this.OpenNodes[openNodeIndex] = (state, newVal);
                    continue;
                }
                
                (State? state, uint val) inCloseNode = this.CloseNodes.FirstOrDefault(item => item.state.Equals(state), (null, 0));
                if (inCloseNode.state != null && newVal < inCloseNode.val) {
                    this.CloseNodes.Remove(inCloseNode);
                    this.OpenNodes.Add((state, newVal));
                    continue;
                }

                this.OpenNodes.Add((state, newVal));
            }
        
            this._sortOpenNodes();
        }

        Console.WriteLine("Search finished");
        return null;
    }

    public string GetStatistic() => this.Info.ToString();

    private void _sortOpenNodes() {
        this.OpenNodes.Sort(comparison: (first, second) => (int)first.val - (int)second.val );
    }
}