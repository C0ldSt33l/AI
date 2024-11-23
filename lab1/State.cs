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

    public uint[] GetColorPositions(Color c) {
        var pos = new uint[4] { 0, 0, 0, 0 };

        for (int count = 0, i = 0; count < 4; i++) {
            int row = i / 4, col = i % 4; 
            if (this.Colors[row, col].Equals(c)) {
                pos[count++] = (uint)(i + 1);
            }
        }

        return pos;
    }

    // Search how many colors not on its own places
    public static uint Heuristics1(State state, State target) {
        float value = 0;
        for (var row = 0; row < state.Colors.GetLength(0); row++) {
            for (var col = 0; col < state.Colors.GetLength(1); col++) {
                if (!state.Colors[row, col].Equals(target.Colors[row, col])) value++;
            }
        }

        return (uint)Math.Floor(value / 4.0f);
    }

    // Manhattan distance (Mosany)
    public static uint Heuristics2(State state, State target) {
        float value = 0;

        for (var row = 0; row < state.Colors.GetLength(0); row++) {
            for (var col = 0; col < state.Colors.GetLength(1); col++) {
                var color = state.Colors[row, col];

                if (!color.Equals(target.Colors[row, col])) {
                    for (var targetRow = 0; targetRow < TARGET_STATE.Colors.GetLength(0); targetRow++) {
                        for (var targetCol = 0; targetCol < TARGET_STATE.Colors.GetLength(1); targetCol++) {
                            if (color.Equals(TARGET_STATE.Colors[targetRow, targetCol])) {
                                uint distance = (uint)(Math.Abs(row - targetRow) + Math.Abs(col - targetCol));
                                if (distance <= 2) {
                                    value += distance;
                                }
                                else value += 1;

                                break;
                            }
                        }
                    }
                }
            }
        }

        return (uint)Math.Floor(value / 4.0f);
    }
        

    // Count how many rows and cols in right color?
    public static uint TheMostFoolishHeuristics(State state, State target) {
        uint
            rowsNotInPlace = 0,
            colsNotInPlace = 0;

        var rows = state._getRows();
        var cols = state._getCols();
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

        return (uint)Math.Floor((rowsNotInPlace + colsNotInPlace) / 2.0f);
    }

    // Extra task
    // Using subtask DB to evaluate approximated rest of path
    public static uint DBHeuristics(State state, State target) {
        uint val = 0;
        return val;
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

    public int getHashCodeByColor(Color c) {
        var hash = 0;
        var i = 0;

        foreach (var _ in this.Colors) {
            int row = i / 4, col = i % 4;
            hash += i + 1 + row + 1 + col + 1;
            i++;
        }
        return hash;
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

    public bool EqualsByColor(State other, Color c) {
        var col_count = 0;
        for (var row = 0; row < this.Colors.GetLength(0); row++) {
            for (var col = 0; col < this.Colors.GetLength(1); col++) {
                if (col_count == 4) break;
                if (this.Colors[row, col].Equals(c)) {
                    if (other.Colors[row, col].Equals(c))
                        col_count++;
                    else
                        return false;
                }
            }
        }

        return true;
    }
}