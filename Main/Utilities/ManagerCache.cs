using Il2Cpp;
using UnityEngine;

namespace HelicopterMod.Main.Utilities
{
    public struct ManagerCache
    {
        private static PlayerManager _playerManager;
        private static Transform _playerTransform;
        
        public static PlayerManager PlayerManager
        {
            get
            {
                if(!_playerManager) _playerManager = GameManager.GetPlayerManagerComponent();
                return _playerManager;
            }
        }
        
        public static Transform PlayerTransform
        {
            get
            {
                if(!_playerTransform) _playerTransform = GameManager.GetPlayerTransform();
                return _playerTransform;
            }
        }
    }
}
