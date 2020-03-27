using Harmony12;
using System.Collections.Generic;
using UnityEngine;

namespace DreamLover
{
	public static class GameDataHelper
	{
		public static bool HasCharacterProperty(int actorId, int propertyId)
		{
			return GameData.Characters.HasChar(actorId) && GameData.Characters.HasCharProperty(actorId, propertyId);
		}

		public static bool TryGetCharacterProperty(int actorId, int propertyId, out string value)
		{
			if(HasCharacterProperty(actorId, propertyId))
			{
				value = GameData.Characters.GetCharProperty(actorId, propertyId);
				return true;
			}
			value = default;
			return false;
		}
	}
}
