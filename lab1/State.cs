using System.Text;
using Raylib_cs;
using rl = Raylib_cs.Raylib;

namespace Game;

public class State {
    public static readonly State TARGET_STATE = new(new char[4,4] {
        {'R', 'R', 'R', 'R'},
        {'G', 'G', 'G', 'G'},
        {'Y', 'Y', 'Y', 'Y'},
        {'B', 'B', 'B', 'B'},
    });
    public readonly State? Parent = null;
    public readonly Color[,] Colors = new Color[4, 4];

    public State(Circle[,] circles) {
        for (var row = 0; row < circles.GetLength(0); row++) {
            for (var col = 0; col < circles.GetLength(1); col++) {
                this.Colors[row, col] = circles[row, col].Color;
            }
        }
        this.Parent = null;
    }
    public State(Color[,] colors, State? parent) {
        this.Colors = colors;
        this.Parent = parent;
    }
    
    public State(char[,] chars) {
        for (var row = 0; row < 4; row++) {
            for (var col = 0; col < 4; col++) {
                this.Colors[row, col] = chars[row, col] switch {
                    'R' => Color.Red,
                    'G' => Color.Green,
                    'Y' => Color.Yellow,
                    'B' => Color.Blue
                };
            }
        }
        this.Parent = null;
    }

    public List<State> Discovery() {
        var states = new List<State>();
        for (var i = 0; i < 4; i++) {
            states.Add(this.moveRow(i, Direction.LEFT));
            states.Add(this.moveCol(i, Direction.UP));
            states.Add(this.moveRow(i, Direction.RIGHT));
            states.Add(this.moveCol(i, Direction.DOWN));
        }

        return states;
    }

    public List<State> GetPath() {
        var path = new List<State>();
        var node = this;
        while (node.Parent != null) {
            path.Add(node);
            node = node.Parent;
        }
        path.Add(node);
        path.Reverse();
        return path;
    }

    public bool IsTargetState() {
        return this.Equals(TARGET_STATE);
    }

    // Search how many colors not on its own places
    public uint Heuristics1(State target) {
        uint value = 0;
        for (var row = 0; row < this.Colors.GetLength(0); row++) {
            for (var col = 0; col < this.Colors.GetLength(1); col++) {
                if (!this.Colors[row, col].Equals(target.Colors[row, col])) value++;
            }
        }

        return value;
    }

    // how many rows in right color?
    // 
    public uint TheMostFoolishnessHeuristics(State target) {
        uint
            rowsNotInPlace = 0,
            colsNotInPlace = 0;

        var rows = this._getRows();
        var cols = this._getCols();
        var targetRows = target._getRows();
        var targetCols = target._getCols();

        for (var i = 0; i < 4; i++) {
            for (var j = 0; j < 4; j++) {
                if (!rows[i][j].Equals(targetRows[i][j])) {
                    rowsNotInPlace++;
                    break;
                }
            }
        }

        for (var i = 0; i < 4; i++) {
            for (var j = 0; j < 4; j++) {
                if (!cols[i][j].Equals(targetCols[i][j])) {
                    colsNotInPlace++;
                    break;
                }
            }
        }

        return rowsNotInPlace + colsNotInPlace;
    }

    private Color[][] _getRows() {
        var rows = new Color[4][] {
            new Color[4] { Color.White, Color.White, Color.White, Color.White },
            new Color[4] { Color.White, Color.White, Color.White, Color.White },
            new Color[4] { Color.White, Color.White, Color.White, Color.White },
            new Color[4] { Color.White, Color.White, Color.White, Color.White },
        };
        for (var row = 0; row < this.Colors.GetLength(0); row++) {
            for (var col = 0; col < this.Colors.GetLength(1); col++) {
                rows[row][col] = this.Colors[row, col];
            }
        }

        return rows;
    }

    private Color[][] _getCols() {
        var cols = new Color[4][] {
            new Color[4] { Color.White, Color.White, Color.White, Color.White },
            new Color[4] { Color.White, Color.White, Color.White, Color.White },
            new Color[4] { Color.White, Color.White, Color.White, Color.White },
            new Color[4] { Color.White, Color.White, Color.White, Color.White },
        };
        for (var col = 0; col < this.Colors.GetLength(0); col++) {
            for (var row = 0; row < this.Colors.GetLength(1); row++) {
                cols[row][col] = this.Colors[col, row];
            }
        }

        return cols;
    }
    // // Equevivalents to `Heuristics1` but see 2 steps ahead
    // public uint Heuristics2(State target) {
    //     var first_step_heuristics = this.Heuristics1(State.TARGET_STATE);
    //     var children = this.Discovery();
    //     uint min_heuristics_on_second_step = 17;
    //     foreach (var child in children) {
    //         var heuristics = child.Heuristics1(State.TARGET_STATE);
    //         if (min_heuristics_on_second_step > heuristics)
    //             min_heuristics_on_second_step = heuristics;
    //     }

    //     return first_step_heuristics + min_heuristics_on_second_step;
    // }

    private State moveRow(int row, Direction dir) {
        var rowColors = new LinkedList<Color>();
        for (var i = 0; i < 4; i++) {
            rowColors.AddLast(this.Colors[row, i]);
        }

        switch (dir) {
            case Direction.LEFT: {
                var node = rowColors.First;
                rowColors.RemoveFirst();
                rowColors.AddLast(node);
                break;
            }
            case Direction.RIGHT: {
                var node = rowColors.Last;
                rowColors.RemoveLast();
                rowColors.AddFirst(node);
                break;
            }
        }

        var colors = this.Colors.Clone() as Color[,];
        for (var i = 0; i < 4; i++) {
            colors[row, i] = rowColors.First.Value;
            rowColors.RemoveFirst();
        }

        return new State(colors, this);
    }

