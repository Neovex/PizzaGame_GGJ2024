using System;
using System.Linq;
using SFML.System;
using SFML.Graphics;
using BlackCoat;
using BlackCoat.Entities;

namespace PizzaGame
{
    internal class GameScene : Scene
    {
        //private static readonly Vector2f _TileSize = new Vector2f(480, 270);
        private static readonly Vector2f _TileSize = new Vector2f(464, 217);
        private Vector2u _GridSize;
        private readonly Vector2f _I = new Vector2f(1, 0.5f);
        private readonly Vector2f _J = new Vector2f(-1, 0.5f);
        private Graphic[,] _Grid;
        private TextItem _DebugTxt;

        public float XMod { get; set; } = 0.5f;
        public float YMod { get; set; } = 1f;
        public float MapScale { get; set; } = 0.15f;
        public Vector2f MapOffset { get; set; } = new Vector2f(150, 0);

        public GameScene(Core core) : base(core, "PizzaTime", "Assets")
        {
            core.Debug = true;
        }

        protected override bool Load()
        {
            _Core.ClearColor = new Color(75, 75, 75);

            MapOffset = new Vector2f(_Core.DeviceSize.X / 2, 50);
            LoadGrid(Layer_Game);

            Input.KeyPressed += k => UpdateGrid();

            _DebugTxt = new TextItem(_Core);
            _DebugTxt.Position = new Vector2f(10, 150);
            Layer_Overlay.Add(_DebugTxt);

            OpenInspector();
            return true;
        }

        protected override void Update(float deltaT)
        {
            var index = PosToGrid(Input.MousePosition - MapOffset);
            _DebugTxt.Text = index.ToString();
            foreach (var g in _Grid) g.Alpha = 1;
            if (index.X < 0) index.X = 0;
            if (index.Y < 0) index.Y = 0;
            if (index.X >= _GridSize.X) index.X = (int)_GridSize.X - 1;
            if (index.Y >= _GridSize.Y) index.Y = (int)_GridSize.Y - 1;
            _Grid[index.X, index.Y].Alpha = 0.5f;
        }

        private void LoadGrid(Container parent)
        {
            _GridSize = new Vector2u(8, 5);
            var textures = Enumerable.Range(0, 9).Select(i => TextureLoader.Load(i.ToString())).ToArray();
            Texture GetTexFor(int x, int y)
            {
                if (x == 0)
                {
                    if (y == 0) return textures[1];
                    if (y == _GridSize.Y-1) return textures[6];
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
            UpdateGrid();
        }


        private void UpdateGrid()
        {
            for (int x = 0; x < _GridSize.X; x++)
            {
                for (int y = 0; y < _GridSize.Y; y++)
                {
                    _Grid[x, y].Position = GridToPos(x, y);
                    _Grid[x, y].Scale = new Vector2f(MapScale, MapScale);
                }
            }
        }


        private Vector2i PosToGrid(Vector2f pos)
        {
            pos -= new Vector2f(_TileSize.X*XMod,0) * MapScale;
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
                   x * a + y * b + MapOffset.X,
                   x * c + y * d + MapOffset.Y);
        }

        protected override void Destroy() { }
    }
}
