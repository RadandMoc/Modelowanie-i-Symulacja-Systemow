using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class RandomController : IMakeAction
    {
        public DroneMove MakeAction()
        {
            Random random = new Random();
            //int actionIndex = random.Next(0, Enum.GetValues(typeof(DroneMove)).Length);
            int actionIndex = random.Next(0, 1);
            DroneMove action = (DroneMove)actionIndex;
            return action;
        }
    }
}
