using MelonLoader;

namespace HelicopterMod.Main
{
    public static class Logger
    {
        public static void Msg(string message)
        {
            Melon<HelicopterMod>.Logger.Msg(message);
        }
    }    
}
