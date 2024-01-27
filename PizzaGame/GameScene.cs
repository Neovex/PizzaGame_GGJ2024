using BlackCoat;
using BlackCoat.Entities;
using BlackCoat.Entities.Shapes;
using SFML;
using SFML.System;
using SFML.Graphics;

namespace PizzaGame
{
    internal class GameScene : Scene
    {
        private Vector2u _TileSize;
        private Vector2u _GridSize;
        private readonly Vector2f _I = new Vector2f(1, 0.5f);
        private readonly Vector2f _J = new Vector2f(-1, 0.5f);
        private Graphic[,] _Grid;

        public GameScene(Core core) : base(core, "PizzaTime", "Assets")
        {
        }

        protected override bool Load()
        {
            _Core.ClearColor = new Color(75, 75, 75);

            _GridSize = new Vector2u(10, 10);
            var tileTex = TextureLoader.Load("tile");
            _TileSize = tileTex.Size;

            _Grid = new Graphic[_GridSize.X, _GridSize.Y];
            for (int x = 0; x < _GridSize.X; x++)
            {
                for (int y = 0; y < _GridSize.Y; y++)
                {
                    Graphic tile = new Graphic(_Core, tileTex);
                    Layer_Game.Add(tile);
                    _Grid[x, y] = tile;
                }
            }
            UpdateGrid();

            Input.KeyPressed += k => UpdateGrid();

            return true;
        }

        private void UpdateGrid()
        {
            for (int x = 0; x < _GridSize.X; x++)
            {
                for (int y = 0; y < _GridSize.Y; y++)
                {
                    _Grid[x, y].Position = GridToPos(x, y);
                }
            }
        }

        protected override void Update(float deltaT)
        {

        }


        private Vector2i PosToGrid(Vector2f pos)
        {
            return pos.ToVector2i();
        }
        private Vector2f GridToPos(int x, int y)
        {
            float xMod = .5f;
            float yMod = 1f;

            return new(
                   x * _I.X * xMod * _TileSize.X + y * _J.X * xMod * _TileSize.X,
                   x * _I.Y * yMod * _TileSize.Y + y * _J.Y * yMod * _TileSize.Y);
        }

        protected override void Destroy() { }
    }
}
