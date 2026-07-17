using System;
using WoodClicker.Domain.Gacha;

namespace WoodClicker.Infrastructure.Gacha
{
    public sealed class SystemRandomProvider : IRandomProvider
    {
        private readonly Random _random = new Random();

        public double NextDouble()
        {
            return _random.NextDouble();
        }
    }
}
