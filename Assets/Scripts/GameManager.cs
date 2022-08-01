using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WildSkies
{
    public class GameManager : MonoBehaviour
    {
        public int maxHeight = 15;
        public int maxWidth = 17;

        public Color color1;
        public Color color2;
        public Color appleColor = Color.red;
        public Color playerColor;

        public Transform cameraHolder;

        GameObject playerObj;
        GameObject appleObj;
        GameObject tailObj;
        Node playerNode;
        Node appleNode;
        Sprite playerSprite;

        GameObject mapObject;
        SpriteRenderer mapRenderer;

        Node[,] grid;
        List<Node> availableNodes = new List<Node>();
        List<SpecialNode> tailNode = new List<SpecialNode>();

        bool up,left,right,down;

        public float moveRate = 0.5f;
        float timer;

        Direction curDirection;
        public enum Direction
        {
            up,left,right,down
        }

        #region INIT
        void Start()
        {
            CreateMap();
            PlacePlayer();
            PlaceCamera();
            CreateApple();
            curDirection = Direction.right;
        }

        void CreateMap()
        {
            mapObject = new GameObject("Map");
            mapRenderer = mapObject.AddComponent<SpriteRenderer>();

            grid = new Node[maxWidth, maxHeight];

            Texture2D texture2D = new Texture2D(maxWidth, maxHeight);
            for(int x = 0; x < maxWidth; x++)
            {
                for(int y = 0; y < maxHeight; y++)
                {
                    Vector3 tp = Vector3.zero;
                    tp.x = x;
                    tp.y = y;
                    Node n = new Node()
                    {
                        x = x,
                        y = y,
                        worldPosition = tp
                    };

                    grid[x, y] = n;
                    availableNodes.Add(n);
                    #region VISUAL
                    if(x % 2 != 0)
                    {
                        if(y % 2 != 0)
                        {
                            texture2D.SetPixel(x, y, color1);
                        }
                        else
                        {
                            texture2D.SetPixel(x, y, color2);
                        }
                    }
                    else
                    {
                        if(y % 2 != 0)
                        {
                            texture2D.SetPixel(x, y, color2);
                        }
                        else
                        {
                            texture2D.SetPixel(x, y, color1);
                        }
                    }
                    #endregion
                }

            }
            texture2D.filterMode = FilterMode.Point;
            texture2D.Apply();
            Rect rect = new Rect(0, 0, maxWidth, maxHeight);
            Sprite sprite = Sprite.Create(texture2D, rect, Vector2.zero, 1, 0, SpriteMeshType.FullRect);
            mapRenderer.sprite = sprite;
        }

        void PlacePlayer()
        {
            playerObj = new GameObject("Player");
            SpriteRenderer playerRender = playerObj.AddComponent<SpriteRenderer>();
            playerSprite = CreateSprite(playerColor);
            playerRender.sprite = playerSprite;
            playerRender.sortingOrder = 1;
            playerNode = GetNode(3, 3);
            playerObj.transform.position = playerNode.worldPosition;

            tailObj = new GameObject("TailParent");

        }
        void PlaceCamera()
        {
            Node n = GetNode(maxWidth / 2, maxHeight / 2);
            Vector3 p = n.worldPosition;
            p += Vector3.one * .5f;
            cameraHolder.position = p;
        }
        
        void CreateApple()
        {
            appleObj = new GameObject("Apple");
            SpriteRenderer appleRenderer = appleObj.AddComponent<SpriteRenderer>();
            appleRenderer.sprite = CreateSprite(appleColor);
            appleRenderer.sortingOrder = 1;
            RandomlyPlaceApple();
        }
        #endregion

        #region UPDATE
        private void Update()
        {
            GetInput();
            SetPlayerDirection();

            timer += Time.deltaTime;
            if(timer > moveRate)
            {
                timer = 0;
                MovePlayer();
            }
        }
        void GetInput()
        {
            up = Input.GetButtonDown("Up");
            down = Input.GetButtonDown("Down");
            right = Input.GetButtonDown("Right");
            left = Input.GetButtonDown("Left");
        }
        void SetPlayerDirection()
        {
            if(up)
            {
                curDirection = Direction.up;
            }
            else if(down)
            {
                curDirection = Direction.down;
            }
            else if(left)
            {
                curDirection = Direction.left;
            }
            else if(right)
            {
                curDirection = Direction.right;
            }
        }
        void MovePlayer()
        {
            int x = 0;
            int y = 0;

            switch (curDirection)
            {
                case Direction.up:
                    y = 1;
                    break;
                case Direction.down:
                    y = -1;
                    break;
                case Direction.left:
                    x = -1;
                    break;
                case Direction.right:
                    x = 1;
                    break;
      
            }

            Node targetNode = GetNode(playerNode.x + x, playerNode.y + y);
            if(targetNode == null)
            {
                //game over
            }
            else
            {
                bool isScore = false;
                if(targetNode == appleNode)
                {
                    isScore = true;
                }
                Node previousNode = playerNode;
                availableNodes.Add(previousNode);
                playerObj.transform.position = targetNode.worldPosition;
                playerNode = targetNode;
                availableNodes.Remove(playerNode);
                
                if(isScore)
                {
                    tailNode.Add(CreateTailNode(previousNode.x, previousNode.y));
                    availableNodes.Remove(previousNode);
                }
                MoveTail();

                if(isScore)
                {
                    if(availableNodes.Count > 0)
                    {
                        RandomlyPlaceApple();
                    }
                    else
                    {
                        //you won
                    }
                }
            }
        }

        void MoveTail()
        {
            Node prevNode = null;
            for(int i = 0; i < tailNode.Count; i++)
            {
                SpecialNode previous = tailNode[i];
                availableNodes.Add(previous.node);
                if(i == 0)
                {
                    prevNode = previous.node;
                    previous.node = playerNode;
                }
                else
                {
                    Node prev = previous.node;
                    previous.node = prevNode;
                    prevNode = prev;
                }
                availableNodes.Remove(previous.node);
                previous.obj.transform.position = previous.node.worldPosition;
            }
        }
        #endregion

        #region UTILITIES
        
        void RandomlyPlaceApple()
        {
            int ran = Random.Range(0, availableNodes.Count);
            Node n = availableNodes[ran];
            appleObj.transform.position = n.worldPosition;
            appleNode = n;
        }
        
        Node GetNode(int x, int y)
        {
            if(x < 0 || x > maxWidth - 1 || y < 0 || y > maxHeight - 1) return null;
            return grid [x, y];
        }

        SpecialNode CreateTailNode(int x, int y)
        {
            SpecialNode special = new SpecialNode();
            special.node = GetNode(x, y);
            special.obj = new GameObject();
            special.obj.transform.parent = tailObj.transform;
            special.obj.transform.position = special.node.worldPosition;
            SpriteRenderer renderer = special.obj.AddComponent<SpriteRenderer>();
            renderer.sprite = playerSprite;
            renderer.sortingOrder = 1;

            return special;
        }

        Sprite CreateSprite(Color targetColor)
        {
            Texture2D texture2D = new Texture2D(1, 1);
            texture2D.SetPixel(0,0, targetColor);
            texture2D.Apply();
            texture2D.filterMode = FilterMode.Point;
            Rect rect = new Rect(0, 0, 1, 1);
            return Sprite.Create(texture2D, rect, Vector2.zero, 1, 0, SpriteMeshType.FullRect);

        }
        #endregion
    }
}