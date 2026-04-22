using Photon.Pun;
using UnityEngine;

public class PhotonCharacterAvatar : MonoBehaviourPun, IPunObservable, IPunInstantiateMagicCallback
{
    private const float PositionSmoothing = 15f;
    private const float RotationSmoothing = 12f;

    private Rigidbody2D headRigidbody;
    private PlayerMovement playerMovement;
    private KickController kickController;
    private SimpleAI simpleAI;
    private CharacterSpecialController specialController;

    private Vector2 targetPosition;
    private float targetRotation;
    private RigidbodyType2D originalBodyType;
    private float originalGravityScale;
    private bool hasCachedPhysicsState;
    private bool usePrimarySpawnSlot = true;

    private void Awake()
    {
        if (!GameModeManager.IsOnlineMatch || !PhotonNetwork.InRoom)
        {
            enabled = false;
            return;
        }

        headRigidbody = GetComponentInChildren<Rigidbody2D>(true);
        playerMovement = GetComponentInChildren<PlayerMovement>(true);
        kickController = GetComponent<KickController>();
        if (kickController == null)
            kickController = GetComponentInChildren<KickController>(true);

        simpleAI = GetComponent<SimpleAI>();
        if (simpleAI == null)
            simpleAI = GetComponentInChildren<SimpleAI>(true);

        specialController = GetComponent<CharacterSpecialController>();
        if (specialController == null)
            specialController = GetComponentInChildren<CharacterSpecialController>(true);

        if (headRigidbody != null)
        {
            originalBodyType = headRigidbody.bodyType;
            originalGravityScale = headRigidbody.gravityScale;
            hasCachedPhysicsState = true;
            targetPosition = headRigidbody.position;
            targetRotation = headRigidbody.rotation;
        }
    }

    private void Start()
    {
        if (!GameModeManager.IsOnlineMatch || !PhotonNetwork.InRoom)
        {
            return;
        }

        ApplySpawnFacing();
        ApplyOwnershipState();
    }

    private void FixedUpdate()
    {
        if (!GameModeManager.IsOnlineMatch || !PhotonNetwork.InRoom || photonView.IsMine || headRigidbody == null)
        {
            return;
        }

        Vector2 nextPosition = Vector2.Lerp(headRigidbody.position, targetPosition, Time.deltaTime * PositionSmoothing);
        float nextRotation = Mathf.LerpAngle(headRigidbody.rotation, targetRotation, Time.deltaTime * RotationSmoothing);

        headRigidbody.MovePosition(nextPosition);
        headRigidbody.MoveRotation(nextRotation);
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (!GameModeManager.IsOnlineMatch || !PhotonNetwork.InRoom)
        {
            return;
        }

        object[] instantiationData = info.photonView.InstantiationData;
        if (instantiationData != null &&
            instantiationData.Length > 0 &&
            instantiationData[0] is int spawnSlot)
        {
            usePrimarySpawnSlot = spawnSlot == 0;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (!GameModeManager.IsOnlineMatch || !PhotonNetwork.InRoom || headRigidbody == null)
        {
            return;
        }

        if (stream.IsWriting)
        {
            stream.SendNext(headRigidbody.position);
            stream.SendNext(headRigidbody.rotation);
        }
        else
        {
            targetPosition = (Vector2)stream.ReceiveNext();
            targetRotation = (float)stream.ReceiveNext();
        }
    }

    private void ApplyOwnershipState()
    {
        bool isLocalAvatar = photonView.IsMine;

        if (playerMovement != null)
        {
            playerMovement.isPlayer = isLocalAvatar;
            playerMovement.enabled = true;
        }

        if (kickController != null)
        {
            kickController.isPlayer = isLocalAvatar;
            kickController.enabled = true;
        }

        if (simpleAI != null)
        {
            simpleAI.isAI = false;
            simpleAI.enabled = false;
        }

        if (specialController != null)
        {
            specialController.Configure(isLocalAvatar);
            specialController.enabled = isLocalAvatar;
        }

        if (!hasCachedPhysicsState || headRigidbody == null)
        {
            return;
        }

        if (isLocalAvatar)
        {
            headRigidbody.bodyType = originalBodyType;
            headRigidbody.gravityScale = originalGravityScale;
        }
        else
        {
            headRigidbody.linearVelocity = Vector2.zero;
            headRigidbody.angularVelocity = 0f;
            headRigidbody.bodyType = RigidbodyType2D.Kinematic;
            headRigidbody.gravityScale = 0f;
        }
    }

    private void ApplySpawnFacing()
    {
        Vector3 localScale = transform.localScale;
        float absoluteX = Mathf.Abs(localScale.x);
        localScale.x = usePrimarySpawnSlot ? absoluteX : -absoluteX;
        transform.localScale = localScale;
    }
}
