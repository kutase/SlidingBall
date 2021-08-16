using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class DIInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<GameController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<BallController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<UIController>().FromComponentInHierarchy().AsSingle();

        // game logic events
        Container.Bind<UnityEvent>().WithId("CrystalCollectedEvent").AsCached().NonLazy();
        Container.Bind<UnityEvent>().WithId("GameOverEvent").AsCached().NonLazy();
        Container.Bind<UnityEvent>().WithId("ResetGameEvent").AsCached().NonLazy();
        Container.Bind<UnityEvent>().WithId("StartGameEvent").AsCached().NonLazy();

        // UI events
        Container.Bind<UnityEvent>().WithId("UICrystalsCountUpdateEvent").AsCached().NonLazy();
        Container.Bind<UnityEvent>().WithId("UIGameOverEvent").AsCached().NonLazy();
        Container.Bind<UnityEvent>().WithId("UIResetEvent").AsCached().NonLazy();
        Container.Bind<UnityEvent>().WithId("UIStartGameEvent").AsCached().NonLazy();
    }
}
