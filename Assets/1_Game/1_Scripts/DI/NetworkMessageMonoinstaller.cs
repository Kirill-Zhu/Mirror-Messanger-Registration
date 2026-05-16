using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class NetworkMessageMonoinstaller : MonoInstaller
{
    [SerializeField] NetworkMessageExample networkMessanger;

    public override void InstallBindings() {
        Container.Bind<NetworkMessageExample>().FromInstance(networkMessanger);
    }
    

}
