using System.Collections.Generic;
using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class ObjectPile : Interactable
    {
        [SerializeField] private StackType stackType;
        [SerializeField] private int length = 2;
        [SerializeField] private int width = 2;
        [SerializeField] private Vector3 spacing = new Vector3(0.5f, 0.1f, 0.5f);
        [SerializeField] private float dropInterval = 0.03f;

        public StackType StackType => stackType;
        public int Count => objects.Count;
        public Vector3 PeakPoint => transform.position + new Vector3(0, spacing.y * Count, 0);

        protected Stack<GameObject> objects = new Stack<GameObject>();
        private Vector3 pileCenter;

        private float dropTimer;

        void Awake()
        {
            pileCenter = new Vector3((length - 1) * spacing.x / 2f, 0f, (width - 1) * spacing.z / 2f);
        }

        protected virtual void Start()
        {
            spacing.y = RestaurantManager.Instance.GetStackOffset(stackType);
        }

        void Update()
        {
            if (player == null || objects.Count == 0) return;

            dropTimer += Time.deltaTime;

            if (dropTimer >= dropInterval)
            {
                dropTimer = 0f;
                Drop();
            }
        }

        protected virtual void Drop()
        {
            if (player.Stack.StackType == StackType.None || player.Stack.StackType == stackType)
            {
                if (player.Stack.Height < player.Capacity)
                {
                    var removedObj = objects.Pop();
                    player.Stack.AddToStack(removedObj.transform, stackType);

                    PlayObjectSound();
                }
            }
        }

        public void AddObject(GameObject obj)
        {
            objects.Push(obj);
            ArrangeAddedObject();
        }

        public void RemoveAndStackObject(WobblingStack stack)
        {
            var removedObj = objects.Pop();
            stack.AddToStack(removedObj.transform, stackType);
        }

        private void ArrangeAddedObject()
        {
            int lastIndex = objects.Count - 1;

            int row = (lastIndex / length) % width;
            int column = lastIndex % length;

            float xPos = column * spacing.x - pileCenter.x;
            float yPos = Mathf.FloorToInt(lastIndex / (length * width)) * spacing.y;
            float zPos = row * spacing.z - pileCenter.z;

            var latestObjectPushed = objects.Peek();
            latestObjectPushed.transform.position = transform.position + new Vector3(xPos, yPos, zPos);
        }

        private void PlayObjectSound()
        {
            switch (stackType)
            {
                case StackType.Food:
                case StackType.Package:
                    AudioManager.Instance.PlaySFX(AudioID.Pop);
                    break;

                case StackType.Trash:
                    AudioManager.Instance.PlaySFX(AudioID.Trash);
                    break;

                case StackType.None:
                default:
                    break;
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            pileCenter = new Vector3((length - 1) * spacing.x / 2f, 0f, (width - 1) * spacing.z / 2f);

            Gizmos.color = Color.yellow;
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Vector3 position = transform.position + new Vector3(i * spacing.x - pileCenter.x, spacing.y / 2f, j * spacing.z - pileCenter.z);
                    Gizmos.DrawWireCube(position, spacing);
                }
            }
        }
#endif
    }
}
