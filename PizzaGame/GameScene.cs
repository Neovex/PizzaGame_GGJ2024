using System;
using System.Linq;
using System.Diagnostics;
using SFML.System;
using SFML.Window;
using SFML.Graphics;
using BlackCoat;
using BlackCoat.Entities;
using BlackCoat.Animation;
using BlackCoat.Entities.Shapes;
using BlackCoat.Entities.Animation;

namespace PizzaGame
{
    internal class GameScene : Scene
    {
        private static readonly Vector2f _TileSize = new Vector2f(464, 217);
        private Vector2u _GridSize;
        private readonly Vector2f _I = new Vector2f(1, 0.5f);
        private readonly Vector2f _J = new Vector2f(-1, 0.5f);
        private Graphic[,] _Grid = new Graphic[0, 0];
        private FrameAnimation _Mouse;
        private Vector2f _Direction = new Vector2f();
        private float _Speed = 100;

        public float XMod { get; set; } = 0.5f;
        public float YMod { get; set; } = 1f;
        public float MapScale { get; set; } = 0.35f;
        public Vector2f MapOffset { get; set; } = new Vector2f(150, 0);


        public GameScene(Core core) : base(core, "PizzaTime", "Assets")
        { }

        protected override bool Load()
        {
            // Background
            var part = 0.60f;
            Layer_Background.Add(
                new Rectangle(_Core, new Vector2f(_Core.DeviceSize.X, _Core.DeviceSize.Y * part), new Color(0xb05a1300))
                { Position = new Vector2f(0, _Core.DeviceSize.Y * (1 - part)) });
            Layer_Background.Add(
                new Rectangle(_Core, new Vector2f(_Core.DeviceSize.X * 0.2f, _Core.DeviceSize.Y * (1 - part)), Color.Cyan)
                { Position = new Vector2f(_Core.DeviceSize.X * 0.4f, 0) });

            // Game Field
            _GridSize = new Vector2u(5, 5);
            LoadGrid(Layer_Game);
            UpdateGrid();

            // Player
            _Mouse = new FrameAnimation(_Core, .1f, Enumerable.Range(0, 4).Select(i => TextureLoader.Load("m" + i)).ToArray());
            _Mouse.Paused = true;
            _Mouse.Origin = TextureLoader.Load("m0").Size.ToVector2f() / 2;
            _Mouse.Position = GridToPos(2, 2) + MapOffset;
            _Mouse.Scale = new Vector2f(MapScale, MapScale) * .8f;
            Layer_Game.Add(_Mouse);

            // Temp
            Input.KeyPressed += k => UpdateGrid();
            Input.MouseButtonPressed += m => Trace.WriteLine(Input.MousePosition);

            //OpenInspector();
            return true;
        }

        protected override void Update(float deltaT)
        {
            //Input
            CheckInput();
            _Mouse.Position = _Mouse.Position + _Direction * _Speed * deltaT;

            // Debug
            var index = PosToGrid(Input.MousePosition - MapOffset);
            foreach (var g in _Grid) g.Alpha = 1;
            if (index.X < 0) index.X = 0;
            if (index.Y < 0) index.Y = 0;
            if (index.X >= _GridSize.X) index.X = (int)_GridSize.X - 1;
            if (index.Y >= _GridSize.Y) index.Y = (int)_GridSize.Y - 1;
            _Grid[index.X, index.Y].Alpha = 0.5f;
        }

