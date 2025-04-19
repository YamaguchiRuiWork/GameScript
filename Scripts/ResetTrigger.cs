using UnityEngine;

namespace GameScript.Scripts
{
    public class ResetTrigger : StateMachineBehaviour
    {
        [SerializeField]
        private string triggerName;

        // OnStateExit is called before OnStateExit is called on any state inside this state machine
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.ResetTrigger(triggerName);
        }
    }
}
