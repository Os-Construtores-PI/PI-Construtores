using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/UV Scroller")]
	public class UVScroller : MonoBehaviour
	{
		[Header("UV Scroller")]
		[Tooltip("Renderer component to apply the UV scrolling to.")]
		public new Renderer renderer;

		[Min(0f)]
		[Tooltip("Index of the material to apply the UV scrolling to.")]
		public int materialIndex;

		[Tooltip("Scrolling speed of the UV coordinates.")]
		public Vector2 speed;

		protected Material m_material;
		protected Vector2 m_offset;

		protected virtual void Start()
		{
			m_material = renderer.materials[materialIndex];
			m_offset = m_material.mainTextureOffset;
		}

		protected virtual void Update()
		{
			m_offset += speed * Time.deltaTime;
			m_material.mainTextureOffset = m_offset;
		}
	}
}
