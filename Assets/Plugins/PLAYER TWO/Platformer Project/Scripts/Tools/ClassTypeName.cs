using System;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	public class ClassTypeName : PropertyAttribute
	{
		public Type type;

		public ClassTypeName(Type type)
		{
			this.type = type;
		}
	}
}
