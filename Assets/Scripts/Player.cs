// Authored by: Finn Davis
// Added to by: Harley Clark
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.VFX;
using Random = UnityEngine.Random;
using System.Collections;
using System;
using UnityEngine.InputSystem.UI;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    #region Fields
    [Header("Setup")]
    public GameObject root;
    public Animator targetAnimator;
    [SerializeField] private Animator ikHandAnimator;
    public SkinnedMeshRenderer bodyMesh;
    public GameObject head;

    [HideInInspector] public ConfigurableJoint hipJoint;
    [HideInInspector] public Rigidbody hipRB;

    // Input
    [HideInInspector] public PlayerInput playerInput;
    [HideInInspector] public InputActionAsset inputActionAsset;
    private InputActionMap playerActionMap;
    private InputAction movement;

    [Header("Rotation")]
    public Transform orientation;
    public float rotationSpeed;
    Vector3 targetRotation;

    [Header("Player")]
    public int startingHealth = 100;
    public int startingLives = 3;
    public int currentHealth;
    public int currentLives;
    public float respawnTime = 5.0f;
    public float invincTime;
    public bool invincibool;
    public bool targetable;

    [Header("Movement")]
    [SerializeField] float acceleration = 10f;
    [SerializeField] float speedLimit = 10f;
    [SerializeField] float jumpHeight = 5f;
    [SerializeField] float stepThreshold = 0.1f;
    Vector3 moveDirection;
    bool walking = false;

    [Header("Jumping")]
    public float JumpCooldown;
    bool allreadyJumped = false;
    bool jumping = false;

    [Header("Dash")]
    [SerializeField] float dashStrength = 20f;
    [SerializeField] float dashDuration = 1f;
    [SerializeField] float dashCooldown = 10f;
    [HideInInspector] public float oneToZero;
    float currentDashCooldown;
    bool dashing = false;
    bool readyToDash = true;

    [Header("Push")]
    [SerializeField] public float pushStrength = 15f;
    [SerializeField] float pushDuration = 0.5f;
    [SerializeField] float pushCooldown = 2f;
    public bool pushing = false;
    bool readyToPush = true;

    [Header("Block")]
    [Range(0f, 1f)] public float blockStrength = 1f;
    [SerializeField] float blockCooldown = 4f;
    [HideInInspector] public bool blocking = false;
    bool readyToBlock = true;

    [Header("Chumpkin Launching")]
    public float launchStrength = 200;
    [Range(0, 360)] public float launchAngle = 15;

    [Header("Extras")]
    Camera m_camera;
    private ConfigurableJointMotion[,] jointRagdollStates;
    JointDrive[] storedJointDrives;
    Vector3 hipUpwardsForce;
    ConstantForce cf;
    bool isRagdoll = false;
    public GameObject decaObj;

    public event Action shaker;
    #endregion

    #region VisualEffects
    [Header("VFX")]
    public ParticleSystem damagedEffect;
    public ParticleSystem finalDeathEffect;

    [System.Serializable]
    public class VFX
    {
        public string name;
        public VisualEffect visualEffect;
    }
    public List<VFX> visualEffects;
    public VisualEffect GetEffect(string name)
    {
        foreach (VFX vFX in visualEffects) if (vFX.name == name) return vFX.visualEffect;
        return null;
    }
    bool onFireEffectIsPlaying = false;
    [HideInInspector] public bool onFire = false;
    #endregion

    #region Ground Check
    public float TimeOfLastGrounded
    {
        set;
        private get;
    }
    private bool grounded
    {
        get
        {
            return (Time.time - TimeOfLastGrounded) <= stepThreshold;
        }
    }
    #endregion

    #region UnityMessages
    private void Awake()
    {
        hipRB = root.GetComponent<Rigidbody>();
        hipJoint = root.GetComponent<ConfigurableJoint>();

        cf = hipRB.gameObject.GetComponent<ConstantForce>();
        hipUpwardsForce = cf.force;

        m_camera = PlayerManager.Instance.m_camera.GetComponent<Camera>();

        // Input
        playerInput = GetComponent<PlayerInput>();
        inputActionAsset = playerInput.actions;
        playerActionMap = inputActionAsset.FindActionMap("Player");

        currentHealth = startingHealth;
        currentLives = startingLives;

        gameObject.layer = LayerMask.NameToLayer("Player1") + playerInput.playerIndex;
        foreach (Collider c in gameObject.GetComponentsInChildren<Collider>())
        {
            if (c.gameObject.CompareTag("HitBox")) return;
            c.gameObject.tag = "Player";
            c.gameObject.layer = gameObject.layer;
        }
    }
    private void OnEnable()
    {
        playerActionMap.FindAction("Jump").started += DoJump;
        playerActionMap.FindAction("Attack").started += DoAttack;
        playerActionMap.FindAction("Block").started += DoBlock;
        playerActionMap.FindAction("Block").canceled += FinishBlock;
        playerActionMap.FindAction("OHSHIT").started += DoDash;
        playerActionMap.FindAction("Pause").started += PauseGame;
        movement = playerActionMap.FindAction("Movement");
        StoreAngularLocks();
    }
    private void OnDisable()
    {
        playerActionMap.FindAction("Jump").started -= DoJump;
        playerActionMap.FindAction("Attack").started -= DoAttack;
        playerActionMap.FindAction("Block").started -= DoBlock;
        playerActionMap.FindAction("Block").canceled -= FinishBlock;
        playerActionMap.FindAction("OHSHIT").started -= DoDash;
        playerActionMap.FindAction("Pause").started -= PauseGame;
    }
    private void Update()
    {
        Vector3 cameraRelativeMoveDirection = Vector3.zero;
        cameraRelativeMoveDirection += movement.ReadValue<Vector2>().x * GetCameraRight(m_camera);
        cameraRelativeMoveDirection += movement.ReadValue<Vector2>().y * GetCameraForward(m_camera);
        moveDirection = cameraRelativeMoveDirection;

        orientation.position = new Vector3(hipJoint.transform.position.x, 0f, hipJoint.transform.position.z);

        if (currentDashCooldown > 0)
        {
            currentDashCooldown -= Time.deltaTime;
            oneToZero = Mathf.Lerp(0, 1, currentDashCooldown / dashCooldown);
        }

        if (grounded && !isRagdoll) cf.force = hipUpwardsForce;
        else cf.force = new Vector3(0, 0, 0);

        Effects();

        bodyMesh.SetBlendShapeWeight(0, 100 - currentHealth);
        targetAnimator.SetBool("Walk", walking);
        targetAnimator.SetBool("Blocking", blocking);
    }
    private void FixedUpdate()
    {
        if (!dashing) LimitSpeed();
        MovePlayer(moveDirection);

        if (jumping && !allreadyJumped)
        {
            allreadyJumped = true;
            hipRB.velocity = Vector3.zero;
            hipRB.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        }
    }
    #endregion

    #region InputEvents
    private void DoJump(InputAction.CallbackContext obj)
    {
        if (!grounded || dashing || jumping) return;

        jumping = true;
        GetEffect("Jump").Play();
        AudioManager.Instance.PlaySound("Chumpkin Jump");

        Invoke(nameof(ResetJump), JumpCooldown);
    }
    private void DoAttack(InputAction.CallbackContext obj)
    {
        if (!readyToPush) return;

        pushing = true;
        readyToPush = false;

        ikHandAnimator.SetTrigger("push");
        AudioManager.Instance.PlaySound("Chumpkin Pushes");

        Invoke(nameof(StopPushing), pushDuration);
        Invoke(nameof(ResetPush), pushCooldown);
    }
    private void DoBlock(InputAction.CallbackContext obj) { if (readyToBlock) blocking = true; }
    private void FinishBlock(InputAction.CallbackContext obj) => blocking = false;
    private void DoDash(InputAction.CallbackContext obj)
    {
        if (dashing || !readyToDash) return;

        if (blocking) blocking = false;

        currentDashCooldown = dashCooldown;
        readyToDash = false;
        dashing = true;

        AudioManager.Instance.PlaySound("Chumpkin Dash " + (playerInput.playerIndex + 1).ToString());

        hipRB.velocity = Vector3.zero;
        Vector3 ohShitDir;
        if (moveDirection.magnitude >= 0.1f) ohShitDir = moveDirection;
        else ohShitDir = orientation.forward;

        hipRB.AddForce(ohShitDir.normalized * dashStrength, ForceMode.Impulse);

        Invoke(nameof(StopOhShitting), dashDuration);
        Invoke(nameof(ResetOhShit), dashCooldown);
    }
    private void PauseGame(InputAction.CallbackContext obj)
    {
        var gm = GameManager.Instance;
        if (gm.State == GameManager.GameState.GameState)
        {
            gm.UpdateGameState(GameManager.GameState.Paused);
            FindObjectOfType<InputSystemUIInputModule>().actionsAsset = inputActionAsset;
        }
        else if (gm.State == GameManager.GameState.Paused) gm.UpdateGameState(GameManager.GameState.GameState);
    }
    #endregion

    #region PlayerMovement
    private void MovePlayer(Vector3 movementDirection)
    {
        if (movementDirection.magnitude >= 0.1f && !blocking)
        {
            if (GameManager.Instance.State == GameManager.GameState.EndGame)
            {
                targetRotation = PlayerManager.Instance.winPodium[playerInput.playerIndex].rotation.eulerAngles;
            }
            else
            {
                targetRotation = new Vector3(movementDirection.x, 0f, movementDirection.z);
            }

            orientation.rotation = Quaternion.RotateTowards(orientation.rotation, Quaternion.LookRotation(targetRotation), rotationSpeed);
            hipJoint.targetRotation = Quaternion.LookRotation(Vector3.Reflect(orientation.forward, Vector3.right));

            hipRB.AddForce(hipJoint.transform.forward * acceleration, ForceMode.Force);

            walking = true;
        }
        else walking = false;
    }
    private void LimitSpeed()
    {
        Vector3 flatVel = new Vector3(hipRB.velocity.x, 0f, hipRB.velocity.z);

        if (flatVel.magnitude > speedLimit)
        {
            Vector3 limitedVel = flatVel.normalized * speedLimit;
            hipRB.velocity = new Vector3(limitedVel.x, hipRB.velocity.y, limitedVel.z);
        }
    }
    #endregion

    #region ExtraFunctions
    public void TakeDamage(int dmg)
    {
        if (invincibool) return;

        damagedEffect.Play();
        targetAnimator.SetTrigger("Hurt");
        currentHealth -= dmg;

        if (currentHealth <= 0)
        {
            currentLives--;
            if (currentLives == 0)
            {
                StartCoroutine(LostLastLife());
                PlayerManager.Instance.GameOverCheck(this, playerInput.playerIndex);
                return;
            }
            else
            {
                AudioManager.Instance.PlaySound("Chumpkin Death");
            }
            LostALife();
        }
        shaker?.Invoke();
    }
    public void LostALife()
    {
        decaObj.SetActive(false);
        invincibool = true;
        SetRagdoll();
        LaunchChumpkin();
        Invoke(nameof(SetAnimated), respawnTime);
        Invoke(nameof(Respawn), respawnTime);
    }
    IEnumerator LostLastLife()
    {
        currentHealth = 0;
        invincibool = true;
        decaObj.SetActive(false);
        targetAnimator.SetTrigger("Explode");
        AudioManager.Instance.PlaySound("Chumpkin Final Death");
        yield return new WaitForSeconds(1);
        SetRagdoll();
        SetPosAndRot(PlayerManager.Instance.winPodium[PlayerManager.Instance.deathOrder.Count]);
    }
    public void PushBlocked()
    {
        blocking = false;
        readyToBlock = false;

        Invoke(nameof(ResetBlock), blockCooldown);

        AudioManager.Instance.PlaySound("Chumpkin Push Blocked");
    }
    public void OnFootDown() => AudioManager.Instance.PlaySound("Chumpkin Footstep");
    public void ExplodeParticles() => finalDeathEffect.Play();
    public void SetDecalColor(Material color) => decaObj.GetComponent<MeshRenderer>().material = color;
    void StoreAngularLocks()
    {
        ConfigurableJoint[] cjs = GetComponentsInChildren<ConfigurableJoint>();
        jointRagdollStates = new ConfigurableJointMotion[cjs.Length, 3];
        storedJointDrives = new JointDrive[cjs.Length];
        for (int i = 0; i < cjs.Length; ++i)
        {
            storedJointDrives[i] = cjs[i].angularXDrive;
            jointRagdollStates[i, 0] = cjs[i].angularXMotion;
            jointRagdollStates[i, 1] = cjs[i].angularYMotion;
            jointRagdollStates[i, 2] = cjs[i].angularZMotion;
            cjs[i].angularXMotion = ConfigurableJointMotion.Free;
            cjs[i].angularYMotion = ConfigurableJointMotion.Free;
            cjs[i].angularZMotion = ConfigurableJointMotion.Free;
        }
    }
    void LaunchChumpkin()
    {
        Vector3 puntDirection = Quaternion.Euler(launchAngle, 360f * Random.value, 0) * Vector3.up;
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            rb.AddForce(puntDirection * launchStrength, ForceMode.Impulse);
        }
    }
    public void SetAnimated()
    {
        isRagdoll = false;
        ConfigurableJoint[] cjs = GetComponentsInChildren<ConfigurableJoint>();
        for (int i = 0; i < cjs.Length; ++i)
        {
            cjs[i].angularXDrive = storedJointDrives[i];
            cjs[i].angularYZDrive = storedJointDrives[i];
            cjs[i].angularXMotion = ConfigurableJointMotion.Free;
            cjs[i].angularYMotion = ConfigurableJointMotion.Free;
            cjs[i].angularZMotion = ConfigurableJointMotion.Free;
        }
    }
    public void SetRagdoll()
    {
        isRagdoll = true;
        playerInput.actions.FindActionMap("Player").Disable();
        targetable = false;

        JointDrive newJointDrive = new JointDrive();
        newJointDrive.positionSpring = 0;

        ConfigurableJoint[] cjs = GetComponentsInChildren<ConfigurableJoint>();
        for (int i = 0; i < cjs.Length; ++i)
        {
            cjs[i].angularXDrive = newJointDrive;
            cjs[i].angularYZDrive = newJointDrive;
            cjs[i].angularXMotion = jointRagdollStates[i, 0];
            cjs[i].angularYMotion = jointRagdollStates[i, 1];
            cjs[i].angularZMotion = jointRagdollStates[i, 2];
        }
    }
    void Effects()
    {
        if (onFire)
        {
            if (!onFireEffectIsPlaying) { GetEffect("OnFire").Play(); onFireEffectIsPlaying = true; }
        }
        else
        {
            onFireEffectIsPlaying = false;
            GetEffect("OnFire").Stop();
        }
        if (oneToZero > 0)
        {
            PlayerManager.Instance.decalMaterials[playerInput.playerIndex].SetFloat("_Cooldown", oneToZero);
        }
        else
        {
            PlayerManager.Instance.decalMaterials[playerInput.playerIndex].SetFloat("_Cooldown", 0);
        }
    }
    void Respawn()
    {
        currentHealth = startingHealth;
        onFire = false;
        decaObj.SetActive(true);
        TrapLayout tl = GameManager.Instance.traps.GetComponentInParent<TrapLayout>();
        Transform spawnPoint = tl.spawnPoints[Random.Range(0, tl.spawnPoints.Count)];

        targetable = true;

        ZeroRBVelocity();

        SetPosAndRot(spawnPoint);

        Invoke(nameof(InvincibilityOff), invincTime);
        playerInput.actions.FindActionMap("Player").Enable();

    }
    public void SetPosAndRot(Transform _transform)
    {
        hipRB.transform.position = _transform.position;
        orientation.rotation = _transform.rotation;
        hipJoint.targetRotation = Quaternion.LookRotation(Vector3.Reflect(orientation.forward, Vector3.right));
    }
    public void ZeroRBVelocity()
    {
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>()) rb.velocity = Vector3.zero;
    }
    Vector3 GetCameraForward(Camera camera)
    {
        Vector3 forward = camera.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }
    Vector3 GetCameraRight(Camera camera)
    {
        Vector3 right = camera.transform.right;
        right.y = 0;
        return right.normalized;
    }
    #endregion

    #region ResetFunctions
    private void StopPushing() => pushing = false;
    private void ResetOhShit() => readyToDash = true;
    private void ResetPush() => readyToPush = true;
    private void ResetBlock() => readyToBlock = true;
    private void InvincibilityOff() => invincibool = false;
    private void StopOhShitting() => dashing = false;
    private void ResetJump()
    {
        jumping = false;
        allreadyJumped = false;
    }
    public void CollideWithOtherPlayers(bool _bool)
    {
        for (int i = 0; i < PlayerManager.Instance.playerConfigs.Count; i++)
        {
            if (LayerMask.NameToLayer("Player1") + i != gameObject.layer)
            {
                Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Player1") + i, !_bool);
            }
        }
    }
    #endregion
}