    private State moveCol(int col, Direction dir) {
        var colColors = new LinkedList<Color>();
        for (var i = 0; i < 4; i++) {
            colColors.AddLast(this.Colors[i, col]);
        }

        switch (dir) {
            case Direction.UP: {
                var node = colColors.First;
                colColors.RemoveFirst();
                colColors.AddLast(node);
                break;
            }
            case Direction.DOWN: {
                var node = colColors.Last;
                colColors.RemoveLast();
                colColors.AddFirst(node);
                break;
            }
        }

        var colors = this.Colors.Clone() as Color[,];
        for (var i = 0; i < 4; i++) {
            colors[i, col] = colColors.First.Value;
            colColors.RemoveFirst();
        }

        return new State(colors, this);
    }
    public override string ToString() {
        var builder = new StringBuilder(4 * 4 * 2 + 4);
        for (var row = 0; row < 4; row++) {
            for (var col = 0; col < 4; col++) {
                builder.Append(this.ColorToChar(this.Colors[row, col])).Append(',');
            }
            if (row < 3) builder.Append('\n');
        }

        return builder.ToString();
    }

    public char ColorToChar(Color? color) {
        return color switch {
            null => '_',
            Color c when c.Equals(Color.Red) => 'R',
            Color c when c.Equals(Color.Green) => 'G',
            Color c when c.Equals(Color.Yellow) => 'Y',
            Color c when c.Equals(Color.Blue) => 'B',
        };
    }

    public override bool Equals(object? obj) {
        if (obj == null || this.GetType() != obj.GetType()) return false;
        
        var state = obj as State;
        for (var row = 0; row < 4; row++) {
            for (var col = 0; col < 4; col++) {
                if (!this.Colors[row, col].Equals(state.Colors[row, col]))  return false;
            }
        }

        return true;
    }
}

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
public class AStar(State start): ISearch {
    public List<(State state, uint val)> OpenNodes = new(
        new (State, uint)[] { 
                (start, start.TheMostFoolishnessHeuristics(State.TARGET_STATE)),
            }
        );
    public HashSet<(State state, uint val)> CloseNodes = new();

    public SearchInfo info = new();

    public List<State>? Search() {
        var iter = 0;
        while (this.OpenNodes.Count > 0) {

            // Console.WriteLine("Sorted list:");
            // foreach (var (node, val) in this.OpenNodes) {
            //     Console.WriteLine(node);
            //     Console.WriteLine("priority: " + val);
            //     Console.WriteLine();
            // }
            // Console.WriteLine();

            this.info.Update(
                this.OpenNodes.Count,
                this.OpenNodes.Count + this.CloseNodes.Count
            );
            var item = this.OpenNodes.First();
            // Console.WriteLine("start pos value: " + item.val);
            this.OpenNodes.RemoveAt(0);

            // break;

            // Console.WriteLine("iter: " + (++iter));
            // Console.WriteLine(item.Item1);
            // Console.WriteLine("priority: " + item.Item2);
            // Console.WriteLine();

            if (item.Item1.IsTargetState()) {
                Console.WriteLine(this.info);
                Console.WriteLine("Search finished");
                return item.Item1.GetPath();
            }
            this.CloseNodes.Add(item);

            foreach (var state in item.state.Discovery()) {
                var newVal = (uint)item.state.GetPath().Count + state.TheMostFoolishnessHeuristics(State.TARGET_STATE);

                var openNodeIndex = this.OpenNodes.FindIndex(((State, uint) item) => item.Item1.Equals(state));
                if (openNodeIndex > -1 && newVal < this.OpenNodes[openNodeIndex].val) {
                    this.OpenNodes[openNodeIndex] = (state, newVal);
                    continue;
                }
                
                var inCloseNode = this.CloseNodes.FirstOrDefault(item => item.state.Equals(state), (null, 0));
                if (inCloseNode.Item1 != null && newVal < inCloseNode.Item2) {
                    this.CloseNodes.Remove(inCloseNode);
                    this.OpenNodes.Add((state, newVal));
                    continue;
                }

                this.OpenNodes.Add((state, newVal));
            }

            // Console.WriteLine("before deleting");
            // foreach (var it in this.CloseNodes) {
            //     Console.WriteLine(it);
            // }
            // this.CloseNodes.Remove((start, start.Heuristics1(State.TARGET_STATE)));
            // Console.WriteLine("after deleting");
            // foreach (var it in this.CloseNodes) {
            //     Console.WriteLine(it);
            // }

            this._sortOpenNodes();

            // Console.WriteLine("list after update:");
            // foreach (var (node, val) in this.OpenNodes) {
            //     Console.WriteLine(node);
            //     Console.WriteLine("priority: " + val);
            //     Console.WriteLine();
            // }
            // Console.WriteLine();

            // Console.WriteLine("set:");
            // foreach (var (node, val) in this.CloseNodes) {
            //     Console.WriteLine(node);
            //     Console.WriteLine("priority: " + val);
            //     Console.WriteLine();
            // }

            // Console.ReadLine();
            
            // if (iter == 3) break;
        }

        Console.WriteLine("Search finished");
        return null;
    }

    private void _sortOpenNodes() {
        this.OpenNodes.Sort(comparison: (first, second) => (int)first.Item2 - (int)second.Item2 );
    }
}