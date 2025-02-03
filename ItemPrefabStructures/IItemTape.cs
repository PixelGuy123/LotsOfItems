namespace LotsOfItems.ItemPrefabStructures
{
	public interface ITapeItem : IItemPrefab
	{
		// TODO: methods that works with a patch to override the tape player as it wishes
		void OverrideTape(TapePlayer tapePlayer);
	}
}
