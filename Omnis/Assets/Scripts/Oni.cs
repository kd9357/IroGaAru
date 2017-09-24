using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oni : Enemy {

    //May need to have weapon as different script
    //So we can have varied damage

    void FixedUpdate()
    {
        //This unfortunately flips the debug text as well
        if ((_target.transform.position.x - transform.position.x > 0 && !_facingRight)
            || (_target.transform.position.x - transform.position.x < 0 && _facingRight))
            Flip();

        if (_recoilTimer <= 0)
        {
            //Leave AI_Type alone for now, replace later
            if (_actionTimer > 0)
                _actionTimer -= Time.deltaTime;
            else
            {
                switch (AI_Type)
                {
                    case "Stand":
                        if (InRange() && !_attacking)
                            Attack();
                        _rb.velocity = Vector2.zero;
                        break;
                    case "Move":
                        if (InRange() && !_attacking)
                        {
                            _rb.velocity = Vector2.zero;
                            Attack();
                        }
                        else if(!_attacking)
                        {
                            MoveForward();
                        }
                        break;
                }
            }

        }
    }
}
