using MelonLoader;
using Il2Cpp;

namespace HelicopterMod.Main
{
    public class HelicopterMod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            
            Logger.Msg("HelicopterMod loading!");
        }
    }   
}