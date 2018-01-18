namespace IDeliverable.Utils.Core.CollectionExtensions
{
    public enum CollectionSynchronizationMode
    {
        /// <summary>
        /// Order in source collection is not enforced in the target collection.
        /// </summary>
        IgnoreOrder,

        /// <summary>
        /// Order in source collection is reflected in target collection by moving items.
        /// </summary>
        KeepOrderByMove,

        /// <summary>
        /// Order in source colection is reflected in target collection by removing and re-inserting items.
        /// </summary>
        KeepOrderByRemoveInsert
    }
}
