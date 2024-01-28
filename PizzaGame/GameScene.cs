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
using SFML.Audio;
using BlackCoat.AssetHandling;
using System.Reflection;

namespace PizzaGame
{
    internal class GameScene : Scene
    {
        private static readonly Vector2f _TileSize = new Vector2f(464, 217);
        private readonly Vector2f _AnimationTargetPos = new Vector2f(885,378);
        private Vector2u _GridSize;
        private readonly Vector2f _I = new Vector2f(1, 0.5f);
        private readonly Vector2f _J = new Vector2f(-1, 0.5f);
        private PizzaPiece[,] _Grid = new PizzaPiece[0, 0];
        private FrameAnimation _Mouse;
        private Vector2f _Direction = new Vector2f();
        private float _Speed = 100;
        private FloatRect _GridBounds;
        private Polygon _GameField;
        private TextureLoader _SalamiLoader;
        private TextureLoader _OlivLoader;
        private TextureLoader _TomatoLoader;
        private TextureLoader _RoboLoader;
        private FrameAnimation _BGIdle;
        private TextureLoader _LidClosedLoader;
        private FrameAnimation _BGLid;
        private TextureLoader _LidOpenLoader;
        private FrameAnimation _BGLidOpen;
        private float _GoodieTime = 2, _GoodieReset = 2;
        private bool _GoodyIsSpwaning = false;
        private int _FlightCount = 5;

        public float XMod { get; set; } = 0.5f;
        public float YMod { get; set; } = 1f;
        public float MapScale { get; set; } = 0.7f;
        public Vector2f MapOffset { get; set; } = new Vector2f(150, 0);

        private Music _BgMusic;
        private SfxManager _SfxMan;
        private string[] _SfxFiles = new[] { "sfx_button_backward", "sfx_button_forward", "sfx_button_hover", "sfx_food_hiss", "sfx_food_impact", "sfx_mouse_eat-001", "sfx_mouse_eat-002", "sfx_mouse_eat-003" };
        private Rectangle _DebugMarker;

        public View ViewTest { get; private set; }

        public GameScene(Core core) : base(core, "PizzaTime", "Assets")
        { }

