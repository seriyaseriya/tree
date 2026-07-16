using System;

namespace WoodClicker.State
{
    [Serializable]
    public sealed class TreeState
    {
        public double MaxHealth { get; }
        public double CurrentHealth { get; private set; }
        public bool IsFelled => CurrentHealth <= 0d;

        public TreeState(double maxHealth)
        {
            if (maxHealth <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHealth));
            }

            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
        }

        public void ApplyDamage(double damage)
        {
            if (damage < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(damage));
            }

            CurrentHealth = Math.Max(0d, CurrentHealth - damage);
        }
    }
}
