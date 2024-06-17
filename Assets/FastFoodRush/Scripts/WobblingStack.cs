using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

namespace CryingSnow.FastFoodRush
{
    public class WobblingStack : MonoBehaviour
    {
        [SerializeField] private Vector2 rateRange = new Vector2(0.8f, 0.4f);
        [SerializeField] private float bendFactor = 0.1f;
        [SerializeField] private GameObject tray;

        public StackType StackType { get; private set; }
        public int Count => stack.Count;
        public int Height => height;

        private List<Transform> stack = new List<Transform>();
        private int height;

        private float stackOffset => RestaurantManager.Instance.GetStackOffset(StackType);

        Vector2 movement;

        void Update()
        {
            if (stack.Count == 0) return;

            movement.x = SimpleInput.GetAxis("Horizontal");
            movement.y = SimpleInput.GetAxis("Vertical");

            stack[0].transform.position = transform.position;
            stack[0].transform.rotation = transform.rotation;

            for (int i = 1; i < stack.Count; i++)
            {
                float rate = Mathf.Lerp(rateRange.x, rateRange.y, i / (float)stack.Count);
                stack[i].position = Vector3.Lerp(stack[i].position, stack[i - 1].position + (stack[i - 1].up * stackOffset), rate);

                stack[i].rotation = Quaternion.Lerp(stack[i].rotation, stack[i - 1].rotation, rate);
                if (movement != Vector2.zero) stack[i].rotation *= Quaternion.Euler(-i * bendFactor * rate, 0, 0);
            }
        }

        public void AddToStack(Transform child, StackType stackType)
        {
            if (stack.Count == 0)
            {
                StackType = stackType;
                tray.SetActive(true);
            }

            height++;
            Vector3 peakPoint = transform.position + Vector3.up * height * stackOffset;

            child.DOJump(peakPoint, 5f, 1, 0.3f)
                .OnComplete(() => stack.Add(child));
        }

        public Transform RemoveFromStack()
        {
            if (height == 0) return null;

            var lastChild = stack.LastOrDefault();
            lastChild.rotation = Quaternion.identity;

            stack.Remove(lastChild);
            height--;

            if (stack.Count == 0)
            {
                StackType = StackType.None;
                tray.SetActive(false);
            }

            return lastChild;
        }
    }

    public enum StackType { None, Food, Trash, Package }
}
