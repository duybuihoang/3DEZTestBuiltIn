using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyKidneyPunchRightState : EnemyBaseState
{
    public EnemyKidneyPunchRightState(EnemyController enemy, Animator anim) : base(enemy, anim)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();

        anim.CrossFade(KidneyPunchRight, crossFadeDuration);
    }
}
