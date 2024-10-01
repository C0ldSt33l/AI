using System.Collections;
using System.Data;
using System.Text;
using Raylib_cs;

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
    }

    public List<State> Discovery() {
        var states = new List<State>();
        for (var i = 0; i < 4; i++) {
            states.Add(this.moveRow(i, Direction.LEFT));
            states.Add(this.moveRow(i, Direction.RIGHT));
            states.Add(this.moveCol(i, Direction.UP));
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

    public Color?[,] GetDiff(State other) {
        var diff = this.Colors.Clone() as Color?[,];
        for (var row = 0; row < this.Colors.GetLength(0); row++) {
            for (var col = 0; col < this.Colors.GetLength(1); col++) {
                if (!this.Colors[row, col].Equals(other.Colors[row, col]))
                    diff[row, col] = null;
                else
                    diff[row, col] = this.Colors[row, col];
            }
        }

        return diff;
    }

    public Color?[,] GetDiffFromTargetState() {
        return this.GetDiff(TARGET_STATE);
    }

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
            builder.Append('\n');
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

public class WidthFirstSearch(State StartState): ISearch {
    public Queue<State> OpenNodes = new(new State[] { StartState });
    public HashSet<State> CloseNodes = new();

    public SearchInfo info = new();

    public List<State>? Search() {
        while (this.OpenNodes.Count > 0) {
            this.info.Update(
                this.OpenNodes.Count(), 
                this.OpenNodes.Count() + this.CloseNodes.Count()
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
                this.OpenNodes.Count(), 
                this.OpenNodes.Count() + this.CloseNodes.Count()
            );
 
            var node = this.OpenNodes.Pop();

            if (node.IsTargetState()) {
                Console.WriteLine("Search finished");
                return node.GetPath();
            }
            this.CloseNodes.Add(node);

            foreach (var state in node.Discovery()) {
                if (this.OpenNodes.Contains(state)) continue;
                if (this.CloseNodes.Contains(state)) continue;
                this.OpenNodes.Push(state);
            }
        }

        Console.WriteLine("Search finished");
        return null;
    }
}

public class BiDirectionalSearch(State start): ISearch {
    public Queue<State> startOpenNodes = new(new State[] { start });
    public HashSet<State> startCloseNodes = new();
    public Queue<State> endOpenNodes = new(new State[] { State.TARGET_STATE });
    public HashSet<State> endCloseNodes = new();

    public List<State>? Search() {
        while(this.startCloseNodes.Count() > 0 || this.endCloseNodes.Count() > 0) {
            var startNode = this.startOpenNodes.Dequeue();
            var endNode = this.endOpenNodes.Dequeue();

            if (startNode.Equals(endNode)) {
                var startPath = startNode.GetPath();
                var endPath = endNode.GetPath();
                startPath.Reverse();
                endPath.RemoveAt(0);

                return startPath.Concat(endPath) as List<State>;
            }
        }

        return null;
    }
}