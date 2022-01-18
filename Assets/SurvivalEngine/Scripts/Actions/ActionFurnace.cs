using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalEngine
{
    /// <summary>
    /// Melt an item in the furnace
    /// </summary>

    [CreateAssetMenu(fileName = "Action", menuName = "SurvivalEngine/Actions/Furnace", order = 50)]
    public class ActionFurnace : MAction
    {
        public ItemData melt_item;
        public int melt_item_quantity = 1;
        public float duration = 1f; //In game hours

        //Merge action
        public override void DoAction(PlayerCharacter character, ItemSlot slot, Selectable select)
        {
            InventoryData inventory = slot.GetInventory();
            InventoryItemData iidata = inventory.GetItem(slot.index);

            Furnace furnace = select.GetComponent<Furnace>();
            if (furnace != null && furnace.CountItemSpace() > 0)
            {
                int create_quantity = Mathf.FloorToInt(iidata.quantity / (float)melt_item_quantity);
                int quantity = furnace.PutItem(slot.GetItem(), melt_item, duration, create_quantity);
                inventory.RemoveItemAt(slot.index, quantity * melt_item_quantity);
            }
        }

        public override bool CanDoAction(PlayerCharacter character, ItemSlot slot, Selectable select)
        {
            Furnace furnace = select.GetComponent<Furnace>();
            InventoryData inventory = slot.GetInventory();
            InventoryItemData iidata = inventory?.GetItem(slot.index);
            return furnace != null && iidata != null && furnace.CountItemSpace() > 0 && melt_item_quantity > 0;
        }
    }

}
