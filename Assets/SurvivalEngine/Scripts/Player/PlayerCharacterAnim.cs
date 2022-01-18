using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalEngine
{

    /// <summary>
    /// Manages all character animations
    /// </summary>

    [RequireComponent(typeof(PlayerCharacter))]
    public class PlayerCharacterAnim : MonoBehaviour
    {
        public string move_anim = "Move";
        public string move_side_x = "MoveX";
        public string move_side_z = "MoveZ";
        public string attack_anim = "Attack";
        public string attack_speed = "AttackSpeed";
        public string take_anim = "Take";
        public string craft_anim = "Craft";
        public string build_anim = "Build";
        public string damaged_anim = "Damaged";
        public string death_anim = "Death";
        public string sleep_anim = "Sleep";
        public string fish_anim = "Fish";
        public string dig_anim = "Dig";
        public string water_anim = "Water";
        public string hoe_anim = "Hoe";
        public string ride_anim = "Ride";
        public string swim_anim = "Swim";
        public string climb_anim = "Climb";

        private PlayerCharacter character;
        private Animator animator;

        void Awake()
        {
            character = GetComponent<PlayerCharacter>();
            animator = GetComponentInChildren<Animator>();

            if (animator == null)
                enabled = false;
        }

        private void Start()
        {
            character.Inventory.onTakeItem += OnTake;
            character.Inventory.onDropItem += OnDrop;
            character.Crafting.onCraft += OnCraft;
            character.Crafting.onBuild += OnBuild;
            character.Combat.onAttack += OnAttack;
            character.Combat.onAttackHit += OnAttackHit;
            character.Combat.onDamaged += OnDamaged;
            character.Combat.onDeath += OnDeath;
            character.onTriggerAnim += OnTriggerAnim;

            if (character.Jumping)
                character.Jumping.onJump += OnJump;
        }

        void Update()
        {
            bool player_paused = TheGame.Get().IsPausedByPlayer();
            bool gameplay_paused = TheGame.Get().IsPausedByScript();
            animator.enabled = !player_paused;

            if (animator.enabled)
            {
                animator.SetBool(move_anim, !gameplay_paused && character.IsMoving());
                animator.SetBool(craft_anim, !gameplay_paused && character.Crafting.IsCrafting());
                animator.SetBool(sleep_anim, character.IsSleeping());
                animator.SetBool(fish_anim, character.IsFishing());
                animator.SetBool(ride_anim, character.IsRiding());
                animator.SetBool(swim_anim, character.IsSwimming());
                animator.SetBool(climb_anim, character.IsClimbing());

                Vector3 move_vect = character.GetMoveNormalized();
                float mangle = Vector3.SignedAngle(character.GetFacing(), move_vect, Vector3.up);
                Vector3 move_side = new Vector3(Mathf.Sin(mangle * Mathf.Deg2Rad), 0f, Mathf.Cos(mangle * Mathf.Deg2Rad));
                move_side = move_side * move_vect.magnitude;
                animator.SetFloat(move_side_x, move_side.x);
                animator.SetFloat(move_side_z, move_side.z);
            }
        }

        private void OnTake(Item item)
        {
            animator.SetTrigger(take_anim);
        }

        private void OnDrop(Item item)
        {
            //Add drop anim here
        }

        private void OnCraft(CraftData cdata)
        {
            //Add craft anim here
        }

        private void OnBuild(Buildable construction)
        {
            animator.SetTrigger(build_anim);
        }

        private void OnJump()
        {
            //Add jump animation here
        }

        private void OnDamaged()
        {
            animator.SetTrigger(damaged_anim);
        }

        private void OnDeath()
        {
            animator.SetTrigger(death_anim);
        }

        private void OnAttack(Destructible target, bool ranged)
        {
            string anim = attack_anim;
            float anim_speed = character.Combat.GetAttackAnimSpeed();

            //Replace anim based on current equipped item
            EquipItem equip = character.Inventory.GetEquippedWeaponMesh();
            if (equip != null)
            {
                if (!ranged && !string.IsNullOrEmpty(equip.attack_melee_anim))
                    anim = equip.attack_melee_anim;
                if (ranged && !string.IsNullOrEmpty(equip.attack_ranged_anim))
                    anim = equip.attack_ranged_anim;
            }

            animator.SetFloat(attack_speed, anim_speed);
            animator.SetTrigger(anim);
        }

        private void OnAttackHit(Destructible target)
        {

        }

        private void OnTriggerAnim(string anim, float duration)
        {
            if(!string.IsNullOrEmpty(anim))
                animator.SetTrigger(anim);
        }
    }

}