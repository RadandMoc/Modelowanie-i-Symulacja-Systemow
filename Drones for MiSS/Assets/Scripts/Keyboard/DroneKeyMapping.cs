using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

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

        public static Key GetKey(DroneMove move)
        {
            switch (move)
            {
                // Podstawowy ruch (WSAD)
                case DroneMove.Forward: return Key.W; // 0x57
                case DroneMove.Backward: return Key.S; // 0x53
                case DroneMove.Rightward: return Key.D; // 0x44
                case DroneMove.Leftward: return Key.A; // 0x41

                // Ruch góra/dół (IK)
                case DroneMove.Upward: return Key.I; // 0x49
                case DroneMove.Downward: return Key.K; // 0x4B

                // Rotacja (JL)
                case DroneMove.RotateLeftward: return Key.J; // 0x4A
                case DroneMove.RotateRightward: return Key.L; // 0x4C

                // Beczki (EO)
                case DroneMove.BarrelRollLeft: return Key.E; // 0x45
                case DroneMove.BarrelRollRight: return Key.O; // 0x4F

                // Swing (QU)
                case DroneMove.SwingLeft: return Key.Q; // 0x51
                case DroneMove.SwingRight: return Key.U; // 0x55

            
                default:
                    return Key.None; // Zwraca specjalną wartość oznaczającą brak klawisza
            }
        }
    }
}
