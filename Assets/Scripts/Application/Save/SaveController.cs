using System;
using System.IO;
using UnityEngine;
using WoodClicker.Infrastructure.Save;
using WoodClicker.State;

namespace WoodClicker.Application.Save
{
    public sealed class SaveController : MonoBehaviour
    {
        private SaveService _service;

        public LoadedGameState LoadOrCreate()
        {
            EnsureService();
            return _service.LoadOrCreate();
        }

        public void Save(PlayerGameState player, TreeState tree)
        {
            EnsureService();
            try
            {
                _service.Save(player, tree);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to save game state. {exception.Message}", this);
            }
        }

        private void EnsureService()
        {
            _service ??= new SaveService(Path.Combine(
                UnityEngine.Application.persistentDataPath,
                SaveService.DefaultFileName));
        }
    }
}
