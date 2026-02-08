using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Dynamic AI enemy that hunts the player based on noise, light, and movement
/// Attach to enemy object with NavMeshAgent component
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("AI States")]
    public enum AIState { Patrolling, Investigating, Hunting, Searching }
    [SerializeField] private AIState currentState = AIState.Patrolling;
    
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float fieldOfView = 120f;
    [SerializeField] private float hearingRadius = 20f;
    [SerializeField] private float lightDetectionBonus = 5f;
    [SerializeField] private LayerMask obstacleMask;
    
    [Header("Movement Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float investigateSpeed = 3.5f;
    [SerializeField] private float huntSpeed = 5.5f;
    [SerializeField] private float patrolWaitTime = 3f;
    
    [Header("Hunt Settings")]
    [SerializeField] private float huntDuration = 15f;
    [SerializeField] private float searchDuration = 10f;
    [SerializeField] private float losePlayerTime = 5f;
    [SerializeField] private float attackRange = 2f;
    
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private bool randomPatrol = true;
    
    [Header("Audio")]
    [SerializeField] private AudioSource enemyAudio;
    [SerializeField] private AudioClip detectionSound;
    [SerializeField] private AudioClip huntingSound;
    [SerializeField] private AudioClip ambientBreathing;
    [SerializeField] private AudioClip attackSound;
    
    [Header("Visual")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material huntingMaterial;
    [SerializeField] private Renderer enemyRenderer;
    [SerializeField] private Light enemyLight;
    
    // Private variables
    private NavMeshAgent navAgent;
    private PlayerController player;
    private SanitySystem playerSanity;
    private Vector3 lastKnownPlayerPosition;
    private int currentPatrolIndex = 0;
    private float stateTimer = 0f;
    private float lastSeenTimer = 0f;
    private bool playerInSight = false;
    private bool playerDetected = false;
    private List<Vector3> searchPoints = new List<Vector3>();
    private int currentSearchPoint = 0;
    
    // Public accessors
    public AIState CurrentState => currentState;
    public bool IsHunting => currentState == AIState.Hunting;
    
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        player = FindObjectOfType<PlayerController>();
        playerSanity = player.GetComponent<SanitySystem>();
        
        // Generate patrol points if none assigned
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            GeneratePatrolPoints();
        }
        
        // Start patrolling
        SetState(AIState.Patrolling);
        
        // Play ambient breathing
        if (enemyAudio != null && ambientBreathing != null)
        {
            enemyAudio.clip = ambientBreathing;
            enemyAudio.loop = true;
            enemyAudio.Play();
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Update detection
        DetectPlayer();
        
        // Update state behavior
        switch (currentState)
        {
            case AIState.Patrolling:
                Patrol();
                break;
            case AIState.Investigating:
                Investigate();
                break;
            case AIState.Hunting:
                Hunt();
                break;
            case AIState.Searching:
                Search();
                break;
        }
        
        // Update timers
        stateTimer += Time.deltaTime;
        if (!playerInSight)
        {
            lastSeenTimer += Time.deltaTime;
        }
        else
        {
            lastSeenTimer = 0f;
        }
        
        // Check for player catch
        float distanceToPlayer = Vector3.Distance(transform.position, player.Position);
        if (distanceToPlayer < attackRange && !player.IsHiding)
        {
            AttackPlayer();
        }
    }
    
    void DetectPlayer()
    {
        if (player.IsHiding)
        {
            playerInSight = false;
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.Position);
        Vector3 directionToPlayer = (player.Position - transform.position).normalized;
        
        // Visual detection
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        bool inFOV = angle < fieldOfView / 2f;
        
        float effectiveDetectionRadius = detectionRadius;
        if (player.IsFlashlightOn)
        {
            effectiveDetectionRadius += lightDetectionBonus;
        }
        
        if (distanceToPlayer < effectiveDetectionRadius && inFOV)
        {
            // Check line of sight
            if (!Physics.Raycast(transform.position + Vector3.up, directionToPlayer, distanceToPlayer, obstacleMask))
            {
                playerInSight = true;
                lastKnownPlayerPosition = player.Position;
                
                if (!playerDetected)
                {
                    OnPlayerDetected();
                }
                
                playerDetected = true;
            }
            else
            {
                playerInSight = false;
            }
        }
        else
        {
            playerInSight = false;
        }
        
        // Audio detection (hearing)
        if (distanceToPlayer < hearingRadius)
        {
            float noiseThreshold = 0.4f;
            
            if (player.CurrentNoiseLevel > noiseThreshold)
            {
                lastKnownPlayerPosition = player.Position;
                
                if (currentState == AIState.Patrolling)
                {
                    SetState(AIState.Investigating);
                }
            }
        }
        
        // Lose player if not seen for too long
        if (lastSeenTimer > losePlayerTime && currentState == AIState.Hunting)
        {
            SetState(AIState.Searching);
            GenerateSearchPoints(lastKnownPlayerPosition);
        }
    }
    
    void Patrol()
    {
        navAgent.speed = patrolSpeed;
        
        if (patrolPoints.Length == 0) return;
        
        // Move to patrol point
        if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
        {
            if (stateTimer > patrolWaitTime)
            {
                // Move to next patrol point
                if (randomPatrol)
                {
                    currentPatrolIndex = Random.Range(0, patrolPoints.Length);
                }
                else
                {
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                }
                
                navAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
                stateTimer = 0f;
            }
        }
    }
    
    void Investigate()
    {
        navAgent.speed = investigateSpeed;
        navAgent.SetDestination(lastKnownPlayerPosition);
        
        // If we see the player, start hunting
        if (playerInSight)
        {
            SetState(AIState.Hunting);
        }
        // If we reach the investigation point and don't find anything
        else if (!navAgent.pathPending && navAgent.remainingDistance < 1f)
        {
            if (stateTimer > 3f)
            {
                SetState(AIState.Patrolling);
            }
        }
    }
    
    void Hunt()
    {
        navAgent.speed = huntSpeed;
        
        if (playerInSight)
        {
            navAgent.SetDestination(player.Position);
            lastKnownPlayerPosition = player.Position;
            
            // Affect player sanity
            if (playerSanity != null)
            {
                playerSanity.AddStress(5f * Time.deltaTime);
            }
        }
        else
        {
            navAgent.SetDestination(lastKnownPlayerPosition);
        }
    }
    
    void Search()
    {
        navAgent.speed = investigateSpeed;
        
        if (playerInSight)
        {
            SetState(AIState.Hunting);
            return;
        }
        
        if (searchPoints.Count > 0)
        {
            if (!navAgent.pathPending && navAgent.remainingDistance < 1f)
            {
                currentSearchPoint++;
                
                if (currentSearchPoint >= searchPoints.Count)
                {
                    SetState(AIState.Patrolling);
                }
                else
                {
                    navAgent.SetDestination(searchPoints[currentSearchPoint]);
                }
            }
        }
        else if (stateTimer > searchDuration)
        {
            SetState(AIState.Patrolling);
        }
    }
    
    void SetState(AIState newState)
    {
        currentState = newState;
        stateTimer = 0f;
        
        // Visual feedback
        if (enemyRenderer != null)
        {
            if (currentState == AIState.Hunting)
            {
                enemyRenderer.material = huntingMaterial;
                if (enemyLight != null)
                {
                    enemyLight.color = Color.red;
                    enemyLight.intensity = 2f;
                }
            }
            else
            {
                enemyRenderer.material = normalMaterial;
                if (enemyLight != null)
                {
                    enemyLight.color = Color.yellow;
                    enemyLight.intensity = 1f;
                }
            }
        }
        
        // Audio feedback
        if (currentState == AIState.Hunting && enemyAudio != null && huntingSound != null)
        {
            enemyAudio.PlayOneShot(huntingSound);
        }
    }
    
    void OnPlayerDetected()
    {
        if (enemyAudio != null && detectionSound != null)
        {
            enemyAudio.PlayOneShot(detectionSound);
        }
        
        SetState(AIState.Hunting);
    }
    
    void GenerateSearchPoints(Vector3 centerPoint)
    {
        searchPoints.Clear();
        currentSearchPoint = 0;
        
        // Generate random search points around last known position
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomPoint = centerPoint + Random.insideUnitSphere * 10f;
            randomPoint.y = centerPoint.y;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 10f, NavMesh.AllAreas))
            {
                searchPoints.Add(hit.position);
            }
        }
        
        if (searchPoints.Count > 0)
        {
            navAgent.SetDestination(searchPoints[0]);
        }
    }
    
    void GeneratePatrolPoints()
    {
        // Generate random patrol points on the NavMesh
        List<Transform> points = new List<Transform>();
        
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * 30f;
            randomPoint.y = transform.position.y;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 30f, NavMesh.AllAreas))
            {
                GameObject pointObj = new GameObject("PatrolPoint_" + i);
                pointObj.transform.position = hit.position;
                pointObj.transform.parent = transform.parent;
                points.Add(pointObj.transform);
            }
        }
        
        patrolPoints = points.ToArray();
    }
    
    void AttackPlayer()
    {
        if (enemyAudio != null && attackSound != null)
        {
            enemyAudio.PlayOneShot(attackSound);
        }
        
        // Trigger game over
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.GameOver(false);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Draw hearing radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, hearingRadius);
        
        // Draw FOV
        Gizmos.color = Color.red;
        Vector3 fovLine1 = Quaternion.AngleAxis(fieldOfView / 2f, transform.up) * transform.forward * detectionRadius;
        Vector3 fovLine2 = Quaternion.AngleAxis(-fieldOfView / 2f, transform.up) * transform.forward * detectionRadius;
        
        Gizmos.DrawLine(transform.position, transform.position + fovLine1);
        Gizmos.DrawLine(transform.position, transform.position + fovLine2);
    }
}
