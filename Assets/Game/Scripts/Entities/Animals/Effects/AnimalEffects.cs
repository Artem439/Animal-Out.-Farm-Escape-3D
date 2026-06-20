using System.Collections;
using DG.Tweening;
using Game.Scripts.Entities.Animals.Movement;
using UnityEngine;

namespace Game.Scripts.Entities.Animals.Effects
{
    public class AnimalEffects : MonoBehaviour
    {
        private const float HitPunchDuration = 0.3f;
        private const float HitTakeoffHeight = 0.04f;
        private const float HitLandHeight = 0.015f;
        private const float HitMaxDuration = 1.5f;

        private static readonly int IsMovingParameter = Animator.StringToHash("IsMoving");
        private static readonly int StunParameter = Animator.StringToHash("Stun");
        private static readonly int HitParameter = Animator.StringToHash("Hit");

        [SerializeField] private Mover _mover;
        [SerializeField] private Animator _animator;
        [SerializeField] private ParticleSystem[] _dustEffects;
        [SerializeField] private ParticleSystem _stunStars;
        [SerializeField] private GameObject _hitSymbols;
        [SerializeField] private float _stunEffectDuration = 1.5f;
        [SerializeField] private float _hitPunchScale = 0.4f;

        private Coroutine _stunRoutine;
        private Coroutine _hitRoutine;
        private WaitForSeconds _stunWait;

        private void Awake()
        {
            if (_mover == null)
                _mover = GetComponentInParent<Mover>();

            if (_animator == null)
                _animator = GetComponentInChildren<Animator>(true);

            _stunWait = new WaitForSeconds(_stunEffectDuration);
        }

        private void OnEnable()
        {
            if (_mover == null)
                return;

            _mover.Moving += OnMoving;
            _mover.Stopped += OnStopped;
            _mover.Stunned += OnStunned;
            _mover.Hit += OnHit;
        }

        private void OnDisable()
        {
            if (_mover != null)
            {
                _mover.Moving -= OnMoving;
                _mover.Stopped -= OnStopped;
                _mover.Stunned -= OnStunned;
                _mover.Hit -= OnHit;
            }

            StopDust();
            SetMovingAnimation(false);
            HideStunStars();
            HideHitSymbols();
        }

        private void SetMovingAnimation(bool isMoving)
        {
            if (_animator != null)
                _animator.SetBool(IsMovingParameter, isMoving);
        }

        private void PlayDust()
        {
            foreach (ParticleSystem effect in _dustEffects)
                effect.Play(false);
        }

        private void StopDust()
        {
            foreach (ParticleSystem effect in _dustEffects)
                effect.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }

        private void ShowStunStars()
        {
            if (_stunStars == null)
                return;

            if (_stunRoutine != null)
                StopCoroutine(_stunRoutine);

            _stunRoutine = StartCoroutine(StunStarsRoutine());
        }

        private IEnumerator StunStarsRoutine()
        {
            _stunStars.gameObject.SetActive(true);
            _stunStars.Play(true);

            yield return _stunWait;

            _stunStars.gameObject.SetActive(false);
            _stunRoutine = null;
        }

        private void HideStunStars()
        {
            if (_stunRoutine != null)
            {
                StopCoroutine(_stunRoutine);
                _stunRoutine = null;
            }

            if (_stunStars != null)
                _stunStars.gameObject.SetActive(false);
        }

        private void ShowHitSymbols()
        {
            if (_hitSymbols == null)
                return;

            if (_hitRoutine != null)
                StopCoroutine(_hitRoutine);

            _hitRoutine = StartCoroutine(HitSymbolsRoutine());
        }

        private IEnumerator HitSymbolsRoutine()
        {
            Transform symbolsTransform = _hitSymbols.transform;

            symbolsTransform.DOKill();
            symbolsTransform.localScale = Vector3.one;
            _hitSymbols.SetActive(true);
            symbolsTransform.DOPunchScale(Vector3.one * _hitPunchScale, HitPunchDuration);

            yield return WaitForLanding(symbolsTransform);

            _hitSymbols.SetActive(false);
            _hitRoutine = null;
        }

        private static IEnumerator WaitForLanding(Transform symbolsTransform)
        {
            yield return null;

            float groundHeight = symbolsTransform.position.y;
            float elapsed = 0f;
            bool tookOff = false;

            while (elapsed < HitMaxDuration)
            {
                float height = symbolsTransform.position.y - groundHeight;

                if (height > HitTakeoffHeight)
                {
                    tookOff = true;
                }
                else if (tookOff && height <= HitLandHeight)
                {
                    yield break;
                }

                elapsed += Time.deltaTime;

                yield return null;
            }
        }

        private void HideHitSymbols()
        {
            if (_hitRoutine != null)
            {
                StopCoroutine(_hitRoutine);
                _hitRoutine = null;
            }

            if (_hitSymbols != null)
            {
                _hitSymbols.transform.DOKill();
                _hitSymbols.SetActive(false);
            }
        }

        private void OnMoving()
        {
            HideStunStars();
            PlayDust();
            SetMovingAnimation(true);
        }

        private void OnStopped()
        {
            StopDust();
            SetMovingAnimation(false);
        }

        private void OnStunned()
        {
            if (_animator != null)
                _animator.SetTrigger(StunParameter);

            ShowStunStars();
        }

        private void OnHit()
        {
            if (_animator != null)
                _animator.SetTrigger(HitParameter);

            ShowHitSymbols();
        }
    }
}
