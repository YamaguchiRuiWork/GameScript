using UnityEngine;

namespace GameScript.Scripts
{
    public class AttackHandler
    {
        private readonly PlayerInputsManager _playerInputsManager;
        public int AnimIDAttack { get; set; }
        public int AnimIDCombat { get; set; }

        public AttackHandler(PlayerInputsManager inputManager)
        {
            _playerInputsManager = inputManager;
        }

        public void UpdateAttackState(Animator animator, bool hasAnimator)
        {
            if (!hasAnimator)
            {
                return;
            }

            if (!animator.GetBool(AnimIDCombat))
            {
                return;
            }

            if (_playerInputsManager.attack)
            {
                animator.SetTrigger(AnimIDAttack);
                _playerInputsManager.attack = false;
            }
        }
    }
}