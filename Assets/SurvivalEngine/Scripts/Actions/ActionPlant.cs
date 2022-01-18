using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalEngine
{
    /// <summary>
    /// Sow a seed in the ground.
    /// </summary>
    
    [CreateAssetMenu(fileName = "Action", menuName = "SurvivalEngine/Actions/Plant", order = 50)]
    public class ActionPlant : SAction
    {
        public override void DoAction(PlayerCharacter character, ItemSlot slot)
        {
            ItemData item = slot.GetItem();
            InventoryData inventory = slot.GetInventory();
            if (item != null && item.plant_data != null)
            {
                character.Crafting.CraftPlantBuildMode(item.plant_data, 0, false, (Buildable build) =>
                {
                    inventory.RemoveItemAt(slot.index, 1);
                });

                TheAudio.Get().PlaySFX("craft", item.craft_sound);
            }
        }

        public override bool CanDoAction(PlayerCharacter character, ItemSlot slot)
        {
            ItemData item = slot.GetItem();
            return item != null && item.plant_data != null;
        }
    }

}