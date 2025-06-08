using Assets.Scripts;
using Assets.Scripts.DroneBehaviour;
using NUnit.Framework;
using Python.Runtime;
using SharpNeat.Phenomes;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class DroneSim : MonoBehaviour
{
	public float interval = 2f;
	public float holdTime = 1f;
	private bool isHeadless;
	private IMakeAction controller = new RandomController();
	byte key = 0x57; // W
	[SerializeField]
	SprayBehaviour spray;
    [SerializeField]
    DroneActions droneActions;
	private ulong _sprayCount = 0;
	[SerializeField]
	private TextMeshProUGUI _sprayTextField;

	public void Initialize(IBlackBox blackBox, List<IGetInputs> inputSources)
	{
		NeatActivation neatActivation = new NeatActivation(blackBox, inputSources);
		controller = new GreedyNeatController(neatActivation, new DroneMove[] { DroneMove.Forward, DroneMove.Backward, DroneMove.Leftward, DroneMove.Rightward,
			DroneMove.Upward, DroneMove.Downward, DroneMove.RotateLeftward, DroneMove.RotateRightward, DroneMove.Spray, DroneMove.DoNothing  });
	}

	public void ReleaseKey() => KeyboardSimulator.ReleaseKey(key);

	public DroneMove ClickKey()
	{
		//KeyboardSimulator.ReleaseKey(key);
		DroneMove move = controller.MakeAction();			
		//Debug.Log(move);

		if (move == DroneMove.DoNothing)
		{
			Debug.Log("No action taken");
		}
		else if (move != DroneMove.Spray) 
		{
			droneActions.MakeAction(move);
        }
		else
		{
			_sprayTextField.text = $"Spray: {++_sprayCount}";
			Debug.Log("Spray action triggered");
            spray.Spray();
        }
		return move;
	}

	private IEnumerator RandomKeyPressCoroutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(interval);

			// Wybierz losowy klawisz
			byte key = DroneMoveKeyMapping.GetKeyCode(controller.MakeAction());

			if (isHeadless)
			{
				KeyboardSimulator.PressKey(key);
				Debug.Log($"[Headless] Press {key:X2}");

				yield return new WaitForSeconds(holdTime);

				KeyboardSimulator.ReleaseKey(key);
				Debug.Log($"[Headless] Release {key:X2}");
			}
			else
			{
				// Tryb GUI: tutaj mo¿esz u¿yæ innej metody, np. symulowaæ zdarzenia wejœcia lub korzystaæ z fizycznej klawiatury.
				// W tym przyk³adzie dla uproszczenia wywo³ujemy te same metody, ale mo¿esz to zmodyfikowaæ.
				KeyboardSimulator.PressKey(key);
				Debug.Log($"[GUI] Simulated Press {key:X2}");

				yield return new WaitForSeconds(holdTime);

				KeyboardSimulator.ReleaseKey(key);
				Debug.Log($"[GUI] Simulated Release {key:X2}");
			}
		}
	}

}