        protected override bool Load()
        {
            // Music
            _BgMusic = MusicLoader.Load("msc_ingame_loop");
            _BgMusic.Volume = Program.MUSIC_VOLUME;
            _BgMusic.Loop = true;
#if !DEBUG
            _BgMusic.Play();
#endif

            // SFX
            _SfxMan = new SfxManager(SfxLoader, () => Program.SFX_VOLUME);
            foreach (var sfxFile in _SfxFiles) _SfxMan.AddToLibrary(sfxFile, 2);
            //_Core.AnimationManager.Wait(4, () => _SfxMan.Play(_SfxFiles[3]));


            //ViewTest   
            var defSize = new Vector2f(1920, 1080);
            ViewTest = new View(defSize / 2, defSize);
            ViewTest.Size = _Core.DeviceSize;
            ViewTest.Center = _Core.DeviceSize / 2;
            //Layer_Background.View = ViewTest;
            //Layer_Game.View = ViewTest;

            // Background
            Layer_Background.Add(new Graphic(_Core, TextureLoader.Load("BG")));

#if !DEBUG
            _RoboLoader = new TextureLoader("Assets\\Idle_Robo");
            _BGIdle = new FrameAnimation(_Core, 1f / 60, Enumerable.Range(0, 51).Select(i => _RoboLoader.Load(i.ToString("D4"))).ToArray())
            {
                Position = new Vector2f(650, -50),
                Scale = new Vector2f(0.9f, 0.9f)
            };
            Layer_Background.Add(_BGIdle);

            _LidClosedLoader = new TextureLoader("Assets\\KlappeIdle");
            _BGLid = new FrameAnimation(_Core, 1f / 60, Enumerable.Range(0, 51).Select(i => _LidClosedLoader.Load(i.ToString("D4"))).ToArray())
            {
                Position = _BGIdle.Position,
                Scale = _BGIdle.Scale
            };
            Layer_Background.Add(_BGLid);

            _LidOpenLoader = new TextureLoader("Assets\\KlappeAUF");
            _BGLidOpen = new FrameAnimation(_Core, 1f / 60, Enumerable.Range(0, 51).Select(i => _LidOpenLoader.Load(i.ToString("D4"))).ToArray())
            {
                Position = _BGIdle.Position,
                Scale = _BGIdle.Scale,
                Visible = false
            };
            Layer_Background.Add(_BGLidOpen);
#endif


            // Game Field
            _GridSize = new Vector2u(3, 3);
            LoadGrid(Layer_Game);
            UpdateGrid();

            // Player
            _Mouse = new FrameAnimation(_Core, .1f, Enumerable.Range(0, 4).Select(i => TextureLoader.Load("m" + i)).ToArray());
            _Mouse.Paused = true;
            _Mouse.Origin = TextureLoader.Load("m0").Size.ToVector2f() / 2;
            _Mouse.Position = GridToPos(2, 2) + MapOffset;
            _Mouse.Scale = new Vector2f(MapScale, MapScale) * .8f;
            Layer_Game.Add(_Mouse);

            Input.KeyPressed += k =>
            {
                if (k != Keyboard.Key.Space) return;
                foreach (var anim in Layer_Overlay.GetAll<FrameAnimation>())
                {
                    if (anim.Position.DistanceBetween(_DebugMarker.Position) < 40)
                    {
                        anim.Parent.Remove(anim);
                        _SfxMan.Play("sfx_mouse_eat-00" + _Core.Random.Next(1, 4));
                    }
                }
            };


            // Pickups
            _SalamiLoader = new TextureLoader("Assets\\Salami");
            _OlivLoader = new TextureLoader("Assets\\Olive");
            _TomatoLoader = new TextureLoader("Assets\\Tomate");

            // Temp
            Input.KeyPressed += k => UpdateGrid();
            Input.MouseButtonPressed += m => Trace.WriteLine(Input.MousePosition);

            _DebugMarker = new Rectangle(_Core, new Vector2f(5, 8), Color.Blue);
            //Layer_Overlay.Add(_DebugMarker);

            //OpenInspector();
            return true;
        }

