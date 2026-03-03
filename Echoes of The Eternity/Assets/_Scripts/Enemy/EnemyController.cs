using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyController : MonoBehaviour
{
    public EnemyStatsSO Stats;
    [SerializeField] private MainGunDataSO mainGunData;

    private NavMeshAgent _agent;
    private EnemyHealth _health;
    private Transform _player;
    private PlayerHealth _playerHealth;
    private MissionManager _missionManager;

    private enum State { Idle, Patrol, Investigate, Chase, Attack, Dead }
    private State _state = State.Idle;

    private enum AlertLevel { Unaware, Suspicious, Alerted }
    private AlertLevel _alertLevel = AlertLevel.Unaware;
    private float _susMeter = 0f;
    private float _susThreshold = 100f;

    // expose suspicion for UI/other systems
    public float GetSuspicion() => _susMeter;

    [Header("Patrol")]
    public Transform[] PatrolPoints;
    private int _patrolIndex = 0;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] attackSoundsClose, attackSoundsFar;
    public AudioClip[] deathSounds;

    private float _lastAttackTime;
    private int _currentAmmo = 0;
    private bool _isReloading = false;

    [Header("Stealth View")]
    public float viewRadius = 12f;
    [Range(10f, 180f)] public float viewAngle = 90f;

    [Header("Suspicion Settings")]
    [Tooltip("Suspicion increase per second when player is casing in public areas")]
    public float susRatePublic = 5f;
    [Tooltip("Suspicion increase per second when player is casing in private areas")]
    public float susRatePrivate = 25f;
    [Tooltip("Suspicion increase per second when player is casing in secure areas")]
    public float susRateSecure = 50f;
    [Tooltip("Suspicion increase per second when player is masked (treated like secure)")]
    public float susRateMasked = 50f;
    [Tooltip("How fast suspicion decays per second when not seen")]
    public float susDecayRate = 10f;

    // Investigation
    private bool _isInvestigating = false;
    private Vector3 _investigatePoint;
    private float _investigateTimer = 0f;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _health = GetComponent<EnemyHealth>();

        if (Stats != null)
        {
            _agent.speed = Stats.WalkSpeed;
            viewRadius = Stats.DetectionRadius;
        }

        if (mainGunData != null)
            _currentAmmo = mainGunData.magazineSize;

        _player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (_player != null)
        {
            _playerHealth = _player.GetComponentInChildren<PlayerHealth>();
            if (_playerHealth == null) _playerHealth = _player.GetComponent<PlayerHealth>();
        }

        if (_playerHealth == null)
        {
            _playerHealth = FindObjectOfType<PlayerHealth>();
            if (_playerHealth != null && _player == null) _player = _playerHealth.transform;
        }

        _mission_manager_fallback();
    }

    // separate so we can tolerate missing mission manager at edit-time
    private void _mission_manager_fallback()
    {
        _missionManager = FindObjectOfType<MissionManager>();
    }

    private void Update()
    {
        if (!_health.IsAlive) return;
        if (_player == null) return;

        var stage = _missionManager != null ? _missionManager.GetHeistStage() : MissionManager.HeistStage.Assault;
        float dist = Vector3.Distance(transform.position, _player.position);

        if (stage == MissionManager.HeistStage.Stealth)
        {
            HandleStealth(dist);
        }
        else
        {
            HandleLoud(dist);
        }
    }

    #region Stealth behavior
    private void HandleStealth(float distToPlayer)
    {
        bool sees = CanSeePlayer(distToPlayer);

        // determine suspicion increase rate based on player state and location
        float susIncrease = susRatePublic;
        if (_missionManager != null)
        {
            var playerState = _missionManager.GetPlayerState();
            if (playerState == MissionManager.PlayerState.Masked)
            {
                susIncrease = susRateMasked;
            }
            else
            {
                switch (_missionManager.currentPlayerLocation)
                {
                    case MissionManager.PlayerLocation.Public:
                        susIncrease = susRatePublic;
                        break;
                    case MissionManager.PlayerLocation.Private:
                        susIncrease = susRatePrivate;
                        break;
                    case MissionManager.PlayerLocation.Secure:
                        susIncrease = susRateSecure;
                        break;
                    default:
                        susIncrease = susRatePublic;
                        break;
                }
            }
        }

        if (sees)
        {
            _susMeter += susIncrease * Time.deltaTime;
            if (_alertLevel == AlertLevel.Unaware && _susMeter > 0f) _alertLevel = AlertLevel.Suspicious;
        }
        else
        {
            _susMeter = Mathf.Max(0f, _susMeter - susDecayRate * Time.deltaTime);
            if (_susMeter <= 0f) _alertLevel = AlertLevel.Unaware;
        }

        _susMeter = Mathf.Clamp(_susMeter, 0f, _susThreshold);

        if (_susMeter >= _susThreshold)
        {
            _alertLevel = AlertLevel.Alerted;
            _missionManager?.PullAlarm();
        }
        else if (_susMeter >= _susThreshold * 0.5f)
        {
            if (sees)
            {
                _investigatePoint = _player.position;
            }

            if (!_isInvestigating)
            {
                _isInvestigating = true;
                _investigateTimer = 0f;
                _state = State.Investigate;
                _agent.isStopped = false;
                _agent.speed = Stats != null ? Stats.WalkSpeed : 2.5f;
                _agent.SetDestination(_investigatePoint);
            }

            if (_isInvestigating && !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
            {
                _investigateTimer += Time.deltaTime;
                if (_investigateTimer > 5f)
                {
                    // return to patrol with slight heightened senses
                    _isInvestigating = false;
                    _susMeter = 0f;
                    StartCoroutine(TemporaryHeightenedSenses());
                    StartPatrol();
                }
            }
        }
        else
        {
            // Regular patrol/idle
            if (PatrolPoints != null && PatrolPoints.Length > 0)
            {
                if (_state != State.Patrol)
                    StartPatrol();

                if (_state == State.Patrol)
                    PatrolUpdate();
            }
        }
    }

    private IEnumerator TemporaryHeightenedSenses()
    {
        float original = viewRadius;
        viewRadius = original * 1.5f;
        yield return new WaitForSeconds(8f);
        viewRadius = original;
    }

    private bool CanSeePlayer(float distToPlayer)
    {
        if (distToPlayer > viewRadius) return false;
        Vector3 dirToPlayer = (_player.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, dirToPlayer) > viewAngle * 0.5f) return false;
        return HasLineOfSight();
    }
    #endregion

    #region Loud behavior
    private void HandleLoud(float distToPlayer)
    {
        // In loud/assault, enemies actively chase and engage the player.
        bool hasLOS = HasLineOfSight();
        float attackRange = Stats != null ? Stats.AttackRange : 10f;

        if (_isReloading)
        {
            // fall back a bit while reloading
            Vector3 away = (transform.position - _player.position).normalized;
            _agent.isStopped = false;
            _agent.speed = Stats != null ? Stats.WalkSpeed : 2.5f;
            _agent.SetDestination(transform.position + away * 3f);
            return;
        }

        if (distToPlayer > attackRange || !hasLOS)
        {
            // move closer
            _agent.isStopped = false;
            _agent.speed = Stats != null ? Stats.RunSpeed : 4.5f;
            _agent.SetDestination(_player.position);
            _state = State.Chase;
        }
        else
        {
            // in range - stop and shoot
            _agent.isStopped = true;
            _state = State.Attack;

            // rotate towards player smoothly
            Vector3 look = _player.position - transform.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), Time.deltaTime * 6f);

            if (Time.time - _lastAttackTime >= (Stats != null ? 1f / Stats.AttackRate : 1f))
            {
                _lastAttackTime = Time.time;
                DoAttack();
            }
        }
    }

    private void StartPatrol()
    {
        _state = State.Patrol;
        _agent.isStopped = false;
        _agent.speed = Stats != null ? Stats.WalkSpeed : 2.5f;
        if (PatrolPoints != null && PatrolPoints.Length > 0)
        {
            _patrolIndex = _patrolIndex % PatrolPoints.Length;
            _agent.SetDestination(PatrolPoints[_patrolIndex].position);
        }
    }

    private void PatrolUpdate()
    {
        if (PatrolPoints == null || PatrolPoints.Length == 0) return;
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            _patrolIndex = (_patrolIndex + 1) % PatrolPoints.Length;
            _agent.SetDestination(PatrolPoints[_patrolIndex].position);
        }
    }

    private bool HasLineOfSight()
    {
        if (_player == null) return false;
        Vector3 origin = transform.position + Vector3.up * 1.6f;
        Vector3 target = _player.position + Vector3.up * 1.0f;
        Vector3 dir = (target - origin).normalized;
        if (Physics.Raycast(origin, dir, out RaycastHit hit, Mathf.Infinity))
        {
            return hit.collider.gameObject == _player.gameObject || hit.collider.transform.IsChildOf(_player);
        }
        return false;
    }

    private IEnumerator ReloadRoutine()
    {
        _isReloading = true;
        // optional reload sound
        yield return new WaitForSeconds(2.0f);
        _currentAmmo = Stats != null ? Stats.ammoCount : 6;
        _isReloading = false;
    }

    private void DoAttack()
    {
        if (_player == null) return;

        if (_isReloading) return;
        if (_currentAmmo <= 0)
        {
            StartCoroutine(ReloadRoutine());
            return;
        }

        Vector3 eye = transform.position + Vector3.up * 1.6f;
        Vector3 targetPos = _player.position + Vector3.up * 1.0f;
        Vector3 dir = (targetPos - eye).normalized;

        if (Physics.Raycast(eye, dir, out RaycastHit hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Ignore))
        {
            bool hitPlayer = hit.collider != null && (hit.collider.gameObject == _player.gameObject || hit.collider.transform.IsChildOf(_player));
            if (hitPlayer)
            {
                if (_playerHealth != null)
                    _playerHealth.TakeDamage(Stats.AttackDamage);
                else
                    _player.gameObject.SendMessage("TakeDamage", Stats.AttackDamage, SendMessageOptions.DontRequireReceiver);
            }
        }

        _currentAmmo--;

        if (audioSource != null)
        {
            var pool = (_player != null && Vector3.Distance(transform.position, _player.position) < 10f) ? attackSoundsClose : attackSoundsFar;
            if (pool != null && pool.Length > 0)
                audioSource.PlayOneShot(pool[Random.Range(0, pool.Length)]);
        }
    }

    public void OnDeath()
    {
        _state = State.Dead;
        _agent.isStopped = true;
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        if (audioSource != null && deathSounds != null && deathSounds.Length > 0)
            audioSource.PlayOneShot(deathSounds[Random.Range(0, deathSounds.Length)]);

        float delay = (Stats != null) ? Stats.DeathDelay : 5f;
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
#endregion