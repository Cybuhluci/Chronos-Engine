using UnityEngine;
using UnityEngine.AI;

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
    [SerializeField] private MissionManager _missionManager;

    private enum State { Idle, Patrol, Investigate, Chase, Attack, Dead }
    private State _state = State.Idle;

    private enum AlertLevel { none, caution, danger }
    private AlertLevel _alertLevel = AlertLevel.none;

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
            _playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (_playerHealth != null && _player == null) _player = _playerHealth.transform;
        }
    }

    private void Start()
    {
        _missionManager = FindAnyObjectByType<MissionManager>();
        _missionManager = MissionManager.Instance;
    }

    private void Update()
    {
        if (!_health.IsAlive) return;
        if (_player == null) return;

        // Lobotomised behavior: disable both stealth and loud behaviors so the enemy just stands still.
        // Ensure the agent is stopped, clear any investigation/suspicion and stay idle.
        if (_agent != null)
        {
            _agent.isStopped = true;
            _agent.SetDestination(transform.position);
            // try to zero velocity to be safe
            _agent.velocity = Vector3.zero;
        }

        _state = State.Idle;
        _isInvestigating = false;
        return;
    }

    public void OnDeath()
    {
        _state = State.Dead;
        if (audioSource != null && deathSounds.Length > 0)
        {
            AudioClip clip = deathSounds[Random.Range(0, deathSounds.Length)];
            audioSource.PlayOneShot(clip);
        }
    }
}