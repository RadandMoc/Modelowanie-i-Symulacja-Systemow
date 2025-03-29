using UnityEngine;
using Redzen;

public class NeatTest : MonoBehaviour
{
	void Start()
	{
		// Próbujemy stworzyæ obiekt z biblioteki
		try
		{
			var randomSource = new Redzen.Random.Xoshiro256StarStarRandom(42);
			Debug.Log("Biblioteka Redzen za³adowana poprawnie!");
			Debug.Log($"Wylosowana liczba: {randomSource.NextDouble()}");
		}
		catch (System.Exception ex)
		{
			Debug.LogError("Problem z za³adowaniem biblioteki: " + ex.Message);
		}
	}
}
