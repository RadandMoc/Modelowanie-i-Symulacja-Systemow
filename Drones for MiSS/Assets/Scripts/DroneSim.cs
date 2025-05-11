using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem;
using System.Collections;
using Python.Runtime;
using System.IO;
using Assets.Scripts;
using SharpNeat.Phenomes;
using NUnit.Framework;
using System.Collections.Generic;

public class DroneSim : MonoBehaviour
{
	public float interval = 2f;
	public float holdTime = 1f;
	private bool isHeadless;
	private IMakeAction controller = new RandomController();
	byte key = 0x57; // W


	public void Initialize(IBlackBox blackBox, List<IGetInputs> inputSources)
	{
		NeatActivation neatActivation = new NeatActivation(blackBox, inputSources);
		controller = new GreedyNeatController(neatActivation, new DroneMove[] { DroneMove.Forward, DroneMove.Backward, DroneMove.Leftward, DroneMove.Rightward,
			DroneMove.Upward, DroneMove.Downward, DroneMove.RotateLeftward, DroneMove.BarrelRollRight  });
	}

	public void ReleaseKey() => KeyboardSimulator.ReleaseKey(key);

	public void ClickKey()
	{
		KeyboardSimulator.ReleaseKey(key);
		key = DroneMoveKeyMapping.GetKeyCode(controller.MakeAction());
		KeyboardSimulator.PressKey(key);
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