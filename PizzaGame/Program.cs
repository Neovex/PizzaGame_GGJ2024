using SFML.Window;
using BlackCoat;

namespace PizzaGame
{
    internal static class Program
    {
        public const string TITLE = "Pizza Topo";
        public static int MUSIC_VOLUME = 50;
        public static int SFX_VOLUME = 50;

        static void Main()
        {
#if !DEBUG
            var launcher = new Launcher()
            {
                //BannerImage = Image.FromFile("Assets\\Banner_V2.png"),
                Text = TITLE
            };
            var device = Device.Create(launcher, TITLE);
            if (device == null) return;
            MUSIC_VOLUME = launcher.MusicVolume;
            SFX_VOLUME = launcher.EffectVolume;
#endif

#if DEBUG
            var vm = new VideoMode(1920, 1080);
            var device = Device.Create(vm, TITLE, Styles.Resize, 0, false, 120);
#endif
            using (var core = new Core(device))
            {
#if DEBUG
                core.Debug = true;
#endif
                //core.SceneManager.ChangeScene(new BlackCoatIntro(core, new TitleScene(core)));
                core.SceneManager.ChangeScene(new MenueScene(core));
                core.Run();
            }
        }
    }
}