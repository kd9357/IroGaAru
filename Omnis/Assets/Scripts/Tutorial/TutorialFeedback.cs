// TeamTwo

/*
 * Include Files
 */

using UnityEngine;

/*
 * Typedefs
 */

public class TutorialFeedback : MonoBehaviour
{
    public enum TutorialGoal
    {
        // Basic Movements
        MoveRight,
        MoveLeft,
        Jump,

        // Basic Attacks
        AttackRed,
        AttackYellow,
        AttackBlue,

        // Color Combinations
        AttackOrange,
        AttackPurple,
        AttackGreen
    }

    /*
     * Public Member Variables
     */

    public TutorialGoal Goal;
    public BoxCollider2D[] Barriers;
    public Enemy Enemy;
    public int Count = 1;

    /*
     * Private Member Variables
     */

    private static Player _player;

    private bool _commencedGoal;
    private bool _goalComplete;
    private int _currentCount;

    /*
     * Private (Unity) Method Declarations
     */

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        _goalComplete = false;
        _commencedGoal = false;
        _currentCount = 0;

        TriggerBarriers(false);
    }

    void Update()
    {
        if (_commencedGoal && !_goalComplete)
        {
            switch (Goal)
            {
                case TutorialGoal.MoveRight:
                    if (_player.IsMovingRight())
                        PerformObjective();
                    break;
                case TutorialGoal.MoveLeft:
                    if (_player.IsMovingLeft())
                        PerformObjective();
                    break;
                case TutorialGoal.Jump:
                    if (_player.IsJumping())
                        PerformObjective();
                    break;
                case TutorialGoal.AttackRed:
                    if (Enemy.IsRed())
                        PerformObjective();
                    break;
                case TutorialGoal.AttackYellow:
                    if (Enemy.IsYellow())
                        PerformObjective();
                    break;
                case TutorialGoal.AttackBlue:
                    if (Enemy.IsBlue())
                        PerformObjective();
                    break;
                case TutorialGoal.AttackOrange:
                    if (Enemy.IsOrange())
                        PerformObjective();
                    break;
                case TutorialGoal.AttackPurple:
                    if (Enemy.IsPurple())
                        PerformObjective();
                    break;
                case TutorialGoal.AttackGreen:
                    if (Enemy.IsGreen())
                        PerformObjective();
                    break;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag != "Player")
            return;

        if (!_commencedGoal)
        {
            TriggerBarriers(true);
            _commencedGoal = true;
        }
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.tag != "Player")
            return;

        if (!_commencedGoal)
        {
            TriggerBarriers(true);
            _commencedGoal = true;
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.tag != "Player")
            return;

        if (_goalComplete)
            TriggerBarriers(false);
    }

    /*
     * Private Method Declarations
     */

    private void PerformObjective()
    {
        ++_currentCount;
        if (_currentCount >= Count)
            _goalComplete = true;
    }

    private void TriggerBarriers(bool status)
    {
        foreach (var wall in Barriers)
        {
            wall.enabled = status;
        }
    }
}                 