        private void CheckInput()
        {
            if (Input.IsKeyDown(Keyboard.Key.W, Keyboard.Key.A) || Input.IsKeyDown(Keyboard.Key.Up, Keyboard.Key.Left))
            {
                _Direction = new Vector2f(-1, -1);
                _Mouse.CurrentFrame = 0;
            }
            else if (Input.IsKeyDown(Keyboard.Key.W, Keyboard.Key.D) || Input.IsKeyDown(Keyboard.Key.Up, Keyboard.Key.Right))
            {
                _Direction = new Vector2f(1, -1);
                _Mouse.CurrentFrame = 0;
            }
            else if (Input.IsKeyDown(Keyboard.Key.S, Keyboard.Key.A) || Input.IsKeyDown(Keyboard.Key.Down, Keyboard.Key.Left))
            {
                _Direction = new Vector2f(-1, 1);
                _Mouse.CurrentFrame = 0;
            }
            else if (Input.IsKeyDown(Keyboard.Key.S, Keyboard.Key.D) || Input.IsKeyDown(Keyboard.Key.Down, Keyboard.Key.Right))
            {
                _Direction = new Vector2f(1, 1);
                _Mouse.CurrentFrame = 0;
            }
            else if (Input.IsKeyDown(Keyboard.Key.W) || Input.IsKeyDown(Keyboard.Key.Up))
            {
                _Direction = new Vector2f(0, -1);
                _Mouse.CurrentFrame = 1;
            }
            else if (Input.IsKeyDown(Keyboard.Key.A) || Input.IsKeyDown(Keyboard.Key.Left))
            {
                _Direction = new Vector2f(-1, 0);
                _Mouse.CurrentFrame = 0;
            }
            else if (Input.IsKeyDown(Keyboard.Key.S) || Input.IsKeyDown(Keyboard.Key.Down))
            {
                _Direction = new Vector2f(0, 1);
                _Mouse.CurrentFrame = 3;
            }
            else if (Input.IsKeyDown(Keyboard.Key.D) || Input.IsKeyDown(Keyboard.Key.Right))
            {
                _Direction = new Vector2f(1, 0);
                _Mouse.CurrentFrame = 2;
            }
            else _Direction = default;
        }

        private void LoadGrid(Container parent)
        {
            var textures = Enumerable.Range(0, 9).Select(i => TextureLoader.Load(i.ToString())).ToArray();
            Texture GetTexFor(int x, int y)
            {
                if (x == 0)
                {
                    if (y == 0) return textures[1];
                    if (y == _GridSize.Y - 1) return textures[6];
                    return textures[4];
                }

                if (x == _GridSize.X - 1)
                {
                    if (y == 0) return textures[3];
                    if (y == _GridSize.Y - 1) return textures[8];
                    return textures[5];
                }

                if (y == 0) return textures[2];
                if (y == _GridSize.Y - 1) return textures[7];

                return textures[0];
            }

            _Grid = new Graphic[_GridSize.X, _GridSize.Y];
            for (int x = 0; x < _GridSize.X; x++)
            {
                for (int y = 0; y < _GridSize.Y; y++)
                {
                    Graphic tile = new Graphic(_Core, GetTexFor(x, y));
                    parent.Add(tile);
                    _Grid[x, y] = tile;
                }
            }
        }


        private void UpdateGrid()
        {
            var min = GridToPos(0, (int)_GridSize.Y);
            var max = GridToPos((int)_GridSize.X, 0);
            var width = max.X - min.X;
            min = GridToPos(0, 0);
            max = GridToPos((int)_GridSize.X, (int)_GridSize.Y);
            var height = max.Y - min.Y;
            MapOffset = new Vector2f(_Core.DeviceSize.X * 0.5f - _TileSize.X * MapScale / 2, _Core.DeviceSize.Y * 0.75f - height / 2);

            for (int x = 0; x < _GridSize.X; x++)
            {
                for (int y = 0; y < _GridSize.Y; y++)
                {
                    _Grid[x, y].Position = GridToPos(x, y) + MapOffset;
                    _Grid[x, y].Scale = new Vector2f(MapScale, MapScale);
                }
            }
        }


        private Vector2i PosToGrid(Vector2f pos)
        {
            pos -= new Vector2f(_TileSize.X * XMod, 0) * MapScale;
            var size = _TileSize * MapScale;

            var a = _I.X * XMod * size.X;
            var b = _J.X * XMod * size.X;
            var c = _I.Y * YMod * size.Y;
            var d = _J.Y * YMod * size.Y;

            var det = 1 / (a * d - b * c);

            var ma = det * d;
            var mb = det * -b;
            var mc = det * -c;
            var md = det * a;

            return new Vector2f(pos.X * ma + pos.Y * mb,
                                pos.X * mc + pos.Y * md).ToVector2i();
        }
        private Vector2f GridToPos(int x, int y)
        {
            var size = _TileSize * MapScale;

            var a = _I.X * XMod * size.X;
            var b = _J.X * XMod * size.X;
            var c = _I.Y * YMod * size.Y;
            var d = _J.Y * YMod * size.Y;

            return new(
                   x * a + y * b,
                   x * c + y * d);
        }

        protected override void Destroy() { }
    }
}
