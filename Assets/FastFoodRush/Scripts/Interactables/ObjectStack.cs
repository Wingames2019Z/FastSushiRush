using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace CryingSnow.FastFoodRush
{
    public class ObjectStack : Interactable
    {
        [SerializeField] private StackType stackType;
        [SerializeField] private float stackInterval = 0.05f;

        public StackType StackType => stackType;
        public int MaxStack { get; set; }
        public int Count => objects.Count;
        public bool IsFull => Count >= MaxStack;

        private Stack<GameObject> objects = new Stack<GameObject>();
        private float stackOffset;
        private float stackTimer;

        void Start()
        {
            stackOffset = RestaurantManager.Instance.GetStackOffset(stackType);
        }

        void Update()
        {
            stackTimer += Time.deltaTime;

            if (stackTimer >= stackInterval)
            {
                stackTimer = 0f;

                if (player == null) return;
                if (player.Stack.StackType != stackType) return;
                if (player.Stack.Count == 0) return;

                if (objects.Count >= MaxStack) return;

                var objToStack = player.Stack.RemoveFromStack();
                if (objToStack == null) return;

                AddToStack(objToStack.gameObject);
                AudioManager.Instance.PlaySFX(AudioID.Pop);
            }
        }

        public void AddToStack(GameObject obj)
        {
            objects.Push(obj);

            var heightOffset = new Vector3(0f, (Count - 1) * stackOffset, 0f);
            Vector3 targetPos = transform.position + heightOffset;

            obj.transform.DOJump(targetPos, 5f, 1, 0.3f);
        }

        public Transform RemoveFromStack()
        {
            Transform removed = objects.Pop().transform;
            DOTween.Kill(removed);

            return removed;
        }
    }
}
