using UnityEngine;

namespace GameScript.Scripts
{
    public class GuardHandler
    {
        private readonly PlayerInputsManager _playerInputsManager;
        public int AnimIDGuard { get; set; }
        public bool DoGuard { get; private set; }
        

        public GuardHandler(PlayerInputsManager inputManager)
        {
            _playerInputsManager = inputManager;
        }

        public void UpdateGuardState(Animator animator, bool hasAnimator)
        {
            if (!hasAnimator)
            {
                return;
            }

            if (_playerInputsManager.guard)
            {
                DoGuard = true;
                animator.SetBool( AnimIDGuard, true );
            }
            else
            {
                DoGuard = false;
                animator.SetBool( AnimIDGuard, false );
            }
        }
    }
}