using UnityEngine;

namespace GameScript.Scripts
{
    public class DodgeHandler
    {
        private readonly PlayerInputsManager _playerInputsManager;
        public int AnimIDDodge { get; set; }
        
        public DodgeHandler(PlayerInputsManager inputManager)
        {
            _playerInputsManager = inputManager;
        }
        
        public void UpdateDodgeState(Animator animator, bool hasAnimator)
        {
            if (!hasAnimator)
            {
                return;
            }

            if (_playerInputsManager.dodge)
            {
                animator.SetTrigger(AnimIDDodge);
                _playerInputsManager.dodge = false;
            }
        }
    }
}