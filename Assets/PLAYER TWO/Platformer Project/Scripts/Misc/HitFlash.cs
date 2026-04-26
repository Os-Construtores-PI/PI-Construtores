using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Health))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Hit Flash")]
	public class HitFlash : MonoBehaviour
	{
		[Header("Flash Settings")]
		[Tooltip(
			"The SkinnedMeshRenderers to flash when the object takes damage. "
				+ "If empty, the component will attempt to find SkinnedMeshRenderers "
				+ "in the children of the GameObject."
		)]
		public SkinnedMeshRenderer[] renderers;

		[Tooltip("Name of the property that represents the color of the material.")]
		public string colorProperty = "_Color";

		[Tooltip("The color to flash the renderers.")]
		public Color flashColor = Color.red;

		[Tooltip("The duration of the flash.")]
		public float flashDuration = 0.5f;

		protected Health m_health;
		protected List<Material> m_materials = new();

		protected virtual void InitializeProperty()
		{
			if (string.IsNullOrEmpty(colorProperty))
			{
				EmptyPropertyWarning();
				colorProperty = "_Color";
			}
		}

		protected virtual void InitializeRenderers()
		{
			renderers = renderers.Where(r => r != null).ToArray();

			if (renderers.Length == 0)
			{
				LogRenderersWarning();
				renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
			}
		}

		protected virtual void InitializeMaterials()
		{
			foreach (var renderer in renderers)
			{
				if (!renderer || renderer.materials.Length == 0)
					continue;

				m_materials.AddRange(renderer.materials);
			}

			m_materials = m_materials.Distinct().ToList();
			m_materials.ForEach(m =>
			{
				if (!m.HasProperty(colorProperty))
					NoPropertyWarning(m);
			});
		}

		protected virtual void InitializeHealth()
		{
			m_health = GetComponent<Health>();
			m_health.onDamage.AddListener(Flash);
		}

		public virtual void Flash()
		{
			StopAllCoroutines();
			m_materials.ForEach(m => StartCoroutine(FlashRoutine(m)));
		}

		protected virtual IEnumerator FlashRoutine(Material material)
		{
			if (!material.HasProperty(colorProperty))
				yield break;

			var elapsedTime = 0f;
			var flashColor = this.flashColor;
			var initialColor = material.GetColor(colorProperty);

			while (elapsedTime < flashDuration)
			{
				elapsedTime += Time.deltaTime;
				material.SetColor(
					colorProperty,
					Color.Lerp(flashColor, initialColor, elapsedTime / flashDuration)
				);
				yield return null;
			}

			material.SetColor(colorProperty, initialColor);
		}

		protected virtual void LogRenderersWarning()
		{
			Debug.LogWarning(
				"Hit Flash: Renderers array is empty. The component will "
					+ "attempt to find SkinnedMeshRenderers in the children of the GameObject "
					+ "which may cause unwanted meshes to flash. To avoid this, assign the "
					+ "desired SkinnedMeshRenderers to the Renderers array in the inspector.",
				this
			);
		}

		protected virtual void EmptyPropertyWarning()
		{
			Debug.LogWarning(
				"Hit Flash: The color property is empty. The component will use the default "
					+ "property '_Color'. To avoid this, assign the desired property name in the "
					+ "inspector.",
				this
			);
		}

		protected virtual void NoPropertyWarning(Material material)
		{
			Debug.LogWarning(
				$"Hit Flash: The shader from \"{material.name}\" material does not "
					+ $"have the property \"{colorProperty}\". The Hit Flash component "
					+ $"from \"{gameObject.name}\" Game Object will not work on this material.",
				this
			);
		}

		protected virtual void Start()
		{
			InitializeProperty();
			InitializeRenderers();
			InitializeMaterials();
			InitializeHealth();
		}
	}
}
