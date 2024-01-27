using BlackCoat;
using BlackCoat.Entities;
using BlackCoat.Entities.Shapes;
using SFML;
using SFML.System;
using SFML.Graphics;
using System.Security.Cryptography.Xml;
using System;
using System.Linq;

namespace PizzaGame
{
    internal class GameScene : Scene
    {
        private static readonly Vector2f _TileSize = new Vector2f(480, 270);
        private static readonly Vector2f _TexOffset = new Vector2f(_TileSize.X - 464, _TileSize.Y - 246);
        private Vector2u _GridSize;
        private readonly Vector2f _I = new Vector2f(1, 0.5f);
        private readonly Vector2f _J = new Vector2f(-1, 0.5f);
        private Graphic[,] _Grid;

        public float XMod { get; set; } = 0.5f;
        public float YMod { get; set; } = 0.885f;
        public float MapScale { get; set; } = 0.5f;
        public Vector2f MapOffset { get; set; } = new Vector2f(200, 0);

        public GameScene(Core core) : base(core, "PizzaTime", "Assets")
        {
        }

        protected override bool Load()
        {
            _Core.ClearColor = new Color(75, 75, 75);

            MapOffset = new Vector2f(_Core.DeviceSize.X / 2, 50);
            LoadGrid(Layer_Game);

            Input.KeyPressed += k => UpdateGrid();

            return true;
        }

        protected override void Update(float deltaT)
        {

        }

        private void LoadGrid(Container parent)
        {
            _GridSize = new Vector2u(3, 3);
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
            return pos.ToVector2i();
        }
        private Vector2f GridToPos(int x, int y)
        {
            var size = (_TileSize - _TexOffset) * MapScale;

            return new(
                   x * _I.X * XMod * size.X + y * _J.X * XMod * size.X + MapOffset.X,
                   x * _I.Y * YMod * size.Y + y * _J.Y * YMod * size.Y + MapOffset.Y);
        }

        protected override void Destroy() { }
    }
}
