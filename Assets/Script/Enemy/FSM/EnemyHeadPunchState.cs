using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHeadPunchState : EnemyBaseState
{
    public EnemyHeadPunchState(EnemyController enemy, Animator anim) : base(enemy, anim)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();

        anim.CrossFade(HeadPunch, crossFadeDuration);
    }
}
