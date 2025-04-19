using UnityEngine;

namespace GameScript.Scripts
{
    public class JumpHandler
    {
        private PlayerInputsManager _inputManager;
        private float _jumpGraceTimer;
        private float _fallTimeoutDelta;
        private readonly float _fallTimeout;
        private bool _jumpTriggered;
        private float _jumpTimeoutDelta;
        private float _jumpDelayTimer;
        private readonly float _jumpDelay;
        private readonly float _jumpHeight;
        private readonly float _gravity;
        private readonly float _jumpGraceTime;
        private readonly float _jumpTimeout;
        private readonly float _terminalVelocity;
        public int AnimIDFreeFall { get; set; }
        public int AnimIDJump { get; set; }

        private float VerticalVelocity { get; set; }

        // 各種タイマーと値（jumpTimeout など）をここで管理

        public JumpHandler(PlayerInputsManager inputManager, float fallTimeout, float jumpDelay,
            float jumpHeight, float gravity, float jumpGraceTime, float jumpTimeout, float terminalVelocity)
        {
            _inputManager = inputManager;
            _fallTimeoutDelta = fallTimeout;
            _jumpDelay = jumpDelay;
            _jumpHeight = jumpHeight;
            _gravity = gravity;
            _jumpGraceTime = jumpGraceTime;
            _jumpTimeoutDelta = jumpTimeout;
            _terminalVelocity = terminalVelocity;
            _jumpTimeout = jumpTimeout;
            _fallTimeout = fallTimeout;
            
        }

        public float UpdateJump(bool isGrounded, Animator animator, bool hasAnimator)
        {
            bool effectiveGrounded = isGrounded && (_jumpGraceTimer <= 0f);

            if (effectiveGrounded)
            {
                _fallTimeoutDelta = _fallTimeout;

                if (hasAnimator)
                {
                    animator.SetBool(AnimIDJump, false);
                    animator.SetBool(AnimIDFreeFall, false);
                }

                if (!_jumpTriggered)
                {
                    // 地面に貼り付ける（浮かない）
                    VerticalVelocity = -2f;
                }

                // 入力検出後ジャンプトリガーとアニメーション
                if (_inputManager.jump && _jumpTimeoutDelta <= 0.0f && !_jumpTriggered)
                {
                    _jumpTriggered = true;
                    _jumpDelayTimer = _jumpDelay;

                    if (hasAnimator)
                    {
                        animator.SetBool(AnimIDJump, true);
                    }

                    _inputManager.jump = false;
                }

                // 踏み切り時間中：重力やY移動を止める
                if (_jumpTriggered)
                {
                    _jumpDelayTimer -= Time.deltaTime;

                    VerticalVelocity = 0f;

                    if (_jumpDelayTimer <= 0f)
                    {
                        // 実ジャンプ！
                        VerticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
                        _jumpTriggered = false;
                        _jumpGraceTimer = _jumpGraceTime; // ← ここでgraceタイマー開始！
                    }
                }

                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                _jumpTimeoutDelta = _jumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    if (hasAnimator)
                    {
                        animator.SetBool(AnimIDFreeFall, true);
                    }
                }

                _inputManager.jump = false;
                _jumpTriggered = false; // 空中でリセット
            }

            // graceタイマー減衰
            if (_jumpGraceTimer > 0f)
            {
                _jumpGraceTimer -= Time.deltaTime;
            }

            // ジャンプ中だけ重力加算
            if (!_jumpTriggered && VerticalVelocity < _terminalVelocity)
            {
                VerticalVelocity += _gravity * Time.deltaTime;
            }

            return VerticalVelocity;
        }
    }
}