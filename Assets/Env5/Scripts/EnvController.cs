using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Env5
{
    public class EnvController : MonoBehaviour
    {
        public Transform player;
        public Transform button;
        public Transform target;
        public Transform goalTrigger;
        public Transform goal;
        public GameObject bridgeDown;
        public GameObject bridgeUp;

        private float groundY = 0f;
        private float elevatedGroundY = 4f;
        private float width = 40f;
        private float bridgeWidth = 3f;

        float part1 = 0.25f;
        float part2 = 0.25f;
        float part3 = 0.25f;
        float part4 = 0.25f;
        float playerScale = 1f;
        float buttonHeight = 0.0002f;
        float buttonScale = 4f;
        float margin = 1f;
        float podiumBredth = 10f;

        float x0;
        float x1;
        float x2;
        float x3;
        float x4;
        float height;

        public float X3 => x3;
        public float X1 => x1;
        public float Width => width;
        public float ElevatedGroundY => elevatedGroundY;
        public float BridgeWidth => bridgeWidth;

        public float PlayerScale { get => playerScale; }

        void Awake()
        {
            Initialize();
        }

        void FixedUpdate()
        {
            if (ButtonPressed())
            {
                bridgeDown.SetActive(true);
                bridgeUp.SetActive(false);
            }
            else
            {
                bridgeDown.SetActive(false);
                bridgeUp.SetActive(true);
            }

            if (GoalPressed())
            {
                Debug.Log("You win!");
            }
            // Debug.Log(button.gameObject.GetComponent<CollisionDetector>().Pressed);
        }

        public bool ButtonPressed()
        {
            // return Vector3.Distance(target.position, button.position) < buttonScale;
            return button.gameObject.GetComponent<CollisionDetector>().Touching(target.gameObject);
        }

        public bool GoalPressed()
        {
            // return Vector3.Distance(goalTrigger.position, goal.position) < buttonScale;
            return goal.gameObject.GetComponent<CollisionDetector>().Touching(goalTrigger.gameObject);
        }

        public float DistancePlayerX1FromRight()
        {
            var distance = player.localPosition.x - x1 - Utility.eps;
            return Mathf.Max(distance, 0);
        }
        public float DistancePlayerX3FromLeft()
        {
            var distance = x3 - player.localPosition.x - Utility.eps;
            return Mathf.Max(distance, 0);
        }
        public bool PlayerRightOfX3()
        {
            return DistancePlayerX3FromLeft() == 0;
        }
        public float DistancePlayerX3FromRight()
        {
            var distance = player.localPosition.x - x3 - Utility.eps;
            return Mathf.Max(distance, 0);
        }
        public bool PlayerLeftOfX3()
        {
            return DistancePlayerX3FromRight() == 0;
        }
        public float DistancePlayerX1FromLeft()
        {
            var distance = x1 - player.localPosition.x - Utility.eps;
            return Mathf.Max(distance, 0);
        }
        public bool PlayerRightOfX1()
        {
            return DistancePlayerX1FromLeft() == 0;
        }
        public float DistancePlayerBridgeFromNorth()
        {
            float bridgeNorthEdge = BridgeWidth / 2;
            var distance = player.localPosition.z - bridgeNorthEdge - Utility.eps;
            return Mathf.Max(distance, 0);
        }
        public float DistancePlayerBridgeFromSouth()
        {
            float bridgeSouthEdge = -BridgeWidth / 2;
            var distance = bridgeSouthEdge - player.localPosition.z - Utility.eps;
            return Mathf.Max(distance, 0);
        }
        public bool PlayerAboveBridge()
        {
            return DistancePlayerBridgeFromSouth() == 0 && DistancePlayerBridgeFromNorth() == 0 && PlayerRightOfX1();
        }
        public float DistancePlayerUp()
        {
            var distance = elevatedGroundY + playerScale / 2f - player.localPosition.y - Utility.eps;
            return Mathf.Max(distance, 0);
        }
        public bool PlayerUp()
        {
            return DistancePlayerUp() == 0;
        }

        public void Initialize()
        {
            x0 = -Width / 2;
            x1 = x0 + Width * part1;
            x2 = x1 + Width * part2;
            x3 = x2 + Width * part3;
            x4 = x3 + Width * part4;
            // Debug.Log($"x0: {x0}, x1: {x1}, x2: {x2}, x3: {x3}, x4: {x4}");
            height = elevatedGroundY - groundY;

            float minX = x0 + margin + playerScale / 2;
            float maxX = x4 - margin - playerScale / 2;
            float maxXTarget = x3 - margin - playerScale / 2;
            float z0 = -Width / 2;
            float z1 = Width / 2;
            float minZ = z0 + margin + playerScale / 2;
            float maxZ = z1 - margin - playerScale / 2;
            float playerY = elevatedGroundY + playerScale / 2;
            float buttonY = elevatedGroundY + buttonHeight / 2;
            // float podiumZ1 = -podiumBredth / 2;
            // float podiumZ2 = podiumBredth / 2;

            player.localPosition = new Vector3(-18, playerY, -2);
            player.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            target.localPosition = new Vector3(-15, playerY, -2);
            target.localRotation = Quaternion.Euler(0, 0, 0);
            goalTrigger.localPosition = new Vector3(-17, playerY, 0);

            button.localPosition = new Vector3(-12, buttonY, -2);

            float buttonMaxX = -8;
            float buttonTiltStartX = -20;
            float buttonSmallMaxZ = -2;
            float buttonSmallMinZ = -6;

            // float buttonX = Random.Range(minX, buttonMaxX);
            // float buttonZ = buttonX < buttonTiltStartX ? Random.Range(minZ, maxZ) : Random.Range(buttonSmallMinZ, buttonSmallMaxZ);
            // button.localPosition = new Vector3(buttonX, elevatedGroundYTarget, buttonZ);
            button.localPosition = new Vector3(Random.Range(x0 + buttonScale / 2, x1 - buttonScale / 2), buttonY, Random.Range(z0 + buttonScale / 2, z1 - buttonScale / 2));
            goal.localPosition = new Vector3(Random.Range(x3 + buttonScale / 2, x4 - buttonScale / 2), buttonY, Random.Range(z0 + buttonScale / 2, z1 - buttonScale / 2));

            player.localPosition = new Vector3(Random.Range(minX, maxX), playerY, Random.Range(minZ, maxZ));
            target.localPosition = new Vector3(Random.Range(minX, maxXTarget), playerY, Random.Range(minZ, maxZ));
            goalTrigger.localPosition = new Vector3(Random.Range(minX, maxXTarget), playerY, Random.Range(minZ, maxZ));
            // target.position = button.position;
            // target.localPosition = Utility.SamplePosition(minX, maxXTarget, minZ, maxZ, playerY, playerY, 2f, new Vector3[] { button.localPosition });

            bridgeDown.SetActive(false);
            bridgeUp.SetActive(true);
        }
        public void Reset()
        {
            var playerController = player.GetComponentInParent<PlayerController>();
            playerController.StopControl();
            // It is manually set to true in PlayerController.StopControl() when the player touches the goal or button for the BT to behave correctly.
            button.GetComponentInParent<CollisionDetector>().ManuallyRemove(target.gameObject.tag);
            goal.GetComponentInParent<CollisionDetector>().ManuallyRemove(goalTrigger.gameObject.tag);
            Initialize();
        }
    }
}