        protected override void Update(float deltaT)
        {
            //Input
            CheckInput();
            _DebugMarker.Position = _Mouse.Position + _Direction * 75;
            var pos  = _Mouse.Position + _Direction * _Speed * deltaT;
            var index = PosToGrid(pos - MapOffset);
            index = new(Math.Clamp(index.X, 0, (int)_GridSize.X - 1),
                        Math.Clamp(index.Y, 0, (int)_GridSize.Y - 1));
            var piece = _Grid[index.X, index.Y];
            if (_GameField.CollidesWith(pos) && !piece.GoneFlying)
            {
                _Mouse.Position = pos;
            }

            // Goodie Spawn
            _GoodieTime -= deltaT;
            if (_GoodieTime < 0 && !_GoodyIsSpwaning)
            {
                _GoodieTime = _GoodieReset;
                _GoodieReset *= 0.9f;
                SpawnGoodie();
            }
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

            _Grid = new PizzaPiece[_GridSize.X, _GridSize.Y];
            for (int x = 0; x < _GridSize.X; x++)
            {
                for (int y = 0; y < _GridSize.Y; y++)
                {
                    var tile = new PizzaPiece(_Core, GetTexFor(x, y), x, y);
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
            var yOff = _Core.DeviceSize.Y * 0.65f - height / 2;
            _GridBounds = new FloatRect(_Core.DeviceSize.X / 2 - width / 2, yOff, width, height);
            MapOffset = new Vector2f(_Core.DeviceSize.X / 2 - _TileSize.X * MapScale / 2, yOff);

            for (int x = 0; x < _GridSize.X; x++)
            {
                for (int y = 0; y < _GridSize.Y; y++)
                {
                    _Grid[x, y].Position = GridToPos(x, y) + MapOffset;
                    _Grid[x, y].Scale = new Vector2f(MapScale, MapScale);
                }
            }

            _GameField = new Polygon(_Core, new Vector2f[] {
                    new(_GridBounds.Left + _GridBounds.Width / 2, _GridBounds.Top),
                    new(_GridBounds.Left + _GridBounds.Width, _GridBounds.Top + _GridBounds.Height / 2),
                    new(_GridBounds.Left + _GridBounds.Width  /2, _GridBounds.Top + _GridBounds.Height),
                    new(_GridBounds.Left, _GridBounds.Top + _GridBounds.Height / 2),
                }, null, Color.Magenta)
            { OutlineThickness = 2 };
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

        private FrameAnimation GetRandomGooodie()
        {
            int min = 0, max = 0;
            TextureLoader loader = TextureLoader;
            Vector2f origin = default;
            switch (_Core.Random.Next(3))
            {
                case 0:
                    min = 40;
                    max = 43;
                    loader = _SalamiLoader;
                    origin = new Vector2f(75, 53);
                break;
                case 1:
                    min = 21;
                    max = 60-min;
                    loader = _OlivLoader;
                    origin = new Vector2f(53, 40);
                    break;
                case 2:
                    min = 28;
                    max = 51-min;
                    loader = _TomatoLoader;
                    origin = new Vector2f(53, 28);
                    break;
            }
            return new FrameAnimation(_Core, 1f / 60, Enumerable.Range(min, max).
                                                      Select(i => loader.Load(i.ToString("D4"))).
                                                      ToArray())
            {
                Loop = false,
                Paused = true,
                Scale = new Vector2f(MapScale, MapScale),
                Origin = origin
            };
        }
        private void SpawnGoodie()
        {
            _GoodyIsSpwaning = true;
            Vector2f pos;
            Vector2i index;
            PizzaPiece piece;
            do
            {
                pos = _Core.Random.NextVector(_GridBounds);
                index = PosToGrid(pos - MapOffset);
                index = new(Math.Clamp(index.X, 0, (int)_GridSize.X - 1),
                            Math.Clamp(index.Y, 0, (int)_GridSize.Y - 1));
                
                piece = _Grid[index.X, index.Y];
            }
            while (!_GameField.CollidesWith(pos) || piece.GoneFlying);

            var goodie = GetRandomGooodie();
            Layer_Overlay.Add(goodie);
            _Core.AnimationManager.Run(-100, (float)Math.Round(pos.Y), 1,
                v => goodie.Position = new Vector2f(pos.X, v),
                () =>
                {
                    _GoodyIsSpwaning = false;
                    goodie.Paused = false;
                    _SfxMan.Play("sfx_food_impact");
                    if (!piece.GoneFlying) CheckForFlight(piece);
                });
        }

        private void CheckForFlight(PizzaPiece piece)
        {
            if (piece.GoodyCount++ >= _FlightCount)
            {
                piece.GoneFlying = true;
                Layer_Game.Add(piece); // move to to top

                foreach (var goody in Layer_Overlay.GetAll<FrameAnimation>())
                {
                    var index = PosToGrid(goody.Position-MapOffset);
                    if (index == piece.Index)
                    {
                        goody.Scale = new Vector2f(1, 1);
                        piece.Add(goody);
                        goody.Position = goody.Position.ToLocal(piece.Position).DivideBy(piece.Scale);
                    }
                }

                _Core.AnimationManager.Run(piece.Position.Y, piece.Position.Y - 100, 1.5f,
                    v => piece.Position = new Vector2f(piece.Position.X, v),
                    () =>
                    {
                        _Core.AnimationManager.Run(piece.Position.X, _AnimationTargetPos.X, 1.5f,
                        v => piece.Position = new Vector2f(v, piece.Position.Y), null, InterpolationType.InExpo);

                        _Core.AnimationManager.Run(piece.Position.Y, _AnimationTargetPos.Y, 1.5f,
                        v => piece.Position = new Vector2f(piece.Position.X, v), 
                        ()=>
                        {
                            Layer_Game.Remove(piece);
                            _SfxMan.Play("sfx_food_hiss");
                        },
                        InterpolationType.InExpo);
                    },
                    InterpolationType.OutElastic);
            }
        }

        protected override void Destroy()
        {
            _SalamiLoader.Dispose();
            _OlivLoader.Dispose();
            _TomatoLoader.Dispose();
            _RoboLoader?.Dispose();
            _LidOpenLoader?.Dispose();
            _LidClosedLoader?.Dispose();
        }
    }
}