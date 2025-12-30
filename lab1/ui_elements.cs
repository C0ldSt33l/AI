using System.Numerics;
using Raylib_cs;
using rl = Raylib_cs.Raylib;

namespace Game;

// Available colors: `Red`, `Green`, `Yellow`, `Blue`
public class Circle(Color Color): IDrawable {
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

public class Cell(Vector2 Pos): IDrawable {
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

public class Button: IDrawable{
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

public class UIButton<T>: IDrawable {
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
    public T Action;

    private readonly Vector2 _textOffset;
    private Vector2 _textPos;

    public UIButton(string text, Vector2 size, Vector2 textPosOffset, T action) {
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