using System;
using UnityEngine;
using VRC.SDKBase;

namespace GotoUdon.VRC
{
    public class VRCDestructible : MonoBehaviour, IVRC_Destructible
    {
        private VRC_CombatSystem _vrcCombatSystem;
        public float maxHealth;
        public float currentHealth;
        public object[] state;
        private VRCPlayer _vrcPlayer;
        private bool _setup = false;

        public void Setup()
        {
            state = new object[0];
            _vrcCombatSystem = VRC_CombatSystem.GetInstance();
            maxHealth = _vrcCombatSystem.maxPlayerHealth;
            currentHealth = _vrcCombatSystem.maxPlayerHealth;
            SimulatedVRCPlayer simulatedVrcPlayer = GetComponent<SimulatedVRCPlayer>();
            if (simulatedVrcPlayer != null)
            {
                _vrcPlayer = simulatedVrcPlayer.VRCPlayer;
                _vrcCombatSystem.onSetupPlayer(_vrcPlayer);
            }
        }

        public object[] GetState()
        {
            return state;
        }

        public void SetState(object[] state)
        {
            this.state = state;
        }

        public float GetMaxHealth()
        {
            return maxHealth;
        }

        public float GetCurrentHealth()
        {
            return currentHealth;
        }

        public void ApplyDamage(float damage)
        {
            if (!_setup) return;
            currentHealth -= damage;
            if (_vrcPlayer != null)
            {
                _vrcCombatSystem.onPlayerDamaged(_vrcPlayer);
                if (currentHealth <= 0)
                {
                    _vrcCombatSystem.onPlayerKilled(_vrcPlayer);
                }
            }
        }

        public void ApplyHealing(float healing)
        {
            if (!_setup) return;
            currentHealth = Math.Min(currentHealth + healing, maxHealth);
            _vrcCombatSystem.onPlayerHealed(_vrcPlayer);
        }
    }
}