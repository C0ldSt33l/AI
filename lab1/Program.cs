﻿using System.Numerics;
using System.Security.Cryptography;
using Raylib_cs;
using rl = Raylib_cs.Raylib;

namespace Game;

// Available colors: `Red`, `Green`, `Yellow`, `Blue`
public class Circle(Color Color) {
    public static readonly float RADIUS = 50;
    public  Color Color = Color;
    public Vector2 Pos = Vector2.Zero;

    public void Draw() {
        rl.DrawCircleV(
            this.Pos,
            50,
            Color
        );
    }
}

public class Cell(Vector2 Pos) {
    public static readonly Vector2 SIZE = new Vector2(100);
    public Rectangle Rect = new Rectangle(Pos, SIZE);

    public void AttachCircle(Circle circle) {
        var pos = this.Rect.Position + this.Rect.Size / 2;
        circle.Pos = pos;
    }

    public void Draw() {
        rl.DrawRectangleRec(this.Rect, Color.Beige);
        rl.DrawRectangleLinesEx(this.Rect, 10, Color.Black);
    }
}

public enum Direction {
    UP,
    DOWN,
    LEFT,
    RIGHT,
}

public class Button {
    private static readonly Texture2D _texture;

    public Rectangle Rect;
    public float Rotation;
    public Action Action;

    private Vector2 _offset_pos;

    static Button() {
        var image = rl.LoadImage("arrow.png");
        // rl.ImageCrop(ref image, new Rectangle(Vector2.Zero, new Vector2(100)));
        rl.ImageResize(ref image, 100, 100);
        _texture = rl.LoadTextureFromImage(image);
    }

    public Button(Vector2 Pos, float Rotation, Action action) {
        this.Rect = new Rectangle(Pos, new Vector2(100));
        this.Rotation = Rotation;

        Vector2 offset = Rotation switch {
            0   => Vector2.Zero,
            90  => new Vector2(1, 0),
            180 => new Vector2(1, 1),
            -90 => new Vector2(0, 1),
        } * 100;
        this._offset_pos = this.Rect.Position + offset;

        this.Action = action;
    }

    public void Draw() {
        rl.DrawTextureEx(_texture, this._offset_pos, this.Rotation, 1, Color.White);
    }
}

public class UIButton {
    public static readonly int FONT_SIZE = 30;
    public string Text;
    public Vector2 Pos {
        set {
            this.Rect.Position = value;
            this._textPos = value + _textOffset;
        }
        get => this.Rect.Position;
    }
    public Rectangle Rect;
    public Action Action;

    private readonly Vector2 _textOffset;
    private Vector2 _textPos;

    public UIButton(string text, Vector2 size, Vector2 textPosOffset, Action action) {
        this._textOffset = textPosOffset;
        this.Text = text;
        this.Rect = new(Vector2.Zero, size);
        this.Pos = Vector2.Zero;

        this.Action = action;
    }

    public void Draw() {
        rl.DrawRectangleRec(this.Rect, Color.White);
        rl.DrawRectangleLinesEx(this.Rect, 10, Color.Black);
        rl.DrawText(this.Text, (int)this._textPos.X, (int)this._textPos.Y, FONT_SIZE, Color.Black);
    }
}

public class Game {
    private readonly Vector2 WINDOW_SIZE = new Vector2(1080, 840);
    private readonly string TITLE = "Move balls";
    public Color Background = Color.Gray;
    private Circle[,] _circles;
    private Cell[,] _cells;
    private Button[,] _moveButtons;

    private UIButton[] _searchButtons;
    private UIButton[] _actionButtons;

    private State _startState;

    // 0 - Width
    // 1 - Depth
    // 2 - Depth with limitation
    // 3 - BiDirectional
    // 4 - A*

    private List<State>? _pathToWin = null;
    private int _curState = 0;

    public Game() {
        rl.InitWindow((int)this.WINDOW_SIZE.X, (int)this.WINDOW_SIZE.Y, this.TITLE);
        rl.SetTargetFPS(60);

        this._normalInit();
        // this._threeStepsFromWinningInit();
    }

    public void Update() {
        while(!rl.WindowShouldClose()) {
            rl.BeginDrawing();

                this._update();
                this.Draw();

            rl.EndDrawing();
        }

        rl.CloseWindow();
    }

