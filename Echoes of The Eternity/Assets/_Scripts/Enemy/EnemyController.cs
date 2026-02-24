using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyController : MonoBehaviour
{
    public EnemyStatsSO Stats;
    private NavMeshAgent _agent;
    private EnemyHealth _health;
    private Transform _player;

    private enum State { Idle, Patrol, Chase, Attack, Dead }
    private State _state = State.Idle;
    public LayerMask playerLayer = 7;

    [Header("Patrol (optional)")]
    public Transform[] PatrolPoints;
    private int _patrolIndex;

    [Header("Sounds")]
    public AudioSource audioSource; // for playing attack and death sounds where the enemy is located
    public AudioClip[] attackSoundsClose, attackSoundsFar; // sounds of enemy's gun 
    public AudioClip[] deathSounds; // sounds of enemy's death

    private float _lastAttackTime;
    private int currentAmmoCount = 0;
    private bool isReloading = false;
    public PlayerHealth playerHealth; // reference to player's health for direct damage application

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _health = GetComponent<EnemyHealth>();

        if (Stats != null)
        {
            _agent.speed = Stats.WalkSpeed;
            currentAmmoCount = Stats.ammoCount;
        }

        _player = GameObject.FindGameObjectWithTag("Player")?.transform;
        // try a few fallbacks for finding the PlayerHealth component
        if (_player != null)
        {
            playerHealth = _player.GetComponentInChildren<PlayerHealth>();
            if (playerHealth == null)
                playerHealth = _player.GetComponent<PlayerHealth>();
        }
        if (playerHealth == null)
        {
            // fallback: search the scene for any PlayerHealth instance
            playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null && _player == null)
            {
                _player = playerHealth.transform;
            }
        }

        playerLayer = LayerMask.GetMask("Player");
    }

    private void Update()
    {
        if (!_health.IsAlive) return;

        if (_player == null) return;

        float dist = Vector3.Distance(transform.position, _player.position);

        switch (_state)
        {
            case State.Idle:
                if (dist <= (Stats != null ? Stats.DetectionRadius : 10f))
                    StartChase();
                else if (PatrolPoints != null && PatrolPoints.Length > 0)
                    StartPatrol();
                break;

            case State.Patrol:
                PatrolUpdate();
                if (dist <= (Stats != null ? Stats.DetectionRadius : 10f))
                    StartChase();
                break;

            case State.Chase:
                ChaseUpdate();
                if (dist <= (Stats != null ? Stats.AttackRange : 2f) && HasLineOfSight())
                    StartAttack();
                break;

            case State.Attack:
                AttackUpdate(dist);
                break;
        }
    }

    #region State transitions
    private void StartPatrol()
    {
        _state = State.Patrol;
        _agent.isStopped = false;
        _agent.speed = Stats != null ? Stats.WalkSpeed : 2.5f;
        _patrolIndex = 0;
        if (PatrolPoints.Length > 0)
            _agent.SetDestination(PatrolPoints[_patrolIndex].position);
    }

    private void PatrolUpdate()
    {
        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            _patrolIndex = (_patrolIndex + 1) % PatrolPoints.Length;
            _agent.SetDestination(PatrolPoints[_patrolIndex].position);
        }
    }

    private void StartChase()
    {
        _state = State.Chase;
        _agent.isStopped = false;
        _agent.speed = Stats != null ? Stats.RunSpeed : 4.5f;
    }

    private void ChaseUpdate()
    {
        if (_player != null)
            _agent.SetDestination(_player.position);
    }

    private void StartAttack()
    {
        _state = State.Attack;
        _agent.isStopped = true;
    }

    private void AttackUpdate(float distToPlayer)
    {
        // if player moved out of range or LOS, reposition / chase
        if (distToPlayer > (Stats != null ? Stats.AttackRange : 2f) || !HasLineOfSight())
        {
            // try to find a vantage point to get LOS
            FindVantagePointNearPlayer();
            return;
        }

        if (Time.time - _lastAttackTime >= (Stats != null ? 1f / Stats.AttackRate : 1f))
        {
            _lastAttackTime = Time.time;
            DoAttack();
        }
    }
    #endregion

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

    private void FindVantagePointNearPlayer()
    {
        // Simple reposition: move to a point offset from player to attempt to regain LOS
        if (_player == null) return;
        Vector3 dirToPlayer = (_player.position - transform.position).normalized;
        // pick a flank direction (left/right)
        Vector3 right = Vector3.Cross(Vector3.up, dirToPlayer).normalized;
        Vector3[] offsets = new[] { right * 3f, -right * 3f, dirToPlayer * -3f, dirToPlayer * 3f };
        foreach (var off in offsets)
        {
            Vector3 candidate = _player.position + off;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(candidate, out hit, 2.0f, NavMesh.AllAreas))
            {
                _agent.SetDestination(hit.position);
                _agent.isStopped = false;
                return;
            }
        }
        // fallback: chase directly
        StartChase();
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        // play reload sound? using audioSource
        yield return new WaitForSeconds(2.0f);
        currentAmmoCount = Stats != null ? Stats.ammoCount : 6;
        isReloading = false;
    }

    private void DoAttack()
    {
        if (_player == null) return;

        // If out of ammo, simple reload behaviour: wait a short time then reset ammo
        if (isReloading) return;
        if (currentAmmoCount <= 0)
        {
            StartCoroutine(ReloadRoutine());
            return;
        }

        // Aim at player and fire a hitscan bullet
        Vector3 eye = transform.position + Vector3.up * 1.6f;
        Vector3 targetPos = _player.position + Vector3.up * 1.0f;
        Vector3 dir = (targetPos - eye).normalized;

        // Raycast and only apply damage if the ray reaches the player (not blocked)
        if (Physics.Raycast(eye, dir, out RaycastHit hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Ignore))
        {
            bool hitPlayer = false;
            if (_player != null && hit.collider != null)
            {
                if (hit.collider.gameObject == _player.gameObject || hit.collider.transform.IsChildOf(_player))
                    hitPlayer = true;
            }

            if (hitPlayer)
            {
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(Stats.AttackDamage);
                }
                else
                {
                    // fallback: send message to player root
                    _player.gameObject.SendMessage("TakeDamage", Stats.AttackDamage, SendMessageOptions.DontRequireReceiver);
                }
            }
            else
            {
                // blocked by world geometry; consider moving to vantage point next update
                // optionally spawn bullet hole/effect at hit.point
            }
        }

        // decrement ammo
        currentAmmoCount--;

        // play attack sound (choose based on distance)
        if (audioSource != null)
        {
            AudioClip[] pool = (_player != null && Vector3.Distance(transform.position, _player.position) < 10f) ? attackSoundsClose : attackSoundsFar;
            if (pool != null && pool.Length > 0)
            {
                audioSource.PlayOneShot(pool[Random.Range(0, pool.Length)]);
            }
        }
    }

    public void OnDeath()
    {
        // called from EnemyHealth so we can stop nav and switch state
        _state = State.Dead;
        _agent.isStopped = true;
        
        HandleDeath();
    }

    private void HandleDeath()
    {
        // hook for vfx, loot spawn, etc.
    }
}
