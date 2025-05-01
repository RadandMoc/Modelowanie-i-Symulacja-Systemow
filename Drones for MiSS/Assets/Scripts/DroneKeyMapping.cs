using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{

    public static class DroneMoveKeyMapping
    {
        // Mapa: ruch -> kod klawisza (w formacie heksadecymalnym)
        private static readonly Dictionary<DroneMove, byte> keyMap = new Dictionary<DroneMove, byte>
    {
        { DroneMove.Forward,         0x57 }, // W
        { DroneMove.Backward,        0x53 }, // S
        { DroneMove.Rightward,       0x44 }, // D
        { DroneMove.Leftward,        0x41 }, // A
        { DroneMove.Upward,          0x49 }, // I
        { DroneMove.Downward,        0x4B }, // K
        { DroneMove.RotateRightward, 0x4C }, // L
        { DroneMove.RotateLeftward,  0x4A }, // J
        { DroneMove.BarrelRollRight, 0x4F }, // O
        { DroneMove.BarrelRollLeft,  0x45 }, // E
        { DroneMove.SwingRight,      0x55 }, // U
        { DroneMove.SwingLeft,       0x51 }, // Q
    };

        /// <summary>
        /// Zwraca odpowiadający klawisz (kod wirtualny) dla danego ruchu.
        /// </summary>
        public static byte GetKeyCode(this DroneMove move)
        {
            return keyMap[move];
        }
    }
}