    public void Search(string name, ISearch searcher) {
    Console.WriteLine(name);
    var path = searcher.Search();

    if (path == null) {
        Console.WriteLine("Path is not found");
        this._pathToWin = null;
        this._curState = 0;
    }
    else {
        Console.WriteLine("Path length: " + (path.Count - 1));
        this._pathToWin = path;
        this._curState = 0;
    }
}

    public void PlayNextState() {
        if (this._pathToWin == null || this._curState == this._pathToWin.Count - 1) return;
        this._curState++;
        this.ChangeColors(this._pathToWin[this._curState].Colors);
    }

    public void PlayPrevState() {
        if (this._pathToWin == null || this._curState == 0) return;
        this._curState--;
        this.ChangeColors(this._pathToWin[this._curState].Colors);
    }

    public void Draw() {
        rl.ClearBackground(this.Background);

        foreach (var cell in this._cells) {
            cell.Draw();
        }
        foreach (var circle in this._circles) {
            circle.Draw();
        }
        foreach (var button in this._moveButtons) {
            button.Draw();
        }
        foreach (var button in this._searchButtons) {
            button.Draw();
        }
        foreach (var button in this._actionButtons) {
            button.Draw();
        }
    }

    private void _normalInit() {
        this._setCells();
        this._setCirclesInWinState();
        this._setButtons();

        this._addSomeChaous(5);
    }

    private void _threeStepsFromWinningInit() {
        this._setCells();
        this._setCirclesInWinState();
        this._setButtons();

        this.MoveCol(3, Direction.UP);
        this.MoveCol(3, Direction.UP);
        this.MoveCol(3, Direction.UP);
        // this.MoveRow(3, Direction.LEFT);

        this._startState = new State(this._circles);
        this._curState = 0;
    }

    private void _addSomeChaous(int times) {
        var rand = () => RandomNumberGenerator.GetInt32(0, 4);
        for (; times > 0; times--) {
            int row = rand(), col = rand();
            this._moveButtons[row, col].Action();
        }

        this._startState = new State(this._circles);
        this._curState = 0;
    }

    private void _setCells() {
        var startPos = new Vector2(1080 / 2 - Cell.SIZE.X * 4 / 2, 840 / 2 - Cell.SIZE.Y * 4 / 2);
        var curPos = startPos;

        this._cells = new Cell[4, 4];
        for (var y = 0; y < this._cells.GetLength(0); y++) {
            for (var x = 0; x < this._cells.GetLength(1); x++) {
                var cell = new Cell(curPos);
                this._cells[y, x] = cell;
                curPos.X += Cell.SIZE.X;
            }
            curPos = new Vector2(startPos.X, curPos.Y + Cell.SIZE.Y);
        }
    }
    
    private void _setCirclesInWinState() {
        var colors = new Color[] { Color.Red, Color.Green, Color.Yellow, Color.Blue };
        this._circles = new Circle[4, 4];
        for (var row = 0; row < this._cells.GetLength(0); row++) {
            for (var col = 0; col < this._cells.GetLength(1); col++) {
                var circle = new Circle(colors[row]);
                this._cells[row, col].AttachCircle(circle);
                this._circles[row, col] = circle;
            }
        }
    }

