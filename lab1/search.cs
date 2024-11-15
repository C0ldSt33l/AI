using Raylib_cs;
using rl = Raylib_cs.Raylib;

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
}

// LAB №1
public class WidthFirstSearch(State StartState): ISearch {
    public Queue<State> OpenNodes = new(new State[] { StartState });
    public HashSet<State> CloseNodes = new();

    public SearchInfo info = new();

    public List<State>? Search() {
        while (this.OpenNodes.Count > 0) {
            this.info.Update(
                this.OpenNodes.Count,
                this.OpenNodes.Count + this.CloseNodes.Count
            );
            var node = this.OpenNodes.Dequeue();

            if (node.IsTargetState()) {
                Console.WriteLine(this.info);
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
}

public class DepthFirstSearch(State StartState): ISearch {
    public Stack<State> OpenNodes = new (new State[] { StartState });
    public HashSet<State> CloseNodes = new();

    public SearchInfo info;

    public List<State>? Search() {
        while (this.OpenNodes.Count > 0) {
            this.info.Update(
            this.OpenNodes.Count,
            this.OpenNodes.Count + this.CloseNodes.Count
            );

            var node = this.OpenNodes.Pop();

            if (node.IsTargetState()) {
                Console.WriteLine(this.info);
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
}


// public class DepthFirstSearch(State StartState): ISearch {
//     public Stack<State> OpenNodes = new (new State[] { StartState });
//     public HashSet<State> CloseNodes = new();

//     public SearchInfo info;

//     public List<State>? Search() {
//         while (this.OpenNodes.Count > 0) {
//             this.info.Update(
//                 this.OpenNodes.Count(), 
//                 this.OpenNodes.Count() + this.CloseNodes.Count()
//             );
 
//             var node = this.OpenNodes.Pop();

//             if (node.IsTargetState()) {
//                 Console.WriteLine(this.info);
//                 Console.WriteLine("Search finished");
//                 return node.GetPath();
//             }
//             this.CloseNodes.Add(node);

//             foreach (var state in node.Discovery()) {
//                 if (this.OpenNodes.Contains(state)) continue;
//                 if (this.CloseNodes.Contains(state)) continue;
//                 this.OpenNodes.Push(state);
//             }
//         }

//         Console.WriteLine("Search finished");
//         return null;
//     }
// }

// LAB №2
public class BiDirectionalSearch(State start): ISearch {
    public Queue<State> startOpenNodes = new(new State[] { start });
    public HashSet<State> startCloseNodes = new();
    public SearchInfo startInfo;

    public Queue<State> endOpenNodes = new(new State[] { State.TARGET_STATE });
    public HashSet<State> endCloseNodes = new();
    public SearchInfo endInfo;

    public List<State>? Search() {
        while(this.startOpenNodes.Count() > 0 || this.endOpenNodes.Count() > 0) {
            var startNode = this.startOpenNodes.Dequeue();
            var endNode = this.endOpenNodes.Dequeue();

            this.startCloseNodes.Add(startNode);
            this.endCloseNodes.Add(endNode);

            foreach(var state in startNode.Discovery()) {
                if (this.startOpenNodes.Contains(state)) continue;
                if (this.startCloseNodes.Contains(state)) continue;
                this.startOpenNodes.Enqueue(state);
            }
            foreach(var state in endNode.Discovery()) {
                if (this.endOpenNodes.Contains(state)) continue;
                if (this.endCloseNodes.Contains(state)) continue;
                this.endOpenNodes.Enqueue(state);
            }

            this.startInfo.Update(this.startOpenNodes.Count, this.startOpenNodes.Count + this.startCloseNodes.Count);
            this.endInfo.Update(this.endOpenNodes.Count, this.endOpenNodes.Count + this.startCloseNodes.Count);

            if (this.endOpenNodes.Contains(startNode)) {
                this._printInfo();

                endNode = this.endOpenNodes.First(el => el.Equals(startNode));
                var path = this._getPath(startNode, endNode);
                return path;
            }

            if (this.startOpenNodes.Contains(endNode)) {
                this._printInfo();

                startNode = this.startOpenNodes.First(el => el.Equals(endNode));
                var path = this._getPath(startNode, endNode);
                return path;
            }
        }

        Console.WriteLine("Search finished");
        return null;
    }

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
                Console.WriteLine(this.startInfo);

                Console.WriteLine("End:");
                Console.WriteLine(this.endInfo);

                Console.WriteLine("Search finished");
    }
}

public class DepthLimitedSearch : ISearch {
    public Stack<(State node, int depth)> OpenNodes = new();
    public HashSet<State> CloseNodes = new();
    private int maxDepth = 1;

    public SearchInfo info;

    public DepthLimitedSearch(State startState) {
        OpenNodes.Push((startState, 0));
    }

    public List<State>? Search() {
        while (this.OpenNodes.Count > 0) {
            var (node, depth) = this.OpenNodes.Pop();

            if (node.IsTargetState()) {
                this.info.Update(
                this.OpenNodes.Count(),
                this.OpenNodes.Count() + this.CloseNodes.Count()
                );
                Console.WriteLine(this.info);
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
            this.info.Update(
            this.OpenNodes.Count(),
            this.OpenNodes.Count() + this.CloseNodes.Count()
            );
        }

        Console.WriteLine("Search finished");
        return null;
    }
}


// LAB №3
public class AStar(State start, Func<State, State, uint> heuristics): ISearch {
    public List<(State state, uint val)> OpenNodes = new(
        new (State, uint)[] { 
                (start, heuristics(start, State.TARGET_STATE)),
            }
        );
    public HashSet<(State state, uint val)> CloseNodes = new();

    public SearchInfo info = new();

    public List<State>? Search() {
        // Console.WriteLine("hash: " + start.GetHashCode());
        while (this.OpenNodes.Count > 0) {
            this.info.Update(
                this.OpenNodes.Count,
                this.OpenNodes.Count + this.CloseNodes.Count
            );
            var item = this.OpenNodes.First();
            this.OpenNodes.RemoveAt(0);
            if (item.state.IsTargetState()) {
                Console.WriteLine(this.info);
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

    private void _sortOpenNodes() {
        this.OpenNodes.Sort(comparison: (first, second) => (int)first.val - (int)second.val );
    }
}