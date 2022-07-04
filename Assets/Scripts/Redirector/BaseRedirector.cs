using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseRedirector : MonoBehaviour
{
    public Users users {
        get { return environment.users; }
    }

    public VirtualSpace virtualEnvironment {
        get { return environment.virtualEnvironment; }
    }

    public RealSpace realSpace {
        get { return environment.realSpace; }
    }

    protected Environment environment {
        get { return GetComponent<Environment>(); }
    }

    protected CoinCollectTask experiment2
    {
        get { return GetComponent<CoinCollectTask>();}
    }

}