    private void _setButtons() {
        this._moveButtons = new Button[4, 4];
        var startPos = new Vector2(1080 / 2 - Cell.SIZE.X * 4 / 2, 840 / 2 - Cell.SIZE.Y * 4 / 2);
        for (var i = 0; i < 4; i++) {
            var index = i;
            // hor up buttons
            this._moveButtons[0, i]= new Button(startPos + new Vector2(Cell.SIZE.X * i, -Cell.SIZE.Y), 0, () => this.MoveCol(index, Direction.UP));
            // hor down buttons
            this._moveButtons[1, i]= new Button(startPos + new Vector2(Cell.SIZE.X * i, Cell.SIZE.Y * 4), 180, () => this.MoveCol(index, Direction.DOWN));
            // ver left buttons
            this._moveButtons[2, i]= new Button(startPos + new Vector2(-Cell.SIZE.X, Cell.SIZE.Y * i), -90, () => this.MoveRow(index, Direction.LEFT));
            // ver right buttons
            this._moveButtons[3, i]= new Button(startPos + new Vector2(Cell.SIZE.X * 4, Cell.SIZE.Y * i), 90, () => this.MoveRow(index, Direction.RIGHT));
        }

        this._searchButtons = new UIButton[] {
            new UIButton("Width", new Vector2(130, 75), new Vector2(20, 20), () => this.Search("Width search", new WidthFirstSearch(this._startState))),
            new UIButton("Depth", new Vector2(130, 75), new Vector2(20, 20), () => this.Search("Depth search", new DepthFirstSearch(this._startState))),
            new UIButton("Depth with limit", new Vector2(260, 75), new Vector2(20, 20), () => this.Search("Depth with limitation search", new DepthLimitedSearch(this._startState))),
            new UIButton("BiDirectional", new Vector2(230, 75), new Vector2(20, 20), () => this.Search("BiDirectional search", new BiDirectionalSearch(this._startState))),
            new UIButton("A*", new Vector2(100, 75), new Vector2(35, 20), () => this.Search("A* search", new AStar(this._startState))),
        };
        for (var i = 1; i < this._searchButtons.Length; i++) {
            var prevButton = this._searchButtons[i - 1];
            this._searchButtons[i].Pos = new Vector2(prevButton.Rect.Position.X + prevButton.Rect.Size.X + 50, 0);
        }

        this._actionButtons = new UIButton[] {
            new UIButton("Prev", new Vector2(150, 75), new Vector2(20, 20), this.PlayPrevState),
            new UIButton("Next", new Vector2(150, 75), new Vector2(20, 20), this.PlayNextState),
            new UIButton("Shuffle", new Vector2(150, 75), new Vector2(20, 20), () => this._addSomeChaous(3)),
        };
        for (var i = 0; i < this._actionButtons.Length; i++) {
            this._actionButtons[i].Pos = new Vector2(0, (75 + 50) * (i + 1));
        }

        // this._actionButtons[0].Pos = new Vector2(0, 500);
    }

    private void _update() {
        if (!rl.IsMouseButtonPressed(MouseButton.Left)) return;

        var mousePos = rl.GetMousePosition();
        foreach (var button in this._moveButtons) {
            if (rl.CheckCollisionPointRec(mousePos, button.Rect)) {
                button.Action();
                return;
            }
        }

        var otherButtons = this._actionButtons.Concat(this._searchButtons);
        foreach (var button in otherButtons) {
            if (rl.CheckCollisionPointRec(mousePos, button.Rect)) {
                button.Action();
                return;
            }
        }
    }

    public void MoveRow(int Row, Direction Dir) {
        var circles = new LinkedList<Color>(new Color[4] {
            this._circles[Row, 0].Color,
            this._circles[Row, 1].Color,
            this._circles[Row, 2].Color,
            this._circles[Row, 3].Color,
        });

        switch (Dir) {
            case Direction.RIGHT: {
                var node = circles.Last;
                circles.RemoveLast();
                circles.AddFirst(node);
                break;
            }

            case Direction.LEFT: {
                var node = circles.First;
                circles.RemoveFirst();
                circles.AddLast(node);
                break;
            }

            default:
                throw new Exception("wrong dir");
        }

        for (var i = 0; i < 4; i++) {
            this._circles[Row, i].Color = circles.First.Value;
            circles.RemoveFirst();
        }
    }

    public void MoveCol(int Col, Direction Dir) {
        var colors = new LinkedList<Color>(
        new Color[4] {
            this._circles[0, Col].Color,
            this._circles[1, Col].Color,
            this._circles[2, Col].Color,
            this._circles[3, Col].Color,
        });

        switch (Dir) {
            case Direction.DOWN: {
                var node = colors.Last;
                colors.RemoveLast();
                colors.AddFirst(node);
                break;
            }

            case Direction.UP: {
                var node = colors.First;
                colors.RemoveFirst();
                colors.AddLast(node);
                break;
            }

            default:
                throw new Exception("wrong dir");
        }

        for (var i = 0; i < 4; i++) {
            this._circles[i, Col].Color = colors.First.Value;
            colors.RemoveFirst();
        }
    }

    public void ChangeColors(Color[,] colors) {
        for (var row = 0; row < this._circles.GetLength(0); row++) {
            for (var col = 0; col < this._circles.GetLength(1); col++) {
                this._circles[row, col].Color = colors[row, col];
            }
        }
    }
}

class Program {
    public static void Main(String[] args) {
        var game = new Game();
        game.Update();
    }
}