using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "GameSetting", menuName = "Setting/GameSettinga", order = 1)]
    public class GameSettingData : ScriptableObject
    {
        public string NmaeSpace = "Game";
        public string ResGroup = "";
    }
}