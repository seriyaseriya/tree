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
            : this(maxHealth, maxHealth)
        {
        }

        public TreeState(double maxHealth, double currentHealth)
        {
            if (maxHealth <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHealth));
            }

            if (currentHealth < 0d || currentHealth > maxHealth)
            {
                throw new ArgumentOutOfRangeException(nameof(currentHealth));
            }

            MaxHealth = maxHealth;
            CurrentHealth = currentHealth;
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
