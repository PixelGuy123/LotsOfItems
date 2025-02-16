using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace LotsOfItems.Components;
public class SodaPuddle : MonoBehaviour
{
	[SerializeField]
	internal MovementModifier speedDebuff = new(Vector3.zero, 0.6f);

	[SerializeField]
	internal float spawnSpeed = 10f;

	EnvironmentController ec;

	readonly List<Entity> entities = [];

	public void Initialize(float duration, EnvironmentController ec)
	{
		this.ec = ec;
		StartCoroutine(Lifetime(duration));
	}

	private IEnumerator Lifetime(float duration)
	{
		float scale = 0f;
		while (true)
		{
			scale += ec.EnvironmentTimeScale * Time.deltaTime * spawnSpeed;
			if (scale >= 1f)
				break;
			transform.localScale = scale * Vector3.one;
			yield return null;
		}

		transform.localScale = Vector3.one;

		float timer = duration;
		while (timer > 0f)
		{
			timer -= Time.deltaTime;
			yield return null;
		}

		scale = 1f;
		while (true)
		{
			scale -= ec.EnvironmentTimeScale * Time.deltaTime * spawnSpeed;
			if (scale <= 0f)
				break;
			transform.localScale = scale * Vector3.one;
			yield return null;
		}

		Destroy(gameObject);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.isTrigger && other.TryGetComponent<Entity>(out var entity))
		{
			entities.Add(entity);
			entity.ExternalActivity.moveMods.Add(speedDebuff);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.isTrigger && other.TryGetComponent<Entity>(out var entity))
		{
			entity.ExternalActivity.moveMods.Remove(speedDebuff);
			entities.Remove(entity);
		}
	}

	void OnDestroy()
	{
		foreach (var entity in entities)
			entity.ExternalActivity.moveMods.Remove(speedDebuff);
	}
}