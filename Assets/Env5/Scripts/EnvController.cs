using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Env5
{
    public class EnvController : MonoBehaviour
    {
        public Transform player;
        public Transform button1;
        public Transform trigger1;
        public Transform trigger2;
        public Transform button2;
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

        float x0;
        float x1;
        float x2;
        float x3;
        float x4;
        float height;
        float bridgeZ;

        public float X3 => x3;
        public float X1 => x1;
        public float Width => width;
        public float ElevatedGroundY => elevatedGroundY;
        public float BridgeWidth => bridgeWidth;

        public float PlayerScale { get => playerScale; }
        public float BridgeZ => bridgeZ;

        void Awake()
        {
            Initialize();
        }

        void FixedUpdate()
        {
            if (Button1Pressed())
            {
                bridgeDown.SetActive(true);
                bridgeUp.SetActive(false);
            }
            else
            {
                bridgeDown.SetActive(false);
                bridgeUp.SetActive(true);
            }

            if (button2Pressed())
            {
                Debug.Log("You win!");
            }
        }

        public bool Button1Pressed()
        {
            return button1.gameObject.GetComponent<CollisionDetector>().Touching(trigger1.gameObject);
        }

        public bool button2Pressed()
        {
            return button2.gameObject.GetComponent<CollisionDetector>().Touching(trigger2.gameObject);
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
            float bridgeNorthEdge = bridgeDown.transform.localPosition.z + BridgeWidth / 2;
            var distance = player.localPosition.z - bridgeNorthEdge - Utility.eps;
            return Mathf.Max(distance, 0);
        }
        public float DistancePlayerBridgeFromSouth()
        {
            float bridgeSouthEdge = bridgeDown.transform.localPosition.z - BridgeWidth / 2;
            var distance = bridgeSouthEdge - player.localPosition.z - Utility.eps;
            return Mathf.Max(distance, 0);
        }
        public bool PlayerOnBridge()
        {
            return DistancePlayerBridgeFromSouth() == 0 && DistancePlayerBridgeFromNorth() == 0 && PlayerRightOfX1() && PlayerUp();
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
            height = elevatedGroundY - groundY;

            float minX = x0 + margin + playerScale / 2;
            float maxX = x4 - margin - playerScale / 2;
            float maxXTrigger1 = x3 - margin - playerScale / 2;
            float z0 = -Width / 2;
            float z1 = Width / 2;
            float minZ = z0 + margin + playerScale / 2;
            float maxZ = z1 - margin - playerScale / 2;
            float playerY = elevatedGroundY + playerScale / 2;
            float buttonY = elevatedGroundY + buttonHeight / 2;

            player.localPosition = new Vector3(-18, playerY, -2);
            player.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            trigger1.localPosition = new Vector3(-15, playerY, -2);
            trigger1.localRotation = Quaternion.Euler(0, 0, 0);
            trigger2.localPosition = new Vector3(-17, playerY, 0);

            button1.localPosition = new Vector3(-12, buttonY, -2);

            // float buttonMaxX = -8;
            // float buttonTiltStartX = -20;
            // float buttonSmallMaxZ = -2;
            // float buttonSmallMinZ = -6;

            button1.localPosition = new Vector3(Random.Range(x0 + buttonScale / 2, x1 - buttonScale / 2), buttonY, Random.Range(z0 + buttonScale / 2, z1 - buttonScale / 2));
            button2.localPosition = new Vector3(Random.Range(x3 + buttonScale / 2, x4 - buttonScale / 2), buttonY, Random.Range(z0 + buttonScale / 2, z1 - buttonScale / 2));

            player.localPosition = new Vector3(Random.Range(minX, maxX), playerY, Random.Range(minZ, maxZ));
            trigger1.localPosition = new Vector3(Random.Range(minX, maxXTrigger1), playerY, Random.Range(minZ, maxZ));
            trigger2.localPosition = new Vector3(Random.Range(minX, maxXTrigger1), playerY, Random.Range(minZ, maxZ));

            bridgeZ = Random.Range(z0 + bridgeWidth / 2, z1 - bridgeWidth / 2);
            var bridgeDownY = 3.95f;
            var bridgeUpY = 11.08f;
            var bridgeUpX = 2.91f;
            bridgeDown.transform.localPosition = new Vector3(x2, bridgeDownY, bridgeZ);
            bridgeUp.transform.localPosition = new Vector3(bridgeUpX, bridgeUpY, bridgeZ);
            // bridgeDown.transform.scale = new Vector3(x3 - x1, bridgeDown.transform.scale.y, bridgeWidth);
            // bridgeUp.transform.scale = new Vector3(x3 - x1, bridgeUp.transform.scale.y, bridgeWidth);

            bridgeDown.SetActive(false);
            bridgeUp.SetActive(true);
        }
        public void Reset()
        {
            var playerController = player.GetComponentInParent<PlayerController>();
            playerController.StopControl();
            // It is manually set to true in PlayerController.StopControl() when the player touches button2 or button1 for the BT to behave correctly.
            button1.GetComponentInParent<CollisionDetector>().ManuallyRemove(trigger1.gameObject.tag);
            button2.GetComponentInParent<CollisionDetector>().ManuallyRemove(trigger2.gameObject.tag);
            Initialize();
        }
    }
}
