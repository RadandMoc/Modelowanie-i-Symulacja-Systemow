using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class SprayBehaviour : MonoBehaviour
    {

		[SerializeField]
		float minDistance;

        [SerializeField]
        int sprayForce;

        public void Spray() 
        {
			Ray ray = new Ray(transform.position, transform.forward);
			RaycastHit hit; 
            if (Physics.Raycast(ray, out hit, minDistance))
			{
                try
                {
                    TypeOfObstacle obs = hit.collider.gameObject.GetComponent<TypeOfObstacle>();
                    if (obs != null && obs.IsForClean == 1.0)
                    {
                        SprayableObject sprayableObject = hit.collider.gameObject.GetComponent<SprayableObject>();
                        sprayableObject.ChangeSpray(sprayForce);
                        if (sprayableObject.IsCleaned()) 
                        {
                            obs.ChangeCleaning();
                        }
                    }

                    if (obs.IsNotForClean == 1.0) 
                    {
                        NotSpraybleObject notSpraybleObject = hit.collider.gameObject.GetComponent<NotSpraybleObject>();
                        notSpraybleObject.ChangeSpray();
                    }
                    
                }
                catch 
                {
                    // Do nothing
                }
            }
        }
    }
}
