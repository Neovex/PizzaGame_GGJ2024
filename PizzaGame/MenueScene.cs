using BlackCoat;
using BlackCoat.Entities;
using BlackCoat.Entities.Shapes;
using SFML.Audio;
using SFML.Graphics;

namespace PizzaGame
{
    internal class MenueScene : Scene
    {
        private TextItem _Start;
        private TextItem _Exit;
        private Rectangle _StartCol;
        private Rectangle _ExitCol;
        private Music _Music;

        public MenueScene(Core core) : base(core, "Menue", "Assets")
        { }


        protected override bool Load()
        {
            _Music = MusicLoader.Load("menu_loop");
            _Music.Loop = true;
            _Music.Volume = Program.MUSIC_VOLUME;
#if !DEBUG
            _Music.Play();
#endif

            Layer_Background.Add(new Graphic(_Core, TextureLoader.Load("BG")));

            var fnt = FontLoader.Load("Super Dream");

            _Start = new TextItem(_Core, "Start", 25, fnt) { Position = new(560, 650), CharacterSize=100, Color = Color.Yellow };
            _Exit = new TextItem(_Core, "Exit", 25, fnt) { Position = new(1150, 650), CharacterSize = 100, Color = Color.Yellow };
            Layer_Game.Add(_Start);
            Layer_Game.Add(_Exit);
            

            _StartCol = new Rectangle(_Core, new(280, 80), Color.Cyan) {  Position = new(560, 675) };
            _ExitCol = new Rectangle(_Core, new(200, 80), Color.Cyan) { Position = new(1150, 675) };

            var sfx = new Sound(SfxLoader.Load("sfx_button_forward"));
            sfx.Volume = Program.SFX_VOLUME;
            Input.MouseButtonPressed += m =>
            {
                if (_StartCol.CollidesWith(Input.MousePosition))
                {
                    sfx.Play();
                    _Core.SceneManager.ChangeScene(new GameScene(_Core));
                }
                if (_ExitCol.CollidesWith(Input.MousePosition))
                {
                    sfx.Play();
                    _Core.Exit("Menu Exit");
                }
            };
            return true;
        }

        protected override void Update(float deltaT)
        {
            _Start.Color = _StartCol.CollidesWith(Input.MousePosition) ? Color.Green : Color.Yellow;
            _Exit.Color = _ExitCol.CollidesWith(Input.MousePosition) ? Color.Green : Color.Yellow;
        }

        protected override void Destroy()
        { }
    }
}