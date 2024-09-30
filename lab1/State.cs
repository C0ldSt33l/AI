using Raylib_cs;

namespace Game;

public class State {
    public static readonly State TARGET_STATE = new State(new char[4,4] {
        {'R', 'R', 'R', 'R'},
        {'G', 'G', 'G', 'G'},
        {'Y', 'Y', 'Y', 'Y'},
        {'B', 'B', 'B', 'B'},
    });
    public readonly Color[,] Colors = new Color[4, 4];

    public State(Circle[,] circles) {
        for (var row = 0; row < circles.GetLength(0); row++) {
            for (var col = 0; col < circles.GetLength(1); col++) {
                this.Colors[row, col] = circles[row, col].Color;
            }
        }
    }
    public State(Color[,] colors) {
        this.Colors = colors;
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
            states.Add(this.moveCol(i, Direction.UP));
            states.Add(this.moveCol(i, Direction.DOWN));
            states.Add(this.moveRow(i, Direction.LEFT));
            states.Add(this.moveRow(i, Direction.RIGHT));
        }

        return states;
    }

    public bool IsTargetState() {
        return this.Equals(TARGET_STATE);
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

        return new State(colors);
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

        return new State(colors);
    }
    public override string ToString() {
        var str = "";
        for (var row = 0; row < 4; row++) {
            for (var col = 0; col < 4; col++) {
                str += this.Colors[row, col] switch {
                    Color c when c.Equals(Color.Red) => 'R',
                    Color c when c.Equals(Color.Green) => 'G',
                    Color c when c.Equals(Color.Yellow) => 'Y',
                    Color c when c.Equals(Color.Blue) => 'B',
                } + ",";
            }
            str += "\n";
        }
        return str;
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

public class WidthFirstSearch(State StartState) {
    public List<State> OpenNodes = new() { StartState };
    public HashSet<State> CloseNodes = new();

    public State? Search() {
        while (this.OpenNodes.Count > 0) {
            var node = this.OpenNodes.First();
            this.OpenNodes.RemoveAt(0);

            if (node.IsTargetState()) return node;
            this.CloseNodes.Add(node);
            foreach (var state in node.Discovery()) {
                if (this.OpenNodes.Contains(state)) continue;
                if (this.CloseNodes.Contains(state)) continue;
                this.OpenNodes.Add(state);
            }
        }
        return null;
    }
}

public class DepthFirstSearch(State StartState) {
    public Stack<State> OpenNodes = new (new State[] { StartState });
    public HashSet<State> CloseNodes = new();

    public State? Search() {
        while (this.OpenNodes.Count > 0) {
            var node = this.OpenNodes.Pop();
            if (node.IsTargetState()) return node;
            this.CloseNodes.Add(node);
            foreach (var state in node.Discovery()) {
                if (this.OpenNodes.Contains(state)) continue;
                if (this.CloseNodes.Contains(state)) continue;
                this.OpenNodes.Push(state);
            }
        }
        return null;
    }
}