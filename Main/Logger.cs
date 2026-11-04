using Il2Cpp;
using MelonLoader;

namespace HelicopterMod.Main
{
    public static class Logger
    {
        public static void Msg(string message)
        {
            Melon<HelicopterMod>.Logger.Msg(message);
        }
        
        public static void MsgConsole(string message)
        {
            uConsole.Log(message);
            Melon<HelicopterMod>.Logger.Msg(message);
        }
        
        public static void Warning(string message)
        {
            uConsole.Log(message);
            Melon<HelicopterMod>.Logger.Warning(message);
        }
        
        public static void Error(string message)
        {
            uConsole.Log(message);
            Melon<HelicopterMod>.Logger.Error(message);
        }
    }    
}
