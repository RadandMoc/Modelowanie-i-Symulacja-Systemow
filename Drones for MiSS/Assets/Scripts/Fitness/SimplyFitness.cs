using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimplyFitness : MonoBehaviour, IFitnessFunction
{
	private DroneMove? move = null;
	private bool wasChanged = false;
	[SerializeField]
	private List<SprayableObject> sprayableObjects;

    public double AssessDroneFlewOutOfBounds(DroneSim sim)
    {
		return Math.Max(0.0, this.Evaluate() - 0.5);
    }

    public double Evaluate() => sprayableObjects.Sum(x => x.CalculateSprayResult()) + (wasChanged ? 1.0 : 0.0);
	
	public void OnMoveMade(DroneMove move, Transform trans)
	{
		if (this.move == null)
			this.move = move;
		wasChanged = wasChanged || this.move != move;
	}
